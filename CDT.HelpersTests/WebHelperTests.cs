using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDT.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Net.Http;
using System.Linq;
using System.Web;

namespace CDT.Helpers.Tests {
	[TestClass()]
	public class WebHelperTests {
		//[TestMethod()]
		//public void GetTest() => throw new NotImplementedException();

		//[TestMethod()]
		//public void PostTest() => throw new NotImplementedException();


		#region BuildQueryString
		[TestMethod()]
		public void BuildQueryString_NullTest() {
			var result = WebHelper.BuildQueryString(null);

			Assert.AreEqual(String.Empty, result);
		}

		[TestMethod()]
		public void BuildQueryString_EmptyTest() {
			var query = new NameValueCollection();
			var result = WebHelper.BuildQueryString(query);

			Assert.AreEqual(String.Empty, result);
		}

		[TestMethod()]
		public void BuildQueryString_IsEncodedTest() {
			var query = new NameValueCollection();
			query.Add("visualFeatures", "Categories,Description,Color");

			var result = WebHelper.BuildQueryString(query);

			Assert.AreEqual("?visualFeatures=Categories%2cDescription%2cColor", result);
		}

		[TestMethod()]
		public void BuildQueryString_KeyMultiValueTest() {
			var query = new NameValueCollection();
			query.Add("visualFeatures", "Categories");
			query.Add("visualFeatures", "Description");
			query.Add("visualFeatures", "Color");

			var result = WebHelper.BuildQueryString(query);

			Assert.AreEqual("?visualFeatures=Categories&visualFeatures=Description&visualFeatures=Color", result);
		}
		#endregion BuildQueryString


		#region AddHeaders
		[TestMethod()]
		public void AddHeaders_ValuesTest() {
			//requires internal System.Web.HttpValueCollection class
			//for multi-value support
			var headers = HttpUtility.ParseQueryString(String.Empty);
			//discreet multi-value
			var h1 = "Header1";
			var v1a = "Value1a";
			var v1b = "Value1b";
			//multi-value collapsed to single-value
			var h2 = "Header2";
			var v2 = "Value2a,Value2b";

			//multi-value
			headers.Add(h1, v1a);
			headers.Add(h1, v1b);
			//single value
			headers[h2] = v2;

			using (var client = new HttpClient()) {
				WebHelper.AddHeaders(client, headers);

				//H1[]
				var values = client.DefaultRequestHeaders.GetValues(h1).ToArray();
				Assert.IsTrue(values.Length == 2, $"'h1 values': Expecting two discreet values; Actual: {values.Length}");
				Assert.IsTrue(values[0] == $"{v1a}", $"'v1a': Expecting: '{v1a}'; Actual: '{values[0]}'");
				Assert.IsTrue(values[1] == $"{v1b}", $"'v1b': Expecting: '{v1b}'; Actual: '{values[1]}'");

				//H2 - collapsed csv
				values = client.DefaultRequestHeaders.GetValues(h2).ToArray();
				Assert.IsTrue(values.Length == 1, $"'h2 values': Expecting single collapsed value; Actual: 'Count: {values.Length}'");
				Assert.IsTrue(values[0] == $"{v2}", $"'v2': Expecting: '{v2}'; Actual: '{values[0]}'");
			}
		}

		public void AddHeaders_NullTest() {
			NameValueCollection headers = null;

			using (var client = new HttpClient()) {
				WebHelper.AddHeaders(client, headers);
				//success if prev line didn't throw exception
				Assert.IsTrue(true);
			}
		}
		#endregion


		#region GetHeaderFirstOrDefault
		[TestMethod()]
		public void GetHeaderFirstOrDefault_FoundTest() {
			var headers = new NameValueCollection();
			var h1 = "Header1";
			var v1a = "Value1a";

			headers[h1] = v1a;

			using (var response = new HttpResponseMessage()) {
				response.Headers.Add(h1, v1a);

				var outV1a = WebHelper.GetHeaderFirstOrDefault(response, h1);
				Assert.AreEqual(v1a, outV1a);
			}
		}

		[TestMethod()]
		public void GetHeaderFirstOrDefault_NotFoundTest() {
			var h1 = "Header1";

			using (var response = new HttpResponseMessage()) {
				var outV1a = WebHelper.GetHeaderFirstOrDefault(response, h1);
				Assert.IsNull(outV1a);
			}
		}
		#endregion GetHeaderFirstOrDefault


		[TestMethod()]
		public void CombineUriPathTest() {
			var a = "http://foo.com";
			var b = "a/b/c/";
			var expected = $"{a}/{b}";

			//neither with delimiter
			var formatted = WebHelper.CombineUrl(a, b);
			Assert.AreEqual(expected, formatted);

			//first with delimiter
			formatted = WebHelper.CombineUrl(a + "/", b);
			Assert.AreEqual(expected, formatted);

			//second with delimiter
			formatted = WebHelper.CombineUrl(a, "/" + b);
			Assert.AreEqual(expected, formatted);

			//both with delimiter
			formatted = WebHelper.CombineUrl(a + "/", "/" + b);
			Assert.AreEqual(expected, formatted);
		}
	}
}