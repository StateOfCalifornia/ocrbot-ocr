using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDT.Windows;

namespace CDT.WindowsTests {
	/// <summary>
	/// Summary description for RegistryHelperTests
	/// </summary>
	[TestClass]
	public class RegistryHelperTests {
		#region GetKeyValue
		[TestMethod]
		public void GetKeyValue_ExistsTest() {
			var value = RegistryHelper.GetKeyValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", @"CurrentVersion");

			Assert.IsNotNull(value);
		}

		[TestMethod]
		public void GetKeyValue_DoesNotExist_NoDefaultValueTest() {
			var value = RegistryHelper.GetKeyValue(@"HKEY_LOCAL_MACHINE\does\not\exist", "");

			Assert.IsNull(value);
		}
		#endregion GetKeyValue
	}
}
