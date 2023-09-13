using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.IO;
using CDT.Accessibility.Documents;
using CDT.Log;
using CDT.Helpers;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Windows.Threading;
using System.Globalization;

namespace CDT.Accessibility.UI {
	public partial class MainForm : Form {
		private static string BaseTitle { get; set; } = Application.ProductName;
		private SessionConfig Session { get; } = Program.OCRBotSettings;

		private string InputDirectory {
			get => Session.InputDirectory;
			set => Session.InputDirectory = value;
		}

		private string OutputDirectory {
			get => Session.OutputDirectory;
			set => Session.OutputDirectory = value;
		}

		private IEnumerable<string> SelectedFiles {
			get => Session.InputFiles;
			set => Session.InputFiles = value;
		}

		private bool DoAutoTag {
			get => Session.DoAutoTag;
			set => Session.DoAutoTag = value;
		}

		private bool DoTextOnly {
			get => Session.ShowTextOnly;
			set => Session.ShowTextOnly = value;
		}

        private bool DoConvertCMYK
        {
            get => Session.DoConvertCMYK;
            set => Session.DoConvertCMYK = value;
        }


        private DataTable TableSource;
		private bool sortAscending = false;


		private bool CanChangeOptions { get; set; }


		//set form state based on user selections and background task status
		internal enum FormState {
			EndpointNotSet,
			NotReady,
			Ready,
			Running,
			Cancelling
		}


		#region #UAT_PRE
		[Conditional("UAT_PRE")]
		private void If_UAT_PreRelease() {
			//set pre-release visual indicators
			BaseTitle = $"Test Test Test Test Test Test Test {BaseTitle}";
			this.BackColor = Color.LightGoldenrodYellow;
		}
		#endregion #UAT_PRE


		public MainForm() {
			InitializeForm();
		}

		/// <summary>
		/// load controls, do initial layout
		/// </summary>
		private void InitializeForm() {
			//run generated designer first
			InitializeComponent();

			//now apply extended layout and bindings

			//set default app info
			this.Text = Application.ProductName;
			//set pre-release overrides
			If_UAT_PreRelease();


			//load user settings and service config
			BindSessionConfig();

			//log window
			BindLogData();

			//tool tips
			SetToolTips();

			//initialize form state and available options
			SetFormState();
		}

		private void BindLogData() {
			//data source
			this.TableSource = DataTableHelper.ConvertToDataTable<LogEntry>(new LogEntry[0]);
			dataGridViewLog.DataSource = TableSource;

			//ensure autosize disabled before manually setting col widths
			dataGridViewLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

			//TimeStamp
			dataGridViewLog.Columns[0].HeaderText = "TimeStamp";
			dataGridViewLog.Columns[0].Width = 110;
			//Log Level
			dataGridViewLog.Columns[1].HeaderText = "Log Level";
			dataGridViewLog.Columns[1].Width = 85;
			//Source
			dataGridViewLog.Columns[2].HeaderText = "Source";
			dataGridViewLog.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewLog.Columns[2].FillWeight = 100;
			dataGridViewLog.Columns[2].MinimumWidth = 100;
			//Message
			dataGridViewLog.Columns[3].HeaderText = "Message";
			dataGridViewLog.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewLog.Columns[3].FillWeight = 200;
			dataGridViewLog.Columns[2].MinimumWidth = 100;
			//Exception
			dataGridViewLog.Columns[4].Visible = false;
		}

		private void SetToolTips() {
			this.toolTip1.SetToolTip(this.browseInputDirectoryButton,
				"Select Parent Folder to Process.\r\nIncludes all scanned documents found and this and all subfolders" +
					$"\r\n({ScannedDocumentRemediation.SupportedFileTypesFilterDescription}).");

			this.toolTip1.SetToolTip(this.browseInputFileButton, "Select one or more scanned document files to be processed");

			this.toolTip1.SetToolTip(this.browseOutputDirectoryButton,
				"Select the folder where processed documents will be saved.\r\nAny existing files will not be overwritten.");

			this.toolTip1.SetToolTip(this.checkBoxAutoTag,
				"Acrobat Autotag the documents.\r\n" +
				"Must have Adobe® Acrobat® with Evermap AutoBatch™ Plug-in installed. \r\n" +
				"See User Guide for more information");

			this.toolTip1.SetToolTip(this.checkBoxShowTextOnly,
				"By default, only the text will be visible.\r\n" +
				"Must have Adobe® Acrobat® DC (2019.021.20058 or higher) with Evermap AutoBatch™ Plug-in. \r\n" +
				"See User Guide for more information");

			this.toolTip1.ShowAlways = true;
		}

