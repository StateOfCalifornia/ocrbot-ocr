using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CDT.Accessibility.UI {
	public partial class ReleaseNotesForm : Form {
		public ReleaseNotesForm(string html) {
			InitializeComponent();
			webBrowser.DocumentText = html;
			
		}

		private void buttonClose_Click(object sender, EventArgs e) {
			this.Close();
		}
	}
}
