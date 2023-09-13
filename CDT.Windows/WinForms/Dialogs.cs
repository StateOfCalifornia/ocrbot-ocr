using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APICodePack = Microsoft.WindowsAPICodePack.Dialogs;

namespace CDT.Windows.WinForms {
	public class Dialogs {
		public static DirectoryInfo ShowFolderDialog(string initialDirectory = null) {
			DirectoryInfo folder = null;

			using (var dialog = new APICodePack.CommonOpenFileDialog()) {
				dialog.InitialDirectory = initialDirectory;
				dialog.IsFolderPicker = true;

				if (dialog.ShowDialog() == APICodePack.CommonFileDialogResult.Ok) {
					folder = new DirectoryInfo(dialog.FileName);
				}
			}

			return folder;
		}
	}
}
