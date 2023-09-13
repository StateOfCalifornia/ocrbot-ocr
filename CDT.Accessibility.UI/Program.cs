/**
 * Copyright(C)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License along with this program. If not, see https://www.gnu.org/licenses/.
**/

using CDT.Accessibility.Documents;
using CDT.Acrobat;
using CDT.Data;
using CDT.Log;
using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using static CDT.OCR.Microsoft.Azure.CognitiveServicesConfiguration;

namespace CDT.Accessibility.UI {
	///TODO: convert static methods that should belong to an instance
	static class Program {
		#region Properties
		//threading
		private static Task CurrentTask = null;
		private static CancellationTokenSource CancelTokenSource;
		private static readonly Dispatcher UIDispatcher = Dispatcher.CurrentDispatcher;
		internal static readonly object Lock = new object(); //thread concurrency gatekeeper

		private static AcrobatClient Acrobat;
		private static MainForm MainForm;
		private const String defaultOutputLog = "output_[time].log";
		private static String OutputLog = null;

		private static string AccessibilityPageUrl = "https://cdt.ca.gov/accessibility/";
		private static string UserGuideUrl = "https://ocrbot.technology.ca.gov/UserGuide.pdf";
		private static string SourceCodeUrl = "https://ocrbot.technology.ca.gov/Download/OCRBot.zip";
		private static string ReleaseNotesUrl = "https://ocrbot.technology.ca.gov/docs/OCRBotReleaseNotes.pdf";
		private static string FAQsUrl = "https://ocrbot.technology.ca.gov/docs/FAQs.pdf";
		private static string PrivacyPolicyUrl = "https://cdt.ca.gov/privacy-policy/";
		private static string DataFlowUrl = "https://ocrbot.technology.ca.gov/docs/OCRBotFlowDiagram.pdf";

		//ToDo: need to add post build step to validate this resource path
		//      could fail at runtime if file moved or Namespace changed
		private const string ITextLicenseResourceName = "CDT.Accessibility.UI.Resources.iText.iTextLicense.xml";

		//pass through read (evaluate every time)
		public static string LibraryInfo => ScannedDocumentRemediation.LibraryInfo();
		public static string AcrobatVersion => Acrobat.AcrobatVersion;
		public static bool IsAutoTagAvailable => Acrobat.IsAutoTagAvailable;
		public static bool IsReOCRAvailable => Acrobat.IsReOCRAvailable;
		public static int AcrobatMajorVersion => Acrobat.AcrobatMajorVersion;

		//read-only; evaluate just once
		public static string ApplicationDataDirectory { get; } = ApplicationDeployment.IsNetworkDeployed ?  ApplicationDeployment.CurrentDeployment.DataDirectory : AppDomain.CurrentDomain.BaseDirectory;
		public static string OCRBotSettingsFile { get; } = Path.Combine(ApplicationDataDirectory, @"Resources\Settings\OCRBotSettings.json");
		public static SessionConfig OCRBotSettings { get; } = SessionConfig.Load(OCRBotSettingsFile);
		public static Stats SessionStats { get; }  = new Data.Stats();


		public static string ApplicationVersion {
			get {
				string version;

				if (ApplicationDeployment.IsNetworkDeployed) {
					version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4);
				}
				else {
					version = Application.ProductVersion;
				}

				return version;
			}
		}
		#endregion Properties



		#region #UAT_PRE
		[Conditional("UAT_PRE")]
		private static void If_UAT_PreRelease() {
			UserGuideUrl = "https://testocrbot.technology.ca.gov/UserGuide.pdf";
			ReleaseNotesUrl = "https://testocrbot.technology.ca.gov/docs/OCRBotReleaseNotes.pdf";
			DataFlowUrl = "https://testocrbot.technology.ca.gov/docs/OCRBotFlowDiagram.pdf";
			SourceCodeUrl = "https://testocrbot.technology.ca.gov/Download/OCRBot.zip";
			FAQsUrl = "https://testocrbot.technology.ca.gov/docs/FAQs.pdf";
		}
		#endregion #UAT_PRE


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			InitDependencies();

