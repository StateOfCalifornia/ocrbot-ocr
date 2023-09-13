using CDT.Helpers;
using CDT.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace CDT.Acrobat {
	public class AcrobatClient {
		//AutoBatch Actions
		private AutoBatchConfig AutoBatch;
		private PreFlight AcrobatPreFlight;
		private AutoBatchAction AutoTagConfig;
		private AutoBatchAction ForceAutoTagConfig;
		private AutoBatchAction AddLayerConfig;
		private AutoBatchAction RemoveAllTextConfig;
		private AutoBatchAction RemoveInvisibleTextConfig;
        private AutoBatchAction TextOnlyVisibleConfig;
        private AutoBatchAction ConvertCMYKConfig;


        internal bool IsAutoBatchAvailable => AutoBatch.IsAutoBatchAvailable;

		public bool IsAutoTagAvailable => AutoTagConfig.IsCommandAvailable;
		public bool IsReOCRAvailable => AcrobatPreFlight.IsReOCRAvailable;

		/// <summary>
		/// the full reported version of Acrobat that is installed
		/// </summary>
		public string AcrobatVersion { get; private set; }

		/// <summary>
		/// the major reported version of Acrobat that is installed
		/// </summary>
		public int AcrobatMajorVersion { get; private set; }


		public AcrobatClient() {
			SetVersion();
			InitFeatures();
		}

		private void SetVersion() {
			//full version
			AcrobatVersion = RegistryHelper.GetInstalledVersion("Adobe Acrobat DC");
            if (String.IsNullOrWhiteSpace(AcrobatVersion))
            {
				AcrobatVersion = RegistryHelper.GetInstalledVersion("Adobe Acrobat");
				if (String.IsNullOrWhiteSpace(AcrobatVersion))
				{
                    AcrobatVersion = RegistryHelper.GetInstalledVersion("Adobe Acrobat (64-bit)", 1);
                }
			}

			if (String.IsNullOrEmpty(AcrobatVersion)) {
				AcrobatVersion = "not detected";
				AcrobatMajorVersion = 0;
			}
			else {
				//major version #
				var part = AcrobatVersion?.Split('.')[0];
				int.TryParse(part, out var majorVersion);

				AcrobatMajorVersion = majorVersion;
			}
		}

		private void InitFeatures() {
			AutoBatch = new AutoBatchConfig(this);
			AcrobatPreFlight = new PreFlight(this);
            AutoTagConfig = new AutoBatchAction(AutoBatch, "OCRBot AutoTag");
            ConvertCMYKConfig = new AutoBatchAction(AutoBatch, "OCRBot ConvertCMYK");
            ForceAutoTagConfig = new AutoBatchAction (AutoBatch, "OCRBot Force AutoTag");
			AddLayerConfig = new AutoBatchAction (AutoBatch, "OCRBot Make Visibility Layers");
			RemoveAllTextConfig = new AutoBatchAction (AutoBatch, "OCRBot Remove Text");
			RemoveInvisibleTextConfig = new AutoBatchAction (AutoBatch, "OCRBot Remove Invisible Text");
			TextOnlyVisibleConfig = new AutoBatchAction (AutoBatch, "OCRBot Make TextOnly Visible");
		}


		/// <summary>
		/// autotag a document using the extended Re-OCR workflow; requires the CDT task library
		/// <para>existing markup will be replaced</para>
		/// </summary>
		public void ForceAutotagDocument(string documentPath, CancellationToken cancelToken)
				=> ExecuteBatch(ForceAutoTagConfig, documentPath, documentPath, cancelToken);

		/// <summary>
		/// autotag a document using the older basic workflow; calls the standard Adobe PreFlight task
		/// <para>existing markup will be replaced</para>
		/// </summary>
		public void AutotagDocument(string documentPath, CancellationToken cancelToken)
				=> ExecuteBatch(AutoTagConfig, documentPath, documentPath, cancelToken);

		/// <summary>
		/// remove all text content from the document; images will be left intact
		/// </summary>
		/// <param name="documentSource"></param>
		/// <param name="documentTarget"></param>
		/// <param name="cancelToken"></param>
		public void RemoveAllText(string documentSource, string documentTarget, CancellationToken cancelToken)
				=> ExecuteBatch(RemoveAllTextConfig, documentSource, documentTarget, cancelToken);

		/// <summary>
		/// delete all document text that is hidden/invisible;
		/// e.g. the hidden OCR layer, while leaving the rest of the content intact
		/// </summary>
		public void RemoveInvisibleText(string documentSource, string documentTarget, CancellationToken cancelToken)
				=> ExecuteBatch(RemoveInvisibleTextConfig, documentSource, documentTarget, cancelToken);

		/// <summary>
		/// split document into like-content layers (e.g. images, text, hidden ocr text)
		/// </summary>
		public void AddLayers(string documentPath, CancellationToken cancelToken)
				=> ExecuteBatch(AddLayerConfig, documentPath, documentPath, cancelToken);

        /// <summary>
        /// hide images and make hidden OCR text visible
        /// </summary>
        /// <param name="documentPath"></param>
        /// <param name="cancelToken"></param>
        public void ShowTextOnly(string documentPath, CancellationToken cancelToken)
                => ExecuteBatch(TextOnlyVisibleConfig, documentPath, documentPath, cancelToken);

        /// <summary>
        /// convert cmyke
        /// </summary>
        /// <param name="documentPath"></param>
        /// <param name="cancelToken"></param>
        public void ConvertCMYK(string documentSource, string documentTarget, CancellationToken cancelToken)
                => ExecuteBatch(ConvertCMYKConfig, documentSource, documentTarget, cancelToken);


        /// <summary>
        /// run an AutoBatch script
        /// </summary>
        private void ExecuteBatch(AutoBatchAction action, string inputFile, string outputFile, CancellationToken cancelToken) {
			cancelToken.ThrowIfCancellationRequested();
			var args = action.FormatCommandArgs(inputFile, outputFile);

			ExecuteBatch(action.ShellCommand, args, inputFile, outputFile, action.OnSuccessFolder, action.WorkingSequenceFile);
		}

		/// <summary>
		/// run an AutoBatch script
		/// </summary>
		/// <param name="shellCommand">command path to be executed</param>
		/// <param name="args">shell command arguments</param>
		/// <param name="inputFile">document to be processed</param>
		/// <param name="outputFile">where document should be saved upon completion</param>
		/// <param name="onSuccessFolder">temp folder to use until workflow completed, then will be moved to <paramref name="outputFile"/></param>
		/// <param name="sequenceFile">writable instance of the sequence file to use in the workflow</param>
		private void ExecuteBatch(string shellCommand, string args, string inputFile, string outputFile, string onSuccessFolder, string sequenceFile) {
			//If execution results in new target file, then lets delete target first.
			//this makes testing for success easier
			DeleteTargetIffDiff(inputFile, outputFile);

			string onSuccessFile = (onSuccessFolder == null) ? null
				: Path.Combine(onSuccessFolder, Path.GetFileName(inputFile));

			// clean up successFile before execution
			if (onSuccessFile != null) {
				File.Delete(onSuccessFile);
			}

			//HACK: plugin always returns the same exit code,
			//so cache file date before calling plugin as a baseline
			//to compare against for confirming that autotag was successful
			var fiPre = File.GetLastWriteTime(outputFile);
			ExecuteCommand(shellCommand, args);

			bool haveError = false;

			if (onSuccessFile == null) {
				MoveTargetIffDiff(inputFile, outputFile);
				var fiPost = File.GetLastWriteTime(outputFile);
				var isUpdated = (fiPost > fiPre);
				haveError = !isUpdated;
			}
			else {
				haveError = !File.Exists(onSuccessFile);
				FileHelper.MoveFileForce(onSuccessFile, outputFile);
			}

			Directory.Delete(Path.GetDirectoryName(sequenceFile), true);

			if (haveError) {
				throw new ArgumentException(shellCommand);
			}
		}

		private void DeleteTargetIffDiff(string documentSource, string documentTarget) {
			//If execution results in new target file, then lets delete target first.
			//this makes testing for success easier
			if (!FileHelper.AreSameFolder(documentSource, documentTarget)) {
				File.Delete(documentTarget);
				var acrobatOutputFile = Path.Combine(Path.GetDirectoryName(documentTarget), Path.GetFileName(documentSource));
				File.Delete(acrobatOutputFile);
			}
		}

		private void MoveTargetIffDiff(string documentSource, string documentTarget) {
			//If execution results in new target file, then lets move it to the final target name.
			// evermap autobatch output path only changes the path not the filename			
			if (!FileHelper.AreSameFolder(documentSource, documentTarget)) {
				var acrobatOutputFile = Path.Combine(Path.GetDirectoryName(documentTarget), Path.GetFileName(documentSource));
				if (!FileHelper.AreEqual(acrobatOutputFile, documentTarget)) {
					FileHelper.MoveFileForce(acrobatOutputFile, documentTarget);
				}
			}
		}

		private void ExecuteCommand(string command, string args) {
			var startInfo = new ProcessStartInfo(command);
			startInfo.Arguments = args;
			startInfo.CreateNoWindow = true;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;

			Process.Start(startInfo).WaitForExit();
		}
	}
}
