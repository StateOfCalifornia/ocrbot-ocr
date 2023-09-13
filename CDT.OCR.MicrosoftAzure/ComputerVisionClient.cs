using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using CDT.Helpers;
using System.Collections.Specialized;
using System.Threading;

namespace CDT.OCR.Microsoft.Azure
{
	public class ComputerVisionClient {
		#region Static
		public static string ExceptionSource { get; } = AssemblyHelper.GetName<ComputerVisionClient>();


		public static ReadOperationResult LoadOCR(string path) {
			var results = FileHelper.FromJsonFile<ReadOperationResult>(path);

			return results;
		}

		/// <summary>
		/// serialize the ocr results to plain-text
		/// </summary>
		/// <param name="results">the OCR operation results</param>
		/// <param name="addPageBreaks">option to add a formatted page break for multi-page scans; default is false</param>
		public static string GetOcrPlainText(ReadOperationResult results, bool addPageBreaks = false, 
				Func<TextRecognitionResult, string> formatPageBreak = null)
		{
			//set page break formatter to use between pages of OCR results
			if (addPageBreaks) {
				if (formatPageBreak == null) { formatPageBreak = DefaultPageBreak; }
			}
			else {
				formatPageBreak = null;
			}

			var sb = new StringBuilder();

			foreach (var page in results.RecognitionResults) {
				//insert page break;
				var pageBreak = formatPageBreak?.Invoke(page);
				sb.AppendLine(pageBreak);

				//append OCR lines
				foreach (var line in page.Lines) {
					sb.AppendLine(line.Text);
				}
			}

			return sb.ToString();
		}

		private static string DefaultPageBreak(TextRecognitionResult page) {
			var nl = Environment.NewLine;
			string pageBreak = null;

			if (page.Page > 1) {
				pageBreak = $"{nl}{nl}{nl}- Page: {page.Page} -";
			}

			return pageBreak;
		}
		#endregion Static


		#region Instance
		private readonly CognitiveServicesConfiguration.ClientConfiguration ServiceConfig;


		public ComputerVisionClient() : this(null) {

		}

		public ComputerVisionClient(CognitiveServicesConfiguration.ClientConfiguration serviceConfig) {
			this.ServiceConfig = serviceConfig;// ?? new CognitiveServicesConfiguration.ClientConfiguration(CognitiveServicesKey, CognitiveServicesEndpoint);
		}


		public ReadOperationResult DocumentOCR(Stream document, CancellationToken cancelToken) {
			string operationLocation;

			//submit OCR request
			using (var ocrResponse = PostBatchAnalyzeRequest(document, cancelToken)) {
				if (!ocrResponse.IsSuccessStatusCode) {
					throw new ApplicationException($"BatchAnalyze Request Error: {ocrResponse.StatusCode}: {ocrResponse.ReasonPhrase}");
				}

				operationLocation = WebHelper.GetHeaderFirstOrDefault(ocrResponse, CognitiveServicesConfiguration.Headers.OperationLocation);
			}

			//poll for job completion and return results; error thrown if not successful
			ReadOperationResult result = WaitForReadOperation(operationLocation, cancelToken);

			return result;
		}

