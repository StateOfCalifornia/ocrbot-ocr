using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CDT.Helpers
{
	public class AssemblyHelper
	{
		/// <summary>
		/// get an assembly embedded resource
		/// </summary>
		/// <param name="resourceName">fully qualified name, i.e. 'MyAssembly.MyFile.txt'</param>
		/// <param name="assembly">the assembly containing the resource; assumes the executing assembly by default</param>
		/// <returns>unmanaged Stream</returns>
		public static Stream GetResource(Assembly assembly, string resourceName) {
			var stream = assembly.GetManifestResourceStream(resourceName);

			return stream;
		}

		public static string GetName<T>(){
			var assemblyName = System.Reflection.Assembly.GetAssembly(typeof(T)).GetName();

			return assemblyName.Name;
		}
	}
}
