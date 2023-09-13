using System;
using System.Reflection;
using System.Windows.Forms;

namespace CDT.Accessibility.UI {
	partial class AboutBox : Form {
		internal static string ContactPhoneNumber => "(916) 464-4311";

		public AboutBox() {
			InitializeComponent();
			this.Text = $"About {AssemblyTitle}";
			this.labelProductName.Text = AssemblyProduct;
			this.labelPhoneNumber.Text = $"Service Desk: {ContactPhoneNumber}";
			this.labelVersion.Text = $"Version {Program.ApplicationVersion}";
			this.labelCopyright.Text = $"{AssemblyCopyright} (AGPL)";
			this.labelCompanyName.Text = AssemblyCompany;
			this.textBoxDescription.Text = Description;
			this.labelAcrobatVersion.Text = Program.AcrobatVersion;
			this.labelEverMapInstalled.Text = Program.IsAutoTagAvailable ? "Yes" : "No";
		}

		#region Assembly Attribute Accessors

		public string AssemblyTitle {
			get {
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0) {
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (titleAttribute.Title != "") {
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public string AssemblyVersion {
			get {
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public string AssemblyDescription {
			get {
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if (attributes.Length == 0) {
					return "";
				}
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		public string AssemblyProduct {
			get {
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (attributes.Length == 0) {
					return "";
				}
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		public string AssemblyCopyright {
			get {
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (attributes.Length == 0) {
					return "";
				}
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		public string AssemblyCompany {
			get {
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (attributes.Length == 0) {
					return "";
				}
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}
		#endregion

		public string Description {
			get {
				var description =
					$"{AssemblyDescription}{System.Environment.NewLine}" +
					$"{System.Environment.NewLine}" +
					$"Using {Program.LibraryInfo}{System.Environment.NewLine}";
				return description;
			}
		}

		private void okButton_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void labelSourceCode_Click(object sender, EventArgs e) {
			Program.GetSourceCode();
		}
	}
}
