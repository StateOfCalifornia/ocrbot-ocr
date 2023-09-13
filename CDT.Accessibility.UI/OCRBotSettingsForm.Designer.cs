namespace CDT.Accessibility.UI {
	partial class OCRBotSettingsForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.labelEndPoint = new System.Windows.Forms.Label();
			this.textBoxEndPoint = new System.Windows.Forms.TextBox();
			this.textBoxKey = new System.Windows.Forms.TextBox();
			this.labelKey = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// labelEndPoint
			// 
			this.labelEndPoint.AutoSize = true;
			this.labelEndPoint.Location = new System.Drawing.Point(12, 22);
			this.labelEndPoint.Name = "labelEndPoint";
			this.labelEndPoint.Size = new System.Drawing.Size(92, 13);
			this.labelEndPoint.TabIndex = 0;
			this.labelEndPoint.Text = "Service EndPoint:";
			// 
			// textBoxEndPoint
			// 
			this.textBoxEndPoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxEndPoint.HideSelection = false;
			this.textBoxEndPoint.Location = new System.Drawing.Point(106, 19);
			this.textBoxEndPoint.Name = "textBoxEndPoint";
			this.textBoxEndPoint.Size = new System.Drawing.Size(339, 20);
			this.textBoxEndPoint.TabIndex = 1;
			// 
			// textBoxKey
			// 
			this.textBoxKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxKey.HideSelection = false;
			this.textBoxKey.Location = new System.Drawing.Point(106, 45);
			this.textBoxKey.Name = "textBoxKey";
			this.textBoxKey.Size = new System.Drawing.Size(339, 20);
			this.textBoxKey.TabIndex = 3;
			this.textBoxKey.TextChanged += new System.EventHandler(this.textBoxKey_TextChanged);
			// 
			// labelKey
			// 
			this.labelKey.AutoSize = true;
			this.labelKey.Location = new System.Drawing.Point(33, 48);
			this.labelKey.Name = "labelKey";
			this.labelKey.Size = new System.Drawing.Size(67, 13);
			this.labelKey.TabIndex = 2;
			this.labelKey.Text = "Service Key:";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Location = new System.Drawing.Point(369, 76);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Location = new System.Drawing.Point(288, 76);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// OCRBotSettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 111);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textBoxKey);
			this.Controls.Add(this.labelKey);
			this.Controls.Add(this.textBoxEndPoint);
			this.Controls.Add(this.labelEndPoint);
			this.Name = "OCRBotSettingsForm";
			this.Text = "OCRBot Settings";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

        #endregion

        private System.Windows.Forms.Label labelEndPoint;
        private System.Windows.Forms.TextBox textBoxEndPoint;
        private System.Windows.Forms.TextBox textBoxKey;
        private System.Windows.Forms.Label labelKey;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}