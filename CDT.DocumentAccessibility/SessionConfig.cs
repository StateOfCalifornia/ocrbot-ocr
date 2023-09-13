using CDT.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace CDT.Accessibility.Documents {
	public class SessionConfig {
		#region Properties
		#region User Settings
		private string _inputDirectory;
		private string _outputDirectory;

		//negative property for safe implicit default
		public bool DontLayerPDF { get; set; }

		public bool DoAutoTag { get; set; }
		public bool DoConvertCMYK { get; set; }

		public bool OverwriteDestination { get; set; }

		public bool ShowTextOnly { get; set; }

		public bool UseOCRCache { get; set; }

		public IEnumerable<string> InputFiles { get; set; }


		public string InputDirectory {
			get => _inputDirectory;

			//ensure path is treated as a directory rather than a file name
			set => _inputDirectory = FileHelper.TerminateDirectoryPath(value);
		}

		public string OutputDirectory {
			get => _outputDirectory;

			//ensure path is treated as a directory rather than a file name
			set => _outputDirectory = FileHelper.TerminateDirectoryPath(value);
		}

		public string ServiceEndpoint { get; set; }
		public string SubscriptionKey { get; set; }
		#endregion User Settings


		#region Current Session Cache
		[IgnoreDataMember]
		public CancellationToken CancelToken { get; set; }

		[IgnoreDataMember]
		public Action OnTaskComplete { get; set; }
		#endregion Current Session Cache
		#endregion Properties



		public bool IsSet() => !string.IsNullOrEmpty(ServiceEndpoint) && !string.IsNullOrEmpty(SubscriptionKey);

		public override string ToString() {
			var delimiter = $"{Environment.NewLine}\t";

			var formatted = $"SessionConfig:{delimiter}" +
				$"[InputDirectory]:{InputDirectory}{delimiter}" +
				$"[OutputDirectory]:{OutputDirectory}{delimiter}" +
				$"[InputFiles(count)]:{InputFiles?.Count()}{delimiter}" +
				$"[OverwriteDestination]:{OverwriteDestination}{delimiter}" +
                $"[DoAutoTag]:{DoAutoTag}{delimiter}" +
                $"[DontLayerPDF]:{DontLayerPDF}{delimiter}" +
                $"[DoConvertCMYK]:{DoConvertCMYK}{delimiter}" +
                $"[ShowTextOnly]:{ShowTextOnly}{delimiter}" +
				$"[UseOCRCache]:{UseOCRCache}{delimiter}" +
				$"[ServiceEndpoint]:{ServiceEndpoint}{delimiter}" +
				$"[SubscriptionKey]:{(string.IsNullOrEmpty(SubscriptionKey) ? "Not Set" : "Value is Set")}";

			return formatted;
		}



		public static SessionConfig Load(string file) {
			var settings = FileHelper.FromJsonFile<SessionConfig>(file);
			return settings;
		}

		public void Save(string file) {
			FileHelper.ToJsonFile(file, this);
		}
	}
}
