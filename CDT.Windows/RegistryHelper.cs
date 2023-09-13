using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace CDT.Windows {
	public class RegistryHelper {
		public static object GetKeyValue(string key, string valueName) {
			var value = Registry.GetValue(key, valueName, null);

			return value;
		}

		public static string GetInstalledVersion(string displayName, int type = 0) {
			string ret = "";

			string display_name = "DisplayName";
			string display_version = "DisplayVersion";

			string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

			Microsoft.Win32.RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(registry_key);

            if (type == 1) 
			{
				key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(registry_key);
            }
				
			foreach (string subkey_name in key.GetSubKeyNames()) {					
				using (RegistryKey subkey = key.OpenSubKey(subkey_name)) {
					var name = subkey.GetValue(display_name)?.ToString();
                    Console.WriteLine("name = " + name);
                    var ver = subkey.GetValue(display_version)?.ToString();
                    Console.WriteLine("ver = " + ver);
                    if (name != null && name.Equals(displayName, StringComparison.InvariantCultureIgnoreCase)) {
						ret = ver;
						break;
					}
				}
			}
			
			key.Dispose();
			return ret;
		}
	}
}
