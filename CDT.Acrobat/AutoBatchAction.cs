using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CDT.Windows;

namespace CDT.Acrobat {
	internal class AutoBatchConfig {
		private readonly string AutoBatchDescription = "Evermap AutoBatch";

		public string AutoBatchCommandRoot { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\AcrobatBatch\Application\");

		public bool IsAutoBatchAvailable { get; private set; }


		/// <summary>
		/// AutoBatch plugin core config
		/// - to be executed only once and shared across all sequences
		/// </summary>
		public AutoBatchConfig(AcrobatClient acrobat) {
			// determine feature availability

			// dependency check
			if (!AcrobatConfig.IsCommandAvailable) {
				IsAutoBatchAvailable = false;
			}
			//feature check
			else {
				// Evermap AutoBatch plugin - installed in Adobe Acrobat's plugin folder
				var licensePath = Path.Combine(AcrobatConfig.PluginRoot, @"AutoBatch\license.txt");
				IsAutoBatchAvailable = File.Exists(licensePath);
			}
			
			// fill out feature dependency report
			if (!IsAutoBatchAvailable) {
				UpdateFeatureDependency(AutoBatchDescription);
			}
		}


		private void UpdateFeatureDependency(string description) {
			// all features dependent on Autobatch
			var dependency = new FeatureDependency(description);
			dependency.AddFeature(Feature.Autotag);
			dependency.AddFeature(Feature.Layering);
			dependency.AddFeature(Feature.ReOcr);
			dependency.AddFeature(Feature.TextOnly);
			FeatureDependency.AddMissingExternalDependency(dependency);
		}
	}

	internal class AutoBatchAction {
        // For action wizards with preflight calls, an On Success directory can be set,
        // Greatly increasing error handling
        // in .sequ file CALS_PREFLIGHT_CMD_SUC_ACT_TYPE should = 2
        //   CALS_PREFLIGHT_CMD_SUC_FOLDER = {SUC_FOLDER}
        //   AcrobatClient will replace {SUC_FOLDER} with with actual directory path to 
        //   a temporarily created directory under the output dir
        private const string SuccessSubdirectory = ".~ocrbot-success~";
        private const string SequenceSuccessReplacementToken = "{SUC_FOLDER}";

        //Sequence file has the template and the working copy.
        // have a working copy so the template version will remain unmodified
        public string TemplateSequenceFile { get; private set; }

		public string WorkingSequenceFile { get; private set; }
		/// <summary>
		/// shell command line to run the PDF Autotag plugin
		/// </summary>
		public string ShellCommand { get; private set; }

		/// <summary>
		/// indicates if the plugin is installed and action can be called
		/// </summary>
		public bool IsCommandAvailable { get; private set; }

		/// <summary>
		/// if autobatch will write to separate folder for pdf Action Wizard "On Success"
		/// this  value will be the expected absolute path of file created in On SuccessDirectory
		/// null if no "On Success" available
		/// </summary>
		public string OnSuccessFolder { get; private set; }


		/// <summary>
		/// sequence config
		/// - consider using static instances to avoid repeatedly validating per document processed
		/// </summary>
		/// <param name="templateName">used to auto-discover corresponding script files by convention</param>
		public AutoBatchAction(AutoBatchConfig autobatch, string templateName) {
			if (autobatch.IsAutoBatchAvailable) {
				this.TemplateSequenceFile = Path.Combine(autobatch.AutoBatchCommandRoot, $"{templateName}.sequ");
				this.ShellCommand = Path.Combine(autobatch.AutoBatchCommandRoot, $"{templateName}.bat");

				this.IsCommandAvailable = File.Exists(this.TemplateSequenceFile) && File.Exists(this.ShellCommand);
			}
			else {
				this.IsCommandAvailable = false;
			}

			if (!this.IsCommandAvailable) {
				this.TemplateSequenceFile = null;
				this.ShellCommand = null;
			}
		}



		/// <summary>
		/// get formatted command arguments string
		/// </summary>
		/// <param name="inputDocumentPath">source PDF</param>
		/// <param name="outputDocumentPath">output path of tagged document; if null, will overwrite <paramref name="inputDocumentPath"/></param>
		/// <returns>formmatted command line arguments string</returns>
		public string FormatCommandArgs(string inputDocumentPath, string outputDocumentPath = null) {
			if (String.IsNullOrEmpty(outputDocumentPath)) {
				outputDocumentPath = inputDocumentPath;
			}

			string successFolder;
			this.WorkingSequenceFile = SetSequenceFile(TemplateSequenceFile, outputDocumentPath, out successFolder);
			this.OnSuccessFolder = successFolder;

			var args = $"\"{inputDocumentPath}\" \"{outputDocumentPath}\" \"{this.WorkingSequenceFile}\" {AcrobatConfig.ShellCommand}";

			return args;
		}



		//returns newSequenceFile location
				// If the evermap Action Wizard should result in output directed to a "On Success" folder, successFile will be set to the expected
		// fullfilepath
		// and if not redirected to an On Success Folder , successFile will be set to null 
		internal string SetSequenceFile(string sequenceFile, string outputFile, out string onSuccessFolder ) {
			//CodeReview:  this function is too difficult to follow - refactor to improve clarity
			string sequenceText = File.ReadAllText(sequenceFile);

            var successSubDirectory = Path.Combine(
                Path.GetDirectoryName(outputFile), SuccessSubdirectory);

            Directory.CreateDirectory(successSubDirectory);
   
            if (sequenceText.IndexOf(SequenceSuccessReplacementToken) < 0) {
				onSuccessFolder = null;
			}
			else {
                //the success folder is a "on success" subdirectory under the outputFile base directory
                //replace the replacement paramenter with the success directory path
                sequenceText = sequenceText.Replace(SequenceSuccessReplacementToken, successSubDirectory);
                onSuccessFolder = successSubDirectory;
			}

			var target = Path.Combine(successSubDirectory, Path.GetFileName(sequenceFile));
			File.WriteAllText(target, sequenceText);
			return target;
		}
	}
}
