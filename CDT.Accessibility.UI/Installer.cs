using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Windows.Forms;
using CDT.Helpers;
using CDT.Accessibility.Documents;

namespace CDT.Accessibility.UI {
	public class Installer {
		private static readonly string appdataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		private static readonly string BasePreFlightDirectory = Path.Combine(appdataDir, @"Adobe\Acrobat\Preflight Acrobat Continuous\Repositories\Custom");
		private static readonly string BaseSequenceDirectory = Path.Combine(appdataDir, @"Adobe\Acrobat\DC\Sequences");
		private static readonly string zipLibraryFiles = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\AcrobatBatch\Library\OCRBotCustomInstall.zip");
		private static readonly string userSequenceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\AcrobatBatch\User");
		private static string OCRBotSettingsOldFile { get; } = Path.Combine(Program.ApplicationDataDirectory, @".pre\Resources\Settings\OCRBotSettings.json");

		/// <summary>
		/// Migrate previous settings after a ClickOnce upgrade
		/// </summary>
		private static void MergeSettings() {
			if (FileHelper.Exists(OCRBotSettingsOldFile)) {
				var oldSettings  = SessionConfig.Load(OCRBotSettingsOldFile);
				Program.OCRBotSettings.Merge(oldSettings);
				Program.OCRBotSettings.Save(Program.OCRBotSettingsFile);
				FileHelper.MoveFileForce(OCRBotSettingsOldFile, $"{OCRBotSettingsOldFile}.BACK");
			}
		}

		private static bool InstallLibrary() {
			//restart application to redo init and dependency detection
			bool changesMade = false;

			try {
				if (Directory.Exists(BaseSequenceDirectory)) {
					// first just copy over user sequence files
					var userSequenceDir = new DirectoryInfo(userSequenceDirectory);
					var userFiles = userSequenceDir.GetFiles();

					foreach (var file in userFiles) {
						var dest = Path.Combine(BaseSequenceDirectory, file.Name);

						if (!File.Exists(dest)) {
							changesMade = true;
							File.Copy(file.FullName, dest, true);
						}
					}


					//copy over preflight only if needed
					if (!Program.IsReOCRAvailable) {
						changesMade = true; //trigger restart

						Directory.CreateDirectory(BasePreFlightDirectory);

						//ZipFile.ExtractToDirectory(zipLibraryFiles, BasePreFlightDirectory);
						var archive = ZipFile.OpenRead(zipLibraryFiles);

                        foreach (var file in archive.Entries)
                        {
                            var dest = Path.GetFullPath(Path.Combine(BasePreFlightDirectory, file.FullName));
                            var fullDestDirPath = Path.GetFullPath(BasePreFlightDirectory + Path.DirectorySeparatorChar);



                            if (!dest.StartsWith(fullDestDirPath))
                            {
                                throw new System.InvalidOperationException("File is outside the target directory: " + dest);
                            }
                            else if (dest.EndsWith("/"))
                            {
                                Directory.CreateDirectory(dest);
                            }
                            else
                            {
                                file.ExtractToFile(dest, true);
                            }
                        }
                    }
                }
			}
			catch (Exception ex) {
				Console.WriteLine("Installer Error:" + ex.ToString());
			}


			return changesMade;
		}


		public static bool Install() {
			MergeSettings();
			var changesMade = InstallLibrary();
			return changesMade;
		}
	}
}
