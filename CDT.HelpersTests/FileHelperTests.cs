using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDT.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CDT.Helpers.Tests {
	[TestClass()]
	public class FileHelperTests {
		#region IsBaseOf
		[TestMethod()]
		public void IsBaseOf_SimpleTest() {
			var result = FileHelper.IsBaseOf(@"c:\dir1\", @"c:\dir1\dir2\");

			Assert.IsTrue(result);
		}

		[TestMethod()]
		public void IsBaseOf_RelativeTest() {
			var result = FileHelper.IsBaseOf(@"c:\dir1\", @"c:\temp\..\dir1\dir2\");

			Assert.IsTrue(result);
		}

		[TestMethod()]
		public void IsBaseOf_MaliciousTest() {
			var result = FileHelper.IsBaseOf(@"c:\dir1\", @"c:\dir1\..\windows\system32\");

			Assert.IsFalse(result);
		}

		[TestMethod()]
		public void IsBaseOf_SelfTest() {
			var result = FileHelper.IsBaseOf(@"c:\dir1\", @"c:\dir1\");

			Assert.IsTrue(result);
		}

		[TestMethod()]
		public void IsBaseOf_Self_MissingSlashTest() {
			var result = FileHelper.IsBaseOf(@"c:\dir1\", @"c:\dir1");

			Assert.IsFalse(result);
		}
		#endregion IsBaseOf


		#region TerminateDirectoryPath
		[TestMethod()]
		public void TerminateDirectoryPath_NotAlreadyTerminatedTest() {
			var path = @"c:\dir";
			var result = FileHelper.TerminateDirectoryPath(path);

			Assert.AreEqual($"{path}\\", result);
		}

		[TestMethod()]
		public void TerminateDirectoryPath_AlreadyTerminatedTest() {
			var path = @"c:\dir\";
			var result = FileHelper.TerminateDirectoryPath(path);

			Assert.AreEqual(path, result);
		}
		#endregion TerminateDirectoryPath


		#region AreEqual
		[TestMethod()]
		public void AreEqual_SimpleTest() {
			var result = FileHelper.AreEqual(@"c:\dir1\", @"c:\dir1\");

			Assert.IsTrue(result);
		}

		public void AreEqual_Simple_FailTest() {
			var result = FileHelper.AreEqual(@"c:\dir1\", @"c:\dir1\dir2\");

			Assert.IsFalse(result);
		}

		[TestMethod()]
		public void AreEqual_RelativeTest() {
			var result = FileHelper.AreEqual(@"c:\dir1\", @"c:\temp\..\dir1\");

			Assert.IsTrue(result);
		}

		[TestMethod()]
		public void AreEqual_UNC_NotLocalTest() {
			var result = FileHelper.AreEqual(@"\\share\doesnt\exist", @"\\share\doesnt\exist");

			Assert.IsTrue(result);
		}

		//[TestMethod()]
		//public void AreEqual_UNC_IsLocalTest() {
		//	var result = FileHelper.AreEqual(@"\\share\does\exist", @"c:\share_root\does\exist");

		//	Assert.IsFalse(result);
		//}

		[TestMethod()]
		public void AreEqual_CaseInsensitiveTest() {
			var result = FileHelper.AreEqual(@"c:\dir1\", @"C:\DIR1\");

			Assert.IsTrue(result);
		}

		[TestMethod()]
		public void AreEqual_UnterminatedFolderTest() {
			var result = FileHelper.AreEqual(@"c:\dir1\", @"c:\dir1");

			Assert.IsTrue(result);
		}

		[TestMethod()]
		public void AreEqual_FormatTest() {
			var result = FileHelper.AreEqual(@"c:\dir1\", @"c:/dir1/");

			Assert.IsTrue(result);
		}
		#endregion AreEqual


		#region NormalizePath
		[TestMethod()]
		public void NormalizePath_SimpleTest() {
			var path = @"c:\dir1";
			var result = FileHelper.NormalizePath(path);

			Assert.AreEqual(result, path);
		}

		[TestMethod()]
		public void NormalizePath_RelativeTest() {
			var relative = @"c:\temp\..\dir1\";
			var absolute = @"c:\dir1\";
			var result = FileHelper.NormalizePath(relative);

			Assert.AreEqual(result, absolute);
		}

		[TestMethod()]
		public void NormalizePath_UNC_NotLocalTest() {
			var unc = @"\\share\doesnt\exist";
			var result = FileHelper.NormalizePath(unc);

			Assert.AreEqual(result, unc);
		}

		//[TestMethod()]
		//public void NormalizePath_UNC_IsLocalTest() {
		//	var result = FileHelper.NormalizePath(@"\\share\does\exist", @"c:\share_root\does\exist");

		//	Assert.IsFalse(result);
		//}

		[TestMethod()]
		public void NormalizePath_TerminatedFolderTest() {
			var terminated = @"c:\dir1\a\b\c\";
			var result = FileHelper.NormalizePath(terminated);

			Assert.AreEqual(result, terminated);
		}

		[TestMethod()]
		public void NormalizePath_UnterminatedFolderTest() {
			var path = @"c:\dir1\a\b\c";
			var result = FileHelper.NormalizePath(path);

			Assert.AreEqual(result, path);
		}

		[TestMethod()]
		public void NormalizePath_FormatTest() {
			var std = @"c:\dir1\a\b\c\";
			var alt = @"c:/dir1/a/b/c/";
			var result = FileHelper.NormalizePath(alt);

			Assert.AreEqual(result, std);
		}
		#endregion NormalizePath
	}
}