using System.IO;

namespace CDT.OCR.Microsoft.Azure {
	public class CognitiveServicesConfiguration {
		#region Client
		public ClientConfiguration Config { get; private set; }


		public CognitiveServicesConfiguration(CognitiveServicesConfiguration.ClientConfiguration config) {
			this.Config = config;
		}


		public class ClientConfiguration {
			public string SubscriptionKey { get; set; }
			public string ServiceEndpoint { get; set; }

			public string BatchAnalyzeUrl => $"{ServiceEndpoint}{CognitiveServicesConfiguration.BatchAnalyze.EndPoint}";

			public ClientConfiguration(string subscriptionKey, string serviceEndPoint) {
				this.SubscriptionKey = subscriptionKey;
				this.ServiceEndpoint = serviceEndPoint;
			}
		}
		#endregion


		#region Shared
		public static class Headers {
			/// <summary>
			/// callback url for detached processing request
			/// </summary>
			public static string OperationLocation => "Operation-location";

			/// <summary>
			/// User's authentication key
			/// </summary>
			public static string SubscriptionKey => "Ocp-Apim-Subscription-Key";
		}


		public static class ContentTypes {
			public static string Binary => "application/octet-stream";
			public static string Json => "application/json";
		}
		#endregion Shared


		public static class BatchAnalyze {
			public static string EndPoint => "/vision/v2.0/read/core/asyncBatchAnalyze";
			public static string[] SupportedFileTypes => new string[] { "JPEG", "PNG", "BMP", "PDF", "TIFF" };
		}
	}
}