		private string GetSelectedInputDirectory () {
			string dir;

			if (!String.IsNullOrWhiteSpace(InputDirectory)) {
				dir = InputDirectory;
			}
			else {
				dir = Path.GetDirectoryName(SelectedFiles?.FirstOrDefault());
			}

			return dir;
		}

		/// <summary>
		/// format displayed text of selected input folder/file(s)
		/// </summary>
		private string GetSelectedInputLabel() {
			string selection;
			var count = SelectedFiles?.Count() ?? 0;

			//single file - show full path and file count
			if (count == 1) {
				selection = $"{SelectedFiles.First()}\n(1 file selected)";
			}
			//multiple files - show dir and file count
			else if (count > 1) {
				selection = $"{GetSelectedInputDirectory()}\n({count} files selected)";
			}
			//show selected folder
			else if (!String.IsNullOrWhiteSpace(InputDirectory)) {
				selection = $"{InputDirectory}\n(folder selected)";
			}
			//nothing selected
			else {
				selection = null;
			}

			return selection;
		}

		private void BindSessionConfig() {
			SetUserFields();

			SetEvents();
		}

		private void SetUserFields() {
            //load user preferences
            //HACK: one way binding, so will need manual reload if session changes
            checkBoxAutoTag.Checked = DoAutoTag;
            checkBoxConvertCMYK.Checked = DoConvertCMYK;
            checkBoxShowTextOnly.Checked = DoTextOnly;
			lblSelectedInputDirectory.Text = GetSelectedInputLabel();
			lblSelectedOutputDirectory.Text = OutputDirectory;
		}

		private void SetEvents() {
			//events - should only be bound once!
			this.checkBoxAutoTag.CheckedChanged += new System.EventHandler(this.checkBoxAutoTag_CheckedChanged);
			this.checkBoxShowTextOnly.CheckedChanged += new System.EventHandler(this.checkBoxShowTextOnly_CheckedChanged);

			Session.OnTaskComplete = TaskCompleted;
		}

		private DirectoryInfo ShowFolderDialog(string initialDirectory = null) {
			DirectoryInfo folder = CDT.Windows.WinForms.Dialogs.ShowFolderDialog(initialDirectory);
			return folder;
		}

		private void OcrServiceEndpointToolStripMenuItem_Click(object sender, EventArgs e) {
			using (var ocrbotSettings = new OCRBotSettingsForm(Session)) {
				ocrbotSettings.ShowDialog();
				SetFormState();
			}
		}


		#region Button Events
		private void BrowseInputDirectoryButton_Click(object sender, EventArgs e) {
			var initialDirectory = GetSelectedInputDirectory() ?? OutputDirectory;
			var folder = ShowFolderDialog(initialDirectory);

			if (folder != null) {
				SelectedFiles = null;
				InputDirectory = folder.FullName;
				lblSelectedInputDirectory.Text = InputDirectory;
			}

			SetFormState();
		}

		private void BrowseOutputDirectoryButton_Click(object sender, EventArgs e) {
			var initialDirectory = OutputDirectory ?? GetSelectedInputDirectory();
			var folder = ShowFolderDialog(OutputDirectory);

			if (folder != null) {
				OutputDirectory = folder.FullName;
				lblSelectedOutputDirectory.Text = OutputDirectory;
			}

			SetFormState();
		}

		private void browseInputFileButton_Click(object sender, EventArgs e) {
			openFileDialog.Filter = $"Scanned Documents ({ScannedDocumentRemediation.SupportedFileTypesFilterDescription})|{ScannedDocumentRemediation.SupportedFileTypesFilter}";
			openFileDialog.InitialDirectory = GetSelectedInputDirectory() ?? OutputDirectory;

			DialogResult result = openFileDialog.ShowDialog();

			if (result == DialogResult.OK) {
				string[] files = openFileDialog.FileNames;
				InputDirectory = null;
				SelectedFiles = files;
				lblSelectedInputDirectory.Text = files.Length > 1 ? files[0] + "..." : files[0];
			}

			SetFormState();
		}

		private void BtnSubmit_Click(object sender, EventArgs e) {
			var state = GetFormState();

			//begin processing
			if (state == FormState.Ready) {
				SetFormState(FormState.Running);
				statsTimer.Start();
				Program.ProcessAsync(Session);
			}
			//request cancel
			else if (state == FormState.Running) {
				SetFormState(FormState.Cancelling);
				Program.Cancel();
			}
			//how'd they get here?
			else {
				//resync form, ensure valid state
				SetFormState();
			}
		}

