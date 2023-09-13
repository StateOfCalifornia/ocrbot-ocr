using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDT.Acrobat;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CDT.Acrobat.Tests {
	[TestClass]
	public class FeatureDependencyTests {

		private static List<FeatureDependency>  tests = new List<FeatureDependency>();
		private static List<Feature> addedFeatures = new List<Feature>();
		private static readonly int num_dependencies = 5;
		[TestInitialize()]
		public void BuildMissingDependencies() {
			FeatureDependency.ClearMissingExternalDependency();
			tests.Clear();
			for (int index = 0; index < num_dependencies; index += 1) {
				FeatureDependency featureDependency = new FeatureDependency($"Dependency{index}");

				tests.Add(featureDependency);

				addedFeatures.Clear();


				foreach (Feature f in Enum.GetValues(typeof(Feature))) {
					// have differnet tests have differnt features.
					if ((int)f > index) {
						break;
					}

					addedFeatures.Add(f);
				}
			}


			foreach (var fd in tests) {
				foreach (var f in addedFeatures) {
					fd.AddFeature(f);
				}
				FeatureDependency.AddMissingExternalDependency(fd);
			}

		}

		[TestMethod]
		public void FormattedOutput_HasHeader() {
			var output = FeatureDependency.FormatMissingExternalDependencies();
			var item_s = tests.Count > 1 ? "items" : "item";
			Assert.IsTrue(output.Contains($"By installing the following {tests.Count} {item_s}"));
		}

		[TestMethod]
		public void FormattedOutput_Displays_All_Missing_Dependencies() {
			var output = FeatureDependency.FormatMissingExternalDependencies();
			tests.ForEach((d) => {
				Assert.IsTrue(output.Contains(d.Dependency));
			});
		}

		[TestMethod]
		public void FormattedOutput_Displays_All_Missing_Features() {
			var output = FeatureDependency.FormatMissingExternalDependencies();
			tests.ForEach((d) => {
				addedFeatures.ForEach(f => Assert.IsTrue(output.Contains(f.ToString())));
			});
		}


		[TestMethod]
		public void FormattedOutput_No_Duplicate_Features() {
			var output = FeatureDependency.FormatMissingExternalDependencies();
			addedFeatures.ForEach(f =>
				Assert.IsTrue(System.Text.RegularExpressions.Regex.Matches(output, f.ToString()).Count == 1));
		}

		[TestMethod]
		public void FormattedOutput_Numererates_Missing_Dependencies() {
			var output = FeatureDependency.FormatMissingExternalDependencies();
			int inx = 1;
			tests.ForEach((d) => {
				Assert.IsTrue(output.Contains($"{inx}) {d.Dependency}"));
				inx++;
			});
		}

	}
}
