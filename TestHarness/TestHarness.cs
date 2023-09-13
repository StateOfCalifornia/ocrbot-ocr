using CDT.Accessibility.Documents;
using CDT.Acrobat;
using CDT.Data;
using CDT.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using static CDT.OCR.Microsoft.Azure.CognitiveServicesConfiguration;

namespace CDT.WARS {
	class TestHarness {
		private static readonly string ApplicationRoot = AppDomain.CurrentDomain.BaseDirectory;
		private static readonly string ITextLicensePath = Path.Combine(ApplicationRoot, @"Resources\iText\iTextLicense.xml");
		private static readonly string DocumentRoot = Path.Combine(ApplicationRoot, @"Documents");
		//private static readonly string DocumentRoot = @"D:\itext\CDT\";

		static void Main() {
			TestRotation();
		}

		private static void TestRotation() {
			try {
				var inputFolder = Path.Combine(DocumentRoot, @"Input\");
				var outputFolder = Path.Combine(DocumentRoot, @"Output\");
				var cacheFolder = Path.Combine(DocumentRoot, @"Cache\");
				var azureKey = System.Environment.GetEnvironmentVariable("AZURE_KEY");
				var azureEndPoint = System.Environment.GetEnvironmentVariable("AZURE_ENDPOINT");
				var overwrite = true;
				var doAutoTag = true;

				//var client = new ScannedDocumentRemediation();
				//var cancelToken = new CancellationToken();
				//client.ProcessFolder(inputFolder, outputFolder, cancelToken, true, false);

				//manually iterate so we can use cache
				var files = FileHelper.GetFiles(inputFolder, true, ScannedDocumentRemediation.SupportedFileTypesExtensions);

				//DEBUG: isolate debug session to specific documents
				//files = files.Where(f => f.Contains("\\text-only-rotation\\"));

				foreach (var file in files) {
					var jsonSource = Path.ChangeExtension(Path.Combine(cacheFolder, FileHelper.MakeRelativePath(inputFolder, file)), ".ocr.json");
					var useCache = File.Exists(jsonSource);
					var destinationPath = Path.Combine(outputFolder, FileHelper.MakeRelativePath(inputFolder, file));

					ProcessFile(
						useCache: useCache,
						pdfSource: file,
						jsonSource: jsonSource,
						destinationPath: destinationPath,
						doAutoTag: doAutoTag,
						key: azureKey,
						endPoint: azureEndPoint,
						overwrite: overwrite);
				}

				//System.Console.WriteLine("Hit Enter");
				//System.Console.ReadLine();
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		private static void ProcessFile(bool useCache, string pdfSource, string jsonSource, string destinationPath, bool doAutoTag, string key, string endPoint, bool overwrite = false) {
			var clientConfiguration = new ClientConfiguration(key, endPoint);
			var acrobatClient = new AcrobatClient();
			Stream iTextLicenseStream = File.Exists(ITextLicensePath) ? new StreamReader(ITextLicensePath).BaseStream : null;
			var client = new ScannedDocumentRemediation(clientConfiguration, new Stats(), acrobatClient, pdfClientLicense: iTextLicenseStream);

			if (useCache) {
				//from cached OCR results
				client.ProcessFile(pdfSource, destinationPath, jsonSource, CancellationToken.None, overwrite);
			}
			else {
				//from Azure
				var sc = new SessionConfig(){
					DoAutoTag = doAutoTag,
					CancelToken = CancellationToken.None,
					OverwriteDestination = overwrite
				};
				client.ProcessFile(pdfSource, destinationPath, sc);
			}
		}
	}
}