			//suppress GUI if parameters given in the command line
			if (args?.Length > 0) {
				//Console mode
				ConsoleApp.RunWithOptions(args);
			}
			else {
				//GUI mode
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				If_UAT_PreRelease();

				MainForm = new MainForm();
				MainForm.Show();
				
				ShowMissingDependenciesWarning();

				MainForm.SetFormState(); //toggle installed Acrobat features

				Application.Run(MainForm);
			}
		}


		static void InitDependencies() {
			Acrobat = new AcrobatClient();
			var changesMade = Installer.Install();

			if (changesMade) {
				//renew client to pickup dependency changes
				FeatureDependency.ClearMissingExternalDependency();
				Acrobat = new AcrobatClient();
			}
		}



		/// <summary>
		/// Run OCR process
		/// </summary>
		/// <param name="inputDirectory"></param>
		/// <param name="selectedFiles"></param>
		/// <param name="outputDirectory"></param>
		/// <param name="autoTag"></param>
		internal static async void ProcessAsync(SessionConfig sessionConfig) {
			try {
				//only allow one batch job at a time to avoid duplication and other concurrency issues
				//any parallelization desired should be built into the single batch rather than running multiple concurrent instances
				if (HasRunningTask()) {
					return;
				}


				//enqueue cancellable background process and immediately return
				CancelTokenSource = new CancellationTokenSource();
				sessionConfig.CancelToken = CancelTokenSource.Token;

				CurrentTask = Task.Run(
					() => Process(sessionConfig, LogMessage),
					sessionConfig.CancelToken
				);

				await CurrentTask;
			}
			catch (Exception ex) {
				HandleBatchProcessingException(ex);
			}
			// async specific housekeeping
			finally {
				// log status
				LogTaskStatus(CurrentTask, "ProcessAsync");

				// release
				CurrentTask?.Dispose();
				CurrentTask = null;
				CancelTokenSource?.Dispose();
				CancelTokenSource = null;


				// shared completion workflow
				// keep all other process complete workflow here
				sessionConfig.OnTaskComplete?.Invoke();
			}
		}

		/// <summary>
		/// synchronous process wrapper
		/// </summary>
		internal static void Process(SessionConfig sessionConfig, Action<LogEntry> logMessage) {
			try {
				SaveOCRBotSettings(sessionConfig);
				UpdateLogName(sessionConfig.OutputDirectory);
				LogIfApplicationStatusWarning("Form");

				SessionStats.Start();

				Stream iTextLicenseStream = null;
				var serviceConfig = new ClientConfiguration(OCRBotSettings.SubscriptionKey, OCRBotSettings.ServiceEndpoint);
				var client = new ScannedDocumentRemediation(serviceConfig, SessionStats, Acrobat, logMessage, iTextLicenseStream);

				if (sessionConfig.InputDirectory != null) {
					client.ProcessFolder(sessionConfig);
				}
				else {
					client.ProcessFiles(sessionConfig);
				}
			}
			catch (Exception ex) {
				sessionConfig.CancelToken.ThrowIfCancellationRequested();

				if (HasRunningTask()){
					throw; //defer to async wrapper
				}
				else {
					HandleBatchProcessingException(ex);
				}
			}
			finally {
				SessionStats.Stop();

				//UI state driven off of background task status so
				//defer handling to asyc wrapper if running in background task
				if (!HasRunningTask()) {
					sessionConfig.OnTaskComplete?.Invoke();
				}
			}
		}



		internal static void SaveOCRBotSettings(SessionConfig config) {
			config.Save(OCRBotSettingsFile);
		}

		internal static void LogIfApplicationStatusWarning(string source) {
			if (!IsReOCRAvailable || !IsAutoTagAvailable) {
				LogMessage(
					new LogEntry(
						LogLevel.Warning,
						source,
						$" Advanced Features Available:[{IsReOCRAvailable}] AutoTagAvailable:[{IsAutoTagAvailable}] Acrobat version:[{AcrobatVersion}]"));
			}
		}

		private static void HandleBatchProcessingException(Exception exception) {
			//aggregate exception thrown from Tasks
			if (exception is AggregateException) {
				var ae = exception as AggregateException;

				//This is the top level handler, so don't rethrow any exceptions
				ae.Handle(ex => { HandleBatchProcessingException(ex); return true; });
			}
			//non-aggregated exception
			else {
				//job cancelled warning
				if (exception is OperationCanceledException) {
					//will be log by task closeout
					//LogMessage(new LogEntry(LogLevel.Warning, "User", "Job Cancelled"));
				}
				//all other exceptions
				else {
					SessionStats.FileErrorCount++;
					LogMessage(new LogEntry(exception));
				}
			}
		}
		
		internal static void OpenWebPage(string url) {
			System.Diagnostics.Process.Start(url);
		}

		internal static void GetSourceCode() {
			OpenWebPage(SourceCodeUrl);
		}

		internal static void OpenHelp() {
			OpenWebPage(UserGuideUrl);
		}
		internal static void OpenAccessibilityPage() {
			OpenWebPage(AccessibilityPageUrl);
		}

		internal static void OpenReleaseNotes() {
			OpenWebPage(ReleaseNotesUrl);
		}
		internal static void OpenFAQs() {
			OpenWebPage(FAQsUrl);
		}

		internal static void OpenPrivacyPolicy() {
			OpenWebPage(PrivacyPolicyUrl);
		}

		internal static void OpenDataFlow() {
			OpenWebPage(DataFlowUrl);
		}

		/// <summary>
		/// signal gracefull shutdown of running task
		/// </summary>
		internal static void Cancel() {
			if (CancelTokenSource.Token.CanBeCanceled) {
				CancelTokenSource.Cancel();
			}
		}

		public static bool IsCancelling() =>
			(CancelTokenSource?.IsCancellationRequested).GetValueOrDefault();


		internal static bool HasRunningTask() {
			lock (Lock) {
				bool hasTask = CurrentTask != null;

				if (CurrentTask != null) {
					switch (CurrentTask.Status) {
						case TaskStatus.Created:
						case TaskStatus.WaitingForActivation:
						case TaskStatus.WaitingToRun:
						case TaskStatus.Running:
						case TaskStatus.WaitingForChildrenToComplete:
							hasTask = true;
							break;

						case TaskStatus.RanToCompletion:
						case TaskStatus.Canceled:
						case TaskStatus.Faulted:
							hasTask = false;
							break;
					}
				}

				return hasTask;
			}
		}

		/// <summary>
		/// log the current state of a task
		/// </summary>
		private static void LogTaskStatus(Task task, string source) {
			string status;
			LogLevel logLevel;

			if (task == null) {
				status = "Not Started";
				logLevel = LogLevel.Informational;
			}
			else if (task.IsCanceled) {
				status = "Cancelled";
				source = "User";
				logLevel = LogLevel.Warning;
			}
			else if (task.IsFaulted) {
				status = "Failed";
				logLevel = LogLevel.Error;
			}
			else if (SessionStats.FileErrorCount > 0) {
				status = "Completed with errors";
				logLevel = LogLevel.Warning;
			}
			//ask last - returns true when faulted or cancelled
			else if (task.IsCompleted) {
				status = "Complete";
				logLevel = LogLevel.Informational;
			}
			else {
				status = $"{task.Status}";
				logLevel = LogLevel.Informational;
			}

			SessionStats.StatusMessage = $"Task {status}";
			LogMessage(new LogEntry(logLevel, source, SessionStats.ToString()));
		}

		internal static void UpdateLogName(string outputFolder) {
			string timestamp = DateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.CurrentCulture.DateTimeFormat);
			string fileName = defaultOutputLog.Replace("[time]", timestamp);
			OutputLog = Path.Combine(outputFolder, fileName);
		}

		internal static void LogMessage(LogEntry logEntry) {
			//append to form's message log
			if (MainForm != null) {
				UIDispatcher.Invoke(() => {
					MainForm.AppendStatusMessage(logEntry);
				});
			}

			//write to log file
			if (OutputLog != null) {
				var displayExceptionDetail = true;
				File.AppendAllText(OutputLog, $"{logEntry.ToString(displayExceptionDetail)}{Environment.NewLine}");
			}
		}

		private static void ShowMissingDependenciesWarning() {
			if (FeatureDependency.MissingExternalDependenciesCount() == 0) {
				return;
			}

			var title = $"{Application.ProductName} - Enhanced Functionality Availability";

			string msg = 
				FeatureDependency.FormatMissingExternalDependencies();

			MessageBox.Show(MainForm, msg, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
	}
}
