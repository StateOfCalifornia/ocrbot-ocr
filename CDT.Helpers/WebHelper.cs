using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Flurl;

namespace CDT.Helpers {
	public static class WebHelper {
		/// <summary>
		/// make HTTP GET request
		/// </summary>
		/// <param name="path">url path to GET</param>
		/// <param name="headers">requeest header values</param>
		/// <param name="query">request query string key values</param>
		/// <returns>completed call response</returns>
		public static HttpResponseMessage Get(string path, CancellationToken cancelToken, 
				NameValueCollection headers = null, NameValueCollection query = null)
		{
			//response = client.GetAsync(operationLocation).Result;
			HttpResponseMessage response;

			//path and query
			var url = BuildUrl(path, query);

			using (var client = new HttpClient()) {
				//headers
				AddHeaders(client, headers);

				//deadlock-safe way to synchronously call async function
				var task = Task.Run(() => client.GetAsync(url, cancelToken), cancelToken);
				task.Wait();
				response = task.Result;
			}

			return response;
		}

		/// <summary>
		/// make HTTP POST request
		/// </summary>
		/// <param name="path">url to POST to</param>
		/// <param name="data">request body content</param>
		/// <param name="contentType">type of data being submitted in the request body</param>
		/// <param name="headers">request headers</param>
		/// <param name="query">request query string key values</param>
		/// <returns></returns>
		public static HttpResponseMessage Post(string path, Stream data, string contentType,
				CancellationToken cancelToken, NameValueCollection headers = null, NameValueCollection query = null)
		{
			HttpResponseMessage response;

			//path and query
			var url = BuildUrl(path, query);

			using (var client = new HttpClient()) {
				//headers
				AddHeaders(client, headers);

				//body
				byte[] byteData = StreamHelper.ToByteArray(data);

				using (var content = new ByteArrayContent(byteData)) {
					content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
					//deadlock-safe way to synchronously call async function
					var task = Task.Run(() => client.PostAsync(url, content, cancelToken), cancelToken);
					task.Wait();
					response = task.Result;
				}
			}

			return response;
		}

		public static string BuildUrl(string path, NameValueCollection query) {
			var queryString = BuildQueryString(query);
			var url = $"{path}{queryString}";

			return url;
		}

		/// <summary>
		/// concatenate key/values into an encoded query string
		/// <para>preserves discreet multi-valued keys</para>
		/// </summary>
		public static string BuildQueryString(NameValueCollection parameters) {
			//null or empty
			if (!(parameters?.Count > 0)) { return String.Empty; }


			//get instance of internal System.Web.HttpValueCollection class
			//NameValueCollection with useful querystring extensions
			var query = HttpUtility.ParseQueryString(String.Empty);

			query.Add(parameters); //supports multi-value params (i.e., key1=val1&key1=val2)
			var qs = $"?{query}";  //key/values auto-encoded; prepend qs delimiter for non-empty result

			return qs;
		}

		/// <summary>
		/// combine url segments similar to Path.Combine
		/// </summary>
		/// <returns>concatinated path string</returns>
		public static string CombineUrl(params string[] segments) {
			var url = Url.Combine(segments);
			return url;
		}

		public static bool ValidateUrl(string url) {
			var isValid = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var parsed);

			return isValid;
		}

		public static bool ValidateUrl(string url, bool isRelative) {
			var kind = isRelative ? UriKind.Relative : UriKind.Absolute;
			var isValid = Uri.TryCreate(url, kind, out var parsed);

			return isValid;
		}

		/// <summary>
		/// append headers to an HTTP request
		/// <para>preserves discreet multi-valued keys</para>
		/// </summary>
		public static void AddHeaders(HttpClient client, NameValueCollection headers) {
			if (headers?.Count > 0) {
				foreach (var key in headers.AllKeys) {
					//preserve discreet multi-values (not collapsing to csv)
					var val = headers.GetValues(key);
					client.DefaultRequestHeaders.Add(key, val);
				}
			}
		}

		/// <summary>
		/// get the first available value if the header exists
		/// </summary>
		/// <param name="response">a completed HTTP request</param>
		/// <param name="key">header name</param>
		/// <returns>first value found or null</returns>
		public static string GetHeaderFirstOrDefault(HttpResponseMessage response, string key) {
			string value = null;
			var success = response.Headers.TryGetValues(key, out var values);

			if (success && values != null) {
				value = values.FirstOrDefault();
			}

			return value;
		}
	}
}
