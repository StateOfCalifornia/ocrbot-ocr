using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CDT.Acrobat {
	public class PreFlight {
		private static readonly string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		private static readonly string BasePreFlightDirectory = Path.Combine(AppDataDir, @"Adobe\Acrobat\Preflight Acrobat Continuous\Repositories\Custom");
		private static readonly string RepositoryInfoFileName = "info.xml";
		private static readonly string ProfilesSubdirectory = "Profiles_16";
		private static readonly string ProfileInfoFileName = "index.kfx";
		private static readonly int MinimumRequiredAcrobatVersion = 19;
		private static readonly string MinimumRequiredAcrobatVersionDescription = $"Acrobat DC Pro (20{MinimumRequiredAcrobatVersion}.x) or higher";
		private static readonly string PreflightLibraryDescription = "CDT Preflight Libraries";



		private struct Repositories {
			public static string Cals => "c116ea2f63c200f6681b0a23d1f9a02d";
		}

		private struct Profiles {
			public static string RemoveAllText => "P8650ee11d712b44b92c81a8bdbf68ea9";
			public static string RemoveInvisibleText => "P366606777881a2ba71dfc9340d1c5107";
			public static string MakeVisibilityLayers => "P06e03f8f7887a9baea947ea652b8a736";
			public static string RemoveTags => "P6cb56b77bdbb91b88bd28ad89692f38d";
			public static string ShowTextOnly => "Pf46791722336e53bca80915f84a1dd8e";
		}


		private string RepositoryDirectory { get; set; }
		private bool IsRemoveTagsAvailable { get; set; }
		private bool IsShowTextOnlyAvailable { get; set; }
		private bool IsRemoveAllTextAvailable { get; set; }
		private bool IsRemoveInvisibleTextAvailable { get; set; }
		private bool IsMakeVisibilityLayersAvailable { get; set; }
		private bool IsRequiredMinimumAcrobatVersionInstalled { get; set; }



		public bool IsLibraryAvailable { get; private set; }
		public bool IsReOCRAvailable { get; private set; }


		public PreFlight(AcrobatClient acrobat) {
			RepositoryDirectory = GetRepositoryDirectory();
			IsRequiredMinimumAcrobatVersionInstalled = acrobat.AcrobatMajorVersion >= MinimumRequiredAcrobatVersion;

			//re-OCR sub-features
			IsRemoveAllTextAvailable = IsProfileAvailable(RepositoryDirectory, Profiles.RemoveAllText);
			IsRemoveInvisibleTextAvailable = IsProfileAvailable(RepositoryDirectory, Profiles.RemoveInvisibleText);
			IsMakeVisibilityLayersAvailable = IsProfileAvailable(RepositoryDirectory, Profiles.MakeVisibilityLayers);
			IsRemoveTagsAvailable = IsProfileAvailable(RepositoryDirectory, Profiles.RemoveTags);
			IsShowTextOnlyAvailable = IsProfileAvailable(RepositoryDirectory, Profiles.ShowTextOnly);

			IsLibraryAvailable = Directory.Exists(RepositoryDirectory) &&
				IsRemoveAllTextAvailable &&
				IsRemoveInvisibleTextAvailable &&
				IsMakeVisibilityLayersAvailable &&
				IsRemoveTagsAvailable &&
				IsShowTextOnlyAvailable;


			IsReOCRAvailable = acrobat.IsAutoBatchAvailable &&
				IsLibraryAvailable &&
				IsRequiredMinimumAcrobatVersionInstalled;

			if (!IsRequiredMinimumAcrobatVersionInstalled) {
				UpdateFeatureDependency(MinimumRequiredAcrobatVersionDescription);
			}

			if (!IsLibraryAvailable) {
				UpdateFeatureDependency(PreflightLibraryDescription);
			}
		}

		private static void UpdateFeatureDependency(string desciption) {
			// add features dependent on preflight and minimum adobe version
			var featureDependency = new FeatureDependency(desciption);
			featureDependency.AddFeature(Feature.TextOnly);
			featureDependency.AddFeature(Feature.ReOcr);
			featureDependency.AddFeature(Feature.Layering);
			FeatureDependency.AddMissingExternalDependency(featureDependency);
		}

		private static string GetRepositoryDirectory() {
			string found = String.Empty;
			var rootDir = new DirectoryInfo(BasePreFlightDirectory);

			if (!rootDir.Exists) {
				return found;
			}


			//in each directory look for info.xml with flag id matching the CALS repository ID
			var matchString = $@"id=""{Repositories.Cals}""";
			var directories = rootDir.GetDirectories();

			foreach (var dir in directories) {
				var infoFile = Path.Combine(dir.FullName, RepositoryInfoFileName);

				if (File.Exists(infoFile) && File.ReadAllText(infoFile).Contains(matchString)) {
					//we found our respository directory :)
					found = dir.FullName;
					break;
				}
			}

			return found;
		}

		private static bool IsProfileAvailable(string repositoryDirectory, string profileID) {
			var subDirectory = Path.Combine(repositoryDirectory, ProfilesSubdirectory);
			var profileInfoFile = Path.Combine(subDirectory, ProfileInfoFileName);

			if (!File.Exists(profileInfoFile)) {
				return false;
			}


			var infoDataLines = File.ReadAllLines(profileInfoFile);

			// use reg exp with replacement paramenter
			// typical line looks like <profgroup XMLID="G9e75d64ba3b89dbcca6c3a08d532985d" file="GJPQTCIT3N2ERPIJC784DACKOBK.kfg"></profgroup>
			var matchString = $@".*XMLID=""{profileID}"" file=""(.*)"".*";
			bool found = false;

			foreach (var infoDataLine in infoDataLines) {
				var matches = Regex.Match(infoDataLine, matchString);

				if (matches.Groups.Count > 0) {
					//if (infoDataLine.Contains(matchString)) {
					//get file name and make sure it exists in this directory
					var file = matches.Groups[1].Value;
					var fullPath = Path.Combine(subDirectory, file);

					found = File.Exists(fullPath);

					if (found) {
						break;
					}
				}
			}

			return found;
		}
	}
}