		/// <summary>
		/// closeout current run
		/// </summary>
		private void TaskCompleted() {
			statsTimer.Stop();
			SetFormState();

			//display summary
			MessageBox.Show(Program.SessionStats.ToString(),
				Program.SessionStats.StatusMessage,
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		private void userGuideToolStripMenuItem_Click(object sender, EventArgs e) {
			Program.OpenHelp();
		}

		private void releaseNotesToolStripMenuItem_Click(object sender, EventArgs e) {
			Program.OpenReleaseNotes();
		}

		private void privacyPolicyToolStripMenuItem_Click(object sender, EventArgs e) {
			Program.OpenPrivacyPolicy();
		}

		private void contactUsToolStripMenuItem_Click(object sender, EventArgs e) {
			var aboutBox = new AboutBox();
			aboutBox.Show();
		}

		private void fAQsToolStripMenuItem_Click(object sender, EventArgs e) {
			Program.OpenFAQs();
		}

		private void dataFlowToolStripMenuItem_Click(object sender, EventArgs e) {
			Program.OpenDataFlow();
		}

		private void btnClearLog_Click(object sender, EventArgs e) {
			DataTable dt = (dataGridViewLog.DataSource as DataTable);
			dt.Rows.Clear();
			UpdateChkFilterTexts();
		}
		#endregion Button Events


		#region State Machine
		private FormState GetFormState() {
			lock (Program.Lock) {
				FormState state;

				if (Program.IsCancelling()) {
					state = FormState.Cancelling;
				}
				else if (Program.HasRunningTask()) {
					state = FormState.Running;
				}
				else if (!Program.OCRBotSettings.IsSet()) {
					state = FormState.EndpointNotSet;
				}
				else {
					var sourceSelected = !String.IsNullOrEmpty(InputDirectory) || SelectedFiles != null;
					var outputSelected = !String.IsNullOrEmpty(OutputDirectory);

					if (sourceSelected && outputSelected) {
						state = FormState.Ready;
					}
					else {
						state = FormState.NotReady;
					}
				}

				return state;
			}
		}

		internal void SetFormState() {
			var state = GetFormState();
			SetFormState(state);
		}

		private void SetFormState(FormState state) {
			//ToDo: State is has become too complex.  Form needs a proper binder.
			lock (Program.Lock) {
				switch (state) {
					case FormState.Ready:
						this.Text = BaseTitle;
						btnSubmit.Text = "Submit";
						btnSubmit.Enabled = true;
						CanChangeOptions = true;
						labelProcessing.Text = "Ready";
						break;

					case FormState.Running:
						this.Text = $"{BaseTitle} - Processing";
						btnSubmit.Text = "Cancel";
						btnSubmit.Enabled = true;
						CanChangeOptions = false;
						labelProcessing.Text = "Processing...";
						break;

					case FormState.Cancelling:
						this.Text = $"{BaseTitle} - Cancelling";
						btnSubmit.Text = "Cancelling...";
						btnSubmit.Enabled = false;
						CanChangeOptions = false;
						break;

					case FormState.EndpointNotSet:
						labelProcessing.Text = "Missing OCR Service Endpoint";
						this.Text = BaseTitle;
						btnSubmit.Enabled = false;
						CanChangeOptions = true;
						break;

					case FormState.NotReady:
					default:
						this.Text = BaseTitle;
						btnSubmit.Enabled = false;
						CanChangeOptions = true;
						labelProcessing.Text = "Not Ready";
						break;
				}


				//input and options UI
				SetUserFields();
				//Document Selection
				browseInputDirectoryButton.Enabled = CanChangeOptions;
				browseInputFileButton.Enabled = CanChangeOptions;
				browseOutputDirectoryButton.Enabled = CanChangeOptions;
				//processing feature toggles
				SoftToggleOptionUI(checkBoxAutoTag, Program.IsAutoTagAvailable);
				SoftToggleOptionUI(checkBoxShowTextOnly, Program.IsReOCRAvailable);

				UpdateStats();
			}
		}

		private void SoftToggleOptionUI (Control control, bool isEnabled) {
			control.ForeColor = isEnabled ? SystemColors.ControlText : SystemColors.ControlDark;
		}

		private void UpdateStats() {
			labelFileErrorCount.Text = $"{Program.SessionStats.FileErrorCount}";
			labelFilesProcessed.Text = $"{Program.SessionStats.FileProcessedCount}";
			labelPagesProcessed.Text = $"{Program.SessionStats.PagesProcessedCount}";
			labelTime.Text = Program.SessionStats.ToTimeString();
		}

		private void checkBoxShowTextOnly_CheckedChanged(object sender, EventArgs e) {
			if (CanChangeOptions && Program.IsReOCRAvailable) {
				DoTextOnly = checkBoxShowTextOnly.Checked;
			}
			else {
				checkBoxShowTextOnly.Checked = DoTextOnly;
			}
		}

		private void checkBoxAutoTag_CheckedChanged(object sender, EventArgs e) {
			if (CanChangeOptions && Program.IsAutoTagAvailable) {
				DoAutoTag = checkBoxAutoTag.Checked;
			}
			else {
				checkBoxAutoTag.Checked = DoAutoTag;
			}
		}
		#endregion State Machine


		#region Log Window
		internal void AppendStatusMessage(LogEntry logEntry) {
			object[] parms = {
				logEntry.TimeStamp,
				logEntry.LogLevel,
				logEntry.Source,
				logEntry.Message
			};

			TableSource.Rows.Add(parms);
			UpdateChkFilterTexts();
		}


		private void dataGridViewLog_ColumnHeaderMouseClick(
				object sender, DataGridViewCellMouseEventArgs e) {
			// logBindingSource.DataSource;// = logBindingSource.List.derByDescending(o => o.GetType().GetProperty(MyDataGridView.Columns[iColNumber].Name).GetValue(o));
			if (sortAscending) {
				dataGridViewLog.Sort(dataGridViewLog.Columns[e.ColumnIndex], ListSortDirection.Descending);
			}
			else {
				dataGridViewLog.Sort(dataGridViewLog.Columns[e.ColumnIndex], ListSortDirection.Ascending);
			}

			sortAscending = !sortAscending;
		}


		private void FilterChange() {
			string rowFilter = $"[logLevel] = '{-1}'";
			DataTable dt = (dataGridViewLog.DataSource as DataTable);

			if (chkInformationFilter.Checked) {
				rowFilter += $" OR [logLevel] = '{Convert.ToInt32(LogLevel.Informational)}'";
			}

			if (chkWarningFilter.Checked) {
				rowFilter += $" OR [logLevel] = '{Convert.ToInt32(LogLevel.Warning)}'";
			}

			if (chkErrorFilter.Checked) {
				rowFilter += $" OR [logLevel] = '{Convert.ToInt32(LogLevel.Error)}'";
			}

			dt.DefaultView.RowFilter = rowFilter;
			UpdateChkFilterTexts();
		}


		private void UpdateChkFilterTexts() {
			DataTable dt = (dataGridViewLog.DataSource as DataTable);

			//info
			chkInformationFilter.FlatStyle = chkInformationFilter.Checked ? FlatStyle.Popup : FlatStyle.Standard;

			chkInformationFilter.Text = String.Format("{0} {1}",
				dt.Select($"[logLevel] = '{Convert.ToInt32(LogLevel.Informational)}'").Length,
				LogLevel.Informational);

			//warning
			chkWarningFilter.FlatStyle = chkWarningFilter.Checked ? FlatStyle.Popup : FlatStyle.Standard;

			chkWarningFilter.Text = String.Format("{0} {1}",
				dt.Select($"[logLevel] = '{Convert.ToInt32(LogLevel.Warning)}'").Length,
				LogLevel.Warning);

			//error
			chkErrorFilter.FlatStyle = chkErrorFilter.Checked ? FlatStyle.Popup : FlatStyle.Standard;

			chkErrorFilter.Text = String.Format("{0} {1}",
				dt.Select($"[logLevel] = '{Convert.ToInt32(LogLevel.Error)}'").Length,
				LogLevel.Error);
		}

		private void LogFilter_CheckedChanged(object sender, EventArgs e) {
			FilterChange();
		}
		#endregion Log Window

		private void statsTimer_Tick(object sender, EventArgs e) {
			UpdateStats();
		}

        private void checkBoxConvertCMYK_CheckedChanged(object sender, EventArgs e)
        {
            if (CanChangeOptions && Program.IsAutoTagAvailable)
            {
                DoConvertCMYK = checkBoxConvertCMYK.Checked;
            }
            else
            {
                checkBoxConvertCMYK.Checked = DoConvertCMYK;
            }

        }
    }
}
