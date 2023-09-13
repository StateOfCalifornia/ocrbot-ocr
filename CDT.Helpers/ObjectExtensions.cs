using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CDT.Helpers {
	public static class ObjectExtensions {
		public static void Merge<T>(this T destination, T source)  {
			Type t = typeof(T);

			var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

			foreach (var prop in properties) {
				var value = prop.GetValue(source, null);
				//copy all original values even if empty or null
				prop.SetValue(destination, value, null);
			}
		}
	}
}