		/// <summary>
		/// polls job url checking for completion of detached processing request
		/// </summary>
		/// <param name="operationLocation">location to poll</param>
		/// <param name="result">response from completed job</param>
		/// <param name="pollingInterval">in milliseconds; default is 3000 ms (3 seconds)</param>
		/// <param name="maxPollAttempts">number of pollling attempts before timing out; default is 100 (5 minutes @ 3 seconds)</param>
		/// <returns>results of successfully completed job, else error thrown for any other status</returns>\
		/// <exception cref="ApplicationException">for local wait timeout or job error</exception>
		private ReadOperationResult WaitForReadOperation(string operationLocation, CancellationToken cancelToken, 
				int pollingInterval = 3000, int maxPollAttempts = 100)
		{
			var tryCount = 0;
			bool waiting;
			ReadOperationResult result;

			do {
				cancelToken.ThrowIfCancellationRequested();
				result = null; //flush prev result;
				System.Threading.Thread.Sleep(pollingInterval); //HACK: find non-blocking wait

				var response = GetOperationResults(operationLocation, cancelToken);
				tryCount++;

				if (response.IsSuccessStatusCode) {
					//ToDo: is this always available?
					string content = response.Content.ReadAsStringAsync().Result;
					result = StreamHelper.DeserializeFromJson<ReadOperationResult>(content);
					
					switch (result.Status) {
						//queued / executing
						case TextOperationStatusCodes.NotStarted:
						case TextOperationStatusCodes.Running:
							waiting = tryCount < maxPollAttempts;

							if (!waiting) {
								response.Dispose();
								throw new ApplicationException($"Wait time exceed for GetOperationResults{Environment.NewLine}\tUrl:{operationLocation}");
							}

							break;

						//completed
						case TextOperationStatusCodes.Failed:
							//HACK: Azure doesn't provide a strongly typed object to cast their error response into
							//this will break if they decides to change their error response
							var errorResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
							throw new ApplicationException($"OCR Error: {errorResult["errorCode"]}: {errorResult["errorMessage"]}");

						case TextOperationStatusCodes.Succeeded:
							//job has completed
							waiting = false;
							break;

						default:
							throw new ApplicationException($"Unknown Job Status \"{result.Status }\" for GetOperationResults{Environment.NewLine}\tUrl:{operationLocation}");
					}
				}
				else {
					throw new ApplicationException($"Error occurred in Job Operation: {response.StatusCode}: {response.ReasonPhrase}{Environment.NewLine}\tUrl:{operationLocation}");
				}
			} while (waiting);

			return result;
		}

		/// <summary>
		/// peform OCR of scanned document
		/// </summary>
		/// <param name="document"></param>
		/// <returns>on success, response will contain a callback url of the detached processing job</returns>
		private HttpResponseMessage PostBatchAnalyzeRequest(Stream document, CancellationToken cancelToken)
		{
			var headers = GetDefaultHeaders();
		
			var response = WebHelper.Post(ServiceConfig.BatchAnalyzeUrl, document, 
				CognitiveServicesConfiguration.ContentTypes.Binary, cancelToken, headers);
			
			return response;
		}
		

		/// <summary>
		/// query a detached job operation
		/// </summary>
		/// <param name="operationLocation">location of detached processing job</param>
		/// <returns>response from operation callback containing the current job status, and results if completed</returns>
		private HttpResponseMessage GetOperationResults(string operationLocation, CancellationToken cancelToken) {
			var headers = GetDefaultHeaders();
			var response = WebHelper.Get(operationLocation, cancelToken, headers);

			return response;
		}

		/// <summary>
		/// get required set of headers needed for making service calls; e.g. subscriptionKey
		/// </summary>
		/// <returns></returns>
		private NameValueCollection GetDefaultHeaders()
		{
			var headers = new NameValueCollection();
			headers[CognitiveServicesConfiguration.Headers.SubscriptionKey] = this.ServiceConfig.SubscriptionKey;

			return headers;
		}
		/// <summary>
		/// Merges multiple results from a split pdf due to Azure size limitations
		/// </summary>
		/// <param name="OCRResults">List of results from Azure Cognitive Services</param>
		/// <returns>Merged ReadOperationResults</returns>
		public ReadOperationResult MergOCRResults(IList<ReadOperationResult> OCRResults) {
			var result = OCRResults[0];

			for (var pdfCount = 1; pdfCount < OCRResults.Count; pdfCount++) {
				foreach(var page in OCRResults[pdfCount].RecognitionResults) {
					page.Page = result.RecognitionResults.Count + 1;
					result.RecognitionResults.Add(page);
				}
			}
			return result;
		}
		#endregion Instance
	}
}