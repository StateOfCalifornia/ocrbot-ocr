using System;
using System.IO;
using Newtonsoft.Json;

namespace CDT.Helpers {
	public static class StreamHelper {
		/// <summary>
		/// safe way to fully copy a stream without missing any bytes
		/// </summary>
		/// <param name="input">any type of Stream to be copied</param>
		/// <returns>all bytes of source stream</returns>
		public static byte[] ToByteArray(Stream input) {
			if (input == null) { return null; }

			//short-circuit if input is already a memory stream
			MemoryStream ms = input as MemoryStream;
			byte[] data = ms?.ToArray();

			//if not a memory stream
			if (ms is null) {
				using (ms = new MemoryStream()) {
					input.Position = 0;  //
					input.CopyTo(ms);
					data = ms.ToArray();
				}
			}

			return data;
		}

		//ToDo: find a more appropriate namespace to move these serialization methods to
		public static string SerializeToJson(object item) {
			var json = JsonConvert.SerializeObject(item);
			return json;
		}

		public static T DeserializeFromJson<T>(string json) {
			var item  = JsonConvert.DeserializeObject<T>(json);
			return item;
		}
	}
}
