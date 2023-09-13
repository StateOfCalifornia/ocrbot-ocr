using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDT.Acrobat {

	public enum Feature {
		Autotag,
		ReOcr,
		TextOnly,
		Layering
	}

	public class FeatureDependency {

		// a list of all missing external dependencies
		private static readonly List<FeatureDependency> MissingExternalDependencies = new List<FeatureDependency>();

		private readonly List<Feature> DependentFeatures = new List<Feature>();
		public string Dependency { get; private set; }

		public FeatureDependency(String dependency) {
			Dependency = dependency;
		}

		public void AddFeature(Feature feature) {
			DependentFeatures.Add(feature);
		}

		public static string FormatMissingExternalDependencies() {
			if (MissingExternalDependencies.Count == 0) {
				return "";
			}

			var header = GetFormattedHeader();

			// display list of missing dependencies
			var dependencies = GetFormattedDependencies();

			//add all missing features into overallAffectedFeatures
			var affectedFeatures = GetFormattedAffectedFeatures();

			// put it all together
			var overall = $"{header}{System.Environment.NewLine}{System.Environment.NewLine}" +
				$"{dependencies}{System.Environment.NewLine}{System.Environment.NewLine}" +
				$"{affectedFeatures}";

			return overall;
		}

		public static void AddMissingExternalDependency(FeatureDependency toAdd) {
			MissingExternalDependencies.Add(toAdd);
		}
		public static void ClearMissingExternalDependency() {
			MissingExternalDependencies.Clear();
		}

		public static int MissingExternalDependenciesCount() {
			return MissingExternalDependencies.Count;
		}

		private static string GetFormattedHeader() {
			// should item be plural or singular
			var item_s = MissingExternalDependencies.Count > 1 ? "items" : "item";

			var formattedHeader = $"By installing the following {MissingExternalDependencies.Count} {item_s}:";

			return formattedHeader;
		}

		private static string GetFormattedDependencies() {
			string dependencies = "";
			int inx = 1;

			MissingExternalDependencies.ForEach((d) => {
				dependencies += $"{inx}) {d.Dependency}{System.Environment.NewLine}";
				inx++;
			});

			return dependencies;
		}

		private static string GetFormattedAffectedFeatures() {
			string formattedAffectedFeatures =  $"OCRBot will provide additional features:{System.Environment.NewLine}";

			var overallAffectedFeatures = new List<Feature>();

			//get all dependent feature of all missing external dependencies
			MissingExternalDependencies.ForEach((d) => overallAffectedFeatures.AddRange(d.DependentFeatures));
			
			//remove any duplicates
			overallAffectedFeatures = overallAffectedFeatures.Distinct().ToList();

			//get formatted list of affected features
			if (overallAffectedFeatures.Count > 0) {
				formattedAffectedFeatures += $"{System.Environment.NewLine}" +
					$"\t* " +
					$"{string.Join($"{System.Environment.NewLine}\t* ", overallAffectedFeatures)}";
			}

			return formattedAffectedFeatures;
		}
	}
}
