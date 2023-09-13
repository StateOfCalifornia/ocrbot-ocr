using CDT.Accessibility.Documents;
using CDT.Data;
using CDT.Windows;
using System;

using System.Windows.Forms;

namespace CDT.Accessibility.UI {
	public partial class OCRBotSettingsForm : Form {
		private SessionConfig sessionConfig;
		public OCRBotSettingsForm(SessionConfig config) {
			InitializeComponent();
			sessionConfig = config;
			textBoxEndPoint.Text = sessionConfig.ServiceEndpoint;
			textBoxKey.Text = sessionConfig.SubscriptionKey;
			toolTip1.SetToolTip(textBoxKey, Program.OCRBotSettings.SubscriptionKey);
		}

		private void buttonOK_Click(object sender, EventArgs e) {
			sessionConfig.SubscriptionKey = textBoxKey.Text;
			sessionConfig.ServiceEndpoint = textBoxEndPoint.Text;
			Program.SaveOCRBotSettings(sessionConfig);
			this.Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void textBoxKey_TextChanged(object sender, EventArgs e) {
			toolTip1.SetToolTip(textBoxKey, textBoxKey.Text);
		}
	}
}
