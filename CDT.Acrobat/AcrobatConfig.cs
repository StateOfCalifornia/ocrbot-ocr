using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CDT.Windows;

namespace CDT.Acrobat {
	internal static class AcrobatConfig {
		/// <summary>
		/// root directory path for Adobe Acrobat plugins
		/// </summary>f
		public static string PluginRoot { get; private set; }

		/// <summary>
		/// shell command line to run Adobe Acrobat
		/// </summary>
		public static string ShellCommand { get; private set; }

		/// <summary>
		/// indicates if the application is installed and can be called
		/// </summary>
		public static bool IsCommandAvailable { get; private set; }


		static AcrobatConfig() {
			//lookup Acrobat command path and cache the result
			var shellRegistryKey = @"HKEY_CLASSES_ROOT\Applications\Acrobat.exe\shell\Open\command";
			var cmd = RegistryHelper.GetKeyValue(shellRegistryKey, "")?.ToString();

			if (String.IsNullOrWhiteSpace(cmd)) {
				//Acrobat is not installed (or command not found)
				IsCommandAvailable = false;
				PluginRoot = null;
				ShellCommand = null;
			}
			else {
				//Acrobat appears to be installed
				//strip quotes for Path functions..
				cmd = cmd.Replace("\"", "");
				var commandArgs = "%1"; // "\"%1""; 

				IsCommandAvailable = true;
				PluginRoot = Path.Combine(Path.GetDirectoryName(cmd), @"plug_ins\");
				ShellCommand = $"\"{cmd.Replace(commandArgs, "")}\"";
			}
		}
	}
}
