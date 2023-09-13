namespace CDT.Accessibility.UI {
	partial class MainForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lblInputDirectory = new System.Windows.Forms.Label();
            this.browseInputDirectoryButton = new System.Windows.Forms.Button();
            this.lblSelectedInputDirectory = new System.Windows.Forms.Label();
            this.lblSelectedOutputDirectory = new System.Windows.Forms.Label();
            this.browseOutputDirectoryButton = new System.Windows.Forms.Button();
            this.lblOutputDirectory = new System.Windows.Forms.Label();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.browseInputFileButton = new System.Windows.Forms.Button();
            this.groupBoxSource = new System.Windows.Forms.GroupBox();
            this.lblOr = new System.Windows.Forms.Label();
            this.groupBoxTarget = new System.Windows.Forms.GroupBox();
            this.dataGridViewLog = new System.Windows.Forms.DataGridView();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panelLogHeader = new System.Windows.Forms.Panel();
            this.chkInformationFilter = new System.Windows.Forms.CheckBox();
            this.chkErrorFilter = new System.Windows.Forms.CheckBox();
            this.chkWarningFilter = new System.Windows.Forms.CheckBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.grpBoxLog = new System.Windows.Forms.GroupBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OcrServiceEndpointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userGuideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataFlowStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.releaseNotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.privacyPolicyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fAQsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contactUsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.checkBoxAutoTag = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pagesProcessedLabel = new System.Windows.Forms.Label();
            this.labelProcessing = new System.Windows.Forms.Label();
            this.labelFilesProcessed = new System.Windows.Forms.Label();
            this.filesProcessedLabel = new System.Windows.Forms.Label();
            this.fileErrorsLabel = new System.Windows.Forms.Label();
            this.labelPagesProcessed = new System.Windows.Forms.Label();
            this.labelFileErrorCount = new System.Windows.Forms.Label();
            this.timeLabel = new System.Windows.Forms.Label();
            this.labelTime = new System.Windows.Forms.Label();
            this.checkBoxShowTextOnly = new System.Windows.Forms.CheckBox();
            this.statsTimer = new System.Windows.Forms.Timer(this.components);
            this.checkBoxConvertCMYK = new System.Windows.Forms.CheckBox();
            this.groupBoxSource.SuspendLayout();
            this.groupBoxTarget.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLog)).BeginInit();
            this.panelLogHeader.SuspendLayout();
            this.grpBoxLog.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblInputDirectory
            // 
            this.lblInputDirectory.AutoSize = true;
            this.lblInputDirectory.Location = new System.Drawing.Point(4, 24);
            this.lblInputDirectory.Name = "lblInputDirectory";
            this.lblInputDirectory.Size = new System.Drawing.Size(76, 13);
            this.lblInputDirectory.TabIndex = 2;
            this.lblInputDirectory.Text = "Input Directory";
            // 
            // browseInputDirectoryButton
            // 
            this.browseInputDirectoryButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.browseInputDirectoryButton.Location = new System.Drawing.Point(621, 19);
            this.browseInputDirectoryButton.Name = "browseInputDirectoryButton";
            this.browseInputDirectoryButton.Size = new System.Drawing.Size(100, 23);
            this.browseInputDirectoryButton.TabIndex = 3;
            this.browseInputDirectoryButton.Text = "Choose Folder";
            this.browseInputDirectoryButton.UseVisualStyleBackColor = true;
            this.browseInputDirectoryButton.Click += new System.EventHandler(this.BrowseInputDirectoryButton_Click);
            // 
            // lblSelectedInputDirectory
            // 
            this.lblSelectedInputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSelectedInputDirectory.Location = new System.Drawing.Point(92, 24);
            this.lblSelectedInputDirectory.Name = "lblSelectedInputDirectory";
            this.lblSelectedInputDirectory.Size = new System.Drawing.Size(503, 50);
            this.lblSelectedInputDirectory.TabIndex = 4;
            // 
            // lblSelectedOutputDirectory
            // 
            this.lblSelectedOutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSelectedOutputDirectory.Location = new System.Drawing.Point(95, 24);
            this.lblSelectedOutputDirectory.Name = "lblSelectedOutputDirectory";
            this.lblSelectedOutputDirectory.Size = new System.Drawing.Size(503, 26);
            this.lblSelectedOutputDirectory.TabIndex = 7;
            // 
            // browseOutputDirectoryButton
            // 
            this.browseOutputDirectoryButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.browseOutputDirectoryButton.Location = new System.Drawing.Point(621, 19);
            this.browseOutputDirectoryButton.Name = "browseOutputDirectoryButton";
            this.browseOutputDirectoryButton.Size = new System.Drawing.Size(100, 23);
            this.browseOutputDirectoryButton.TabIndex = 6;
            this.browseOutputDirectoryButton.Text = "Choose Folder";
            this.browseOutputDirectoryButton.UseVisualStyleBackColor = true;
            this.browseOutputDirectoryButton.Click += new System.EventHandler(this.BrowseOutputDirectoryButton_Click);
            // 
            // lblOutputDirectory
            // 
            this.lblOutputDirectory.AutoSize = true;
            this.lblOutputDirectory.Location = new System.Drawing.Point(3, 24);
            this.lblOutputDirectory.Name = "lblOutputDirectory";
            this.lblOutputDirectory.Size = new System.Drawing.Size(84, 13);
            this.lblOutputDirectory.TabIndex = 5;
            this.lblOutputDirectory.Text = "Output Directory";
            // 
            // btnSubmit
            // 
            this.btnSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSubmit.Enabled = false;
            this.btnSubmit.Location = new System.Drawing.Point(653, 215);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 23);
            this.btnSubmit.TabIndex = 8;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.BtnSubmit_Click);
            // 
            // browseInputFileButton
            // 
            this.browseInputFileButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.browseInputFileButton.Location = new System.Drawing.Point(621, 48);
            this.browseInputFileButton.Name = "browseInputFileButton";
            this.browseInputFileButton.Size = new System.Drawing.Size(100, 23);
            this.browseInputFileButton.TabIndex = 10;
            this.browseInputFileButton.Text = "Choose File(s)";
            this.browseInputFileButton.UseVisualStyleBackColor = true;
            this.browseInputFileButton.Click += new System.EventHandler(this.browseInputFileButton_Click);
            // 
            // groupBoxSource
            // 
            this.groupBoxSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSource.Controls.Add(this.lblOr);
            this.groupBoxSource.Controls.Add(this.lblSelectedInputDirectory);
            this.groupBoxSource.Controls.Add(this.lblInputDirectory);
            this.groupBoxSource.Controls.Add(this.browseInputFileButton);
            this.groupBoxSource.Controls.Add(this.browseInputDirectoryButton);
            this.groupBoxSource.Location = new System.Drawing.Point(12, 37);
            this.groupBoxSource.Name = "groupBoxSource";
            this.groupBoxSource.Size = new System.Drawing.Size(728, 86);
            this.groupBoxSource.TabIndex = 12;
            this.groupBoxSource.TabStop = false;
            this.groupBoxSource.Text = "Source Location";
            // 
            // lblOr
            // 
            this.lblOr.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblOr.AutoSize = true;
            this.lblOr.Location = new System.Drawing.Point(601, 53);
            this.lblOr.Name = "lblOr";
            this.lblOr.Size = new System.Drawing.Size(18, 13);
            this.lblOr.TabIndex = 12;
            this.lblOr.Text = "Or";
            // 
            // groupBoxTarget
            // 
            this.groupBoxTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTarget.Controls.Add(this.browseOutputDirectoryButton);
            this.groupBoxTarget.Controls.Add(this.lblOutputDirectory);
            this.groupBoxTarget.Controls.Add(this.lblSelectedOutputDirectory);
            this.groupBoxTarget.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBoxTarget.Location = new System.Drawing.Point(12, 139);
            this.groupBoxTarget.Name = "groupBoxTarget";
            this.groupBoxTarget.Size = new System.Drawing.Size(728, 64);
            this.groupBoxTarget.TabIndex = 13;
            this.groupBoxTarget.TabStop = false;
            this.groupBoxTarget.Text = "Target Location";
            // 
            // dataGridViewLog
            // 
            this.dataGridViewLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLog.Location = new System.Drawing.Point(8, 54);
            this.dataGridViewLog.Name = "dataGridViewLog";
            this.dataGridViewLog.ReadOnly = true;
            this.dataGridViewLog.RowHeadersVisible = false;
            this.dataGridViewLog.Size = new System.Drawing.Size(714, 219);
            this.dataGridViewLog.TabIndex = 14;
            this.dataGridViewLog.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewLog_ColumnHeaderMouseClick);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 20000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // panelLogHeader
            // 
            this.panelLogHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelLogHeader.Controls.Add(this.chkInformationFilter);
            this.panelLogHeader.Controls.Add(this.chkErrorFilter);
            this.panelLogHeader.Controls.Add(this.chkWarningFilter);
            this.panelLogHeader.Location = new System.Drawing.Point(7, 24);
            this.panelLogHeader.Name = "panelLogHeader";
            this.panelLogHeader.Size = new System.Drawing.Size(714, 24);
            this.panelLogHeader.TabIndex = 25;
            // 
            // chkInformationFilter
            // 
            this.chkInformationFilter.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkInformationFilter.AutoSize = true;
            this.chkInformationFilter.Checked = true;
            this.chkInformationFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInformationFilter.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkInformationFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.chkInformationFilter.Image = global::CDT.Accessibility.UI.Properties.Resources.information;
            this.chkInformationFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkInformationFilter.Location = new System.Drawing.Point(161, 0);
            this.chkInformationFilter.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.chkInformationFilter.Name = "chkInformationFilter";
            this.chkInformationFilter.Size = new System.Drawing.Size(104, 24);
            this.chkInformationFilter.TabIndex = 24;
            this.chkInformationFilter.Text = "0 Informational";
            this.chkInformationFilter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkInformationFilter.UseVisualStyleBackColor = true;
            this.chkInformationFilter.CheckedChanged += new System.EventHandler(this.LogFilter_CheckedChanged);
            // 
            // chkErrorFilter
            // 
            this.chkErrorFilter.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkErrorFilter.AutoSize = true;
            this.chkErrorFilter.Checked = true;
            this.chkErrorFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkErrorFilter.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkErrorFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.chkErrorFilter.Image = global::CDT.Accessibility.UI.Properties.Resources.error;
            this.chkErrorFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkErrorFilter.Location = new System.Drawing.Point(87, 0);
            this.chkErrorFilter.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.chkErrorFilter.Name = "chkErrorFilter";
            this.chkErrorFilter.Size = new System.Drawing.Size(74, 24);
            this.chkErrorFilter.TabIndex = 22;
            this.chkErrorFilter.Text = "0 Errors";
            this.chkErrorFilter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkErrorFilter.UseVisualStyleBackColor = true;
            this.chkErrorFilter.CheckedChanged += new System.EventHandler(this.LogFilter_CheckedChanged);
            // 
            // chkWarningFilter
            // 
            this.chkWarningFilter.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkWarningFilter.AutoSize = true;
            this.chkWarningFilter.Checked = true;
            this.chkWarningFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWarningFilter.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkWarningFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.chkWarningFilter.Image = global::CDT.Accessibility.UI.Properties.Resources.warn;
            this.chkWarningFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkWarningFilter.Location = new System.Drawing.Point(0, 0);
            this.chkWarningFilter.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.chkWarningFilter.Name = "chkWarningFilter";
            this.chkWarningFilter.Size = new System.Drawing.Size(87, 24);
            this.chkWarningFilter.TabIndex = 23;
            this.chkWarningFilter.Text = "0 Warnings";
            this.chkWarningFilter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.chkWarningFilter.UseVisualStyleBackColor = true;
            this.chkWarningFilter.CheckedChanged += new System.EventHandler(this.LogFilter_CheckedChanged);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearLog.Location = new System.Drawing.Point(647, 282);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(75, 23);
            this.btnClearLog.TabIndex = 25;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // grpBoxLog
            // 
            this.grpBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpBoxLog.Controls.Add(this.btnClearLog);
            this.grpBoxLog.Controls.Add(this.panelLogHeader);
            this.grpBoxLog.Controls.Add(this.dataGridViewLog);
            this.grpBoxLog.Location = new System.Drawing.Point(12, 242);
            this.grpBoxLog.Name = "grpBoxLog";
            this.grpBoxLog.Size = new System.Drawing.Size(728, 314);
            this.grpBoxLog.TabIndex = 18;
            this.grpBoxLog.TabStop = false;
            this.grpBoxLog.Text = "Logs";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(752, 24);
            this.menuStrip1.TabIndex = 20;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OcrServiceEndpointToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // OcrServiceEndpointToolStripMenuItem
            // 
            this.OcrServiceEndpointToolStripMenuItem.Name = "OcrServiceEndpointToolStripMenuItem";
            this.OcrServiceEndpointToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.OcrServiceEndpointToolStripMenuItem.Text = "OCR Service Endpoint";
            this.OcrServiceEndpointToolStripMenuItem.Click += new System.EventHandler(this.OcrServiceEndpointToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userGuideToolStripMenuItem,
            this.dataFlowStripMenuItem,
            this.releaseNotesToolStripMenuItem,
            this.privacyPolicyToolStripMenuItem,
            this.fAQsToolStripMenuItem,
            this.contactUsToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // userGuideToolStripMenuItem
            // 
            this.userGuideToolStripMenuItem.Name = "userGuideToolStripMenuItem";
            this.userGuideToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.userGuideToolStripMenuItem.Text = "User Guide";
            this.userGuideToolStripMenuItem.Click += new System.EventHandler(this.userGuideToolStripMenuItem_Click);
            // 
            // dataFlowStripMenuItem
            // 
            this.dataFlowStripMenuItem.Name = "dataFlowStripMenuItem";
            this.dataFlowStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.dataFlowStripMenuItem.Text = "Data Flow";
            this.dataFlowStripMenuItem.Click += new System.EventHandler(this.dataFlowToolStripMenuItem_Click);
            // 
            // releaseNotesToolStripMenuItem
            // 
            this.releaseNotesToolStripMenuItem.Name = "releaseNotesToolStripMenuItem";
            this.releaseNotesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.releaseNotesToolStripMenuItem.Text = "Release Notes";
            this.releaseNotesToolStripMenuItem.Click += new System.EventHandler(this.releaseNotesToolStripMenuItem_Click);
            // 
            // privacyPolicyToolStripMenuItem
            // 
            this.privacyPolicyToolStripMenuItem.Name = "privacyPolicyToolStripMenuItem";
            this.privacyPolicyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.privacyPolicyToolStripMenuItem.Text = "Privacy Policy";
            this.privacyPolicyToolStripMenuItem.Click += new System.EventHandler(this.privacyPolicyToolStripMenuItem_Click);
            // 
            // fAQsToolStripMenuItem
            // 
            this.fAQsToolStripMenuItem.Name = "fAQsToolStripMenuItem";
            this.fAQsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.fAQsToolStripMenuItem.Text = "FAQs";
            this.fAQsToolStripMenuItem.Click += new System.EventHandler(this.fAQsToolStripMenuItem_Click);
            // 
            // contactUsToolStripMenuItem
            // 
            this.contactUsToolStripMenuItem.Name = "contactUsToolStripMenuItem";
            this.contactUsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.contactUsToolStripMenuItem.Text = "About";
            this.contactUsToolStripMenuItem.Click += new System.EventHandler(this.contactUsToolStripMenuItem_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Multiselect = true;
            // 
            // checkBoxAutoTag
            // 
            this.checkBoxAutoTag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAutoTag.Location = new System.Drawing.Point(566, 215);
            this.checkBoxAutoTag.Name = "checkBoxAutoTag";
            this.checkBoxAutoTag.Size = new System.Drawing.Size(79, 23);
            this.checkBoxAutoTag.TabIndex = 0;
            this.checkBoxAutoTag.Text = "AutoTag";
            this.checkBoxAutoTag.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 9;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.pagesProcessedLabel, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelProcessing, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelFilesProcessed, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.filesProcessedLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.fileErrorsLabel, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelPagesProcessed, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelFileErrorCount, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.timeLabel, 7, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelTime, 8, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 562);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(728, 22);
            this.tableLayoutPanel1.TabIndex = 21;
            // 
            // pagesProcessedLabel
            // 
            this.pagesProcessedLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.pagesProcessedLabel.AutoSize = true;
            this.pagesProcessedLabel.Location = new System.Drawing.Point(355, 4);
            this.pagesProcessedLabel.Name = "pagesProcessedLabel";
            this.pagesProcessedLabel.Size = new System.Drawing.Size(108, 13);
            this.pagesProcessedLabel.TabIndex = 20;
            this.pagesProcessedLabel.Text = "     Pages Processed:";
            this.pagesProcessedLabel.UseMnemonic = false;
            this.pagesProcessedLabel.UseWaitCursor = true;
            // 
            // labelProcessing
            // 
            this.labelProcessing.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelProcessing.AutoSize = true;
            this.labelProcessing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProcessing.Location = new System.Drawing.Point(3, 4);
            this.labelProcessing.Name = "labelProcessing";
            this.labelProcessing.Size = new System.Drawing.Size(81, 13);
            this.labelProcessing.TabIndex = 24;
            this.labelProcessing.Text = "Processing...";
            this.labelProcessing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelFilesProcessed
            // 
            this.labelFilesProcessed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelFilesProcessed.AutoSize = true;
            this.labelFilesProcessed.Location = new System.Drawing.Point(336, 4);
            this.labelFilesProcessed.Name = "labelFilesProcessed";
            this.labelFilesProcessed.Size = new System.Drawing.Size(13, 13);
            this.labelFilesProcessed.TabIndex = 23;
            this.labelFilesProcessed.Text = "0";
            this.labelFilesProcessed.UseMnemonic = false;
            this.labelFilesProcessed.UseWaitCursor = true;
            // 
            // filesProcessedLabel
            // 
            this.filesProcessedLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.filesProcessedLabel.AutoSize = true;
            this.filesProcessedLabel.Location = new System.Drawing.Point(246, 4);
            this.filesProcessedLabel.Name = "filesProcessedLabel";
            this.filesProcessedLabel.Size = new System.Drawing.Size(84, 13);
            this.filesProcessedLabel.TabIndex = 22;
            this.filesProcessedLabel.Text = "Files Processed:";
            this.filesProcessedLabel.UseMnemonic = false;
            this.filesProcessedLabel.UseWaitCursor = true;
            // 
            // fileErrorsLabel
            // 
            this.fileErrorsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.fileErrorsLabel.AutoSize = true;
            this.fileErrorsLabel.Location = new System.Drawing.Point(488, 4);
            this.fileErrorsLabel.Name = "fileErrorsLabel";
            this.fileErrorsLabel.Size = new System.Drawing.Size(71, 13);
            this.fileErrorsLabel.TabIndex = 25;
            this.fileErrorsLabel.Text = "     File Errors:";
            this.fileErrorsLabel.UseMnemonic = false;
            this.fileErrorsLabel.UseWaitCursor = true;
            // 
            // labelPagesProcessed
            // 
            this.labelPagesProcessed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelPagesProcessed.AutoSize = true;
            this.labelPagesProcessed.Location = new System.Drawing.Point(469, 4);
            this.labelPagesProcessed.Name = "labelPagesProcessed";
            this.labelPagesProcessed.Size = new System.Drawing.Size(13, 13);
            this.labelPagesProcessed.TabIndex = 24;
            this.labelPagesProcessed.Text = "0";
            this.labelPagesProcessed.UseMnemonic = false;
            this.labelPagesProcessed.UseWaitCursor = true;
            // 
            // labelFileErrorCount
            // 
            this.labelFileErrorCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelFileErrorCount.AutoSize = true;
            this.labelFileErrorCount.Location = new System.Drawing.Point(565, 4);
            this.labelFileErrorCount.Name = "labelFileErrorCount";
            this.labelFileErrorCount.Size = new System.Drawing.Size(13, 13);
            this.labelFileErrorCount.TabIndex = 26;
            this.labelFileErrorCount.Text = "0";
            this.labelFileErrorCount.UseMnemonic = false;
            this.labelFileErrorCount.UseWaitCursor = true;
            // 
            // timeLabel
            // 
            this.timeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.timeLabel.AutoSize = true;
            this.timeLabel.Location = new System.Drawing.Point(584, 4);
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(92, 13);
            this.timeLabel.TabIndex = 27;
            this.timeLabel.Text = "     Time(h:mm:ss):";
            // 
            // labelTime
            // 
            this.labelTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTime.AutoSize = true;
            this.labelTime.Location = new System.Drawing.Point(682, 4);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(43, 13);
            this.labelTime.TabIndex = 28;
            this.labelTime.Text = "0:00:00";
            // 
            // checkBoxShowTextOnly
            // 
            this.checkBoxShowTextOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxShowTextOnly.Location = new System.Drawing.Point(452, 215);
            this.checkBoxShowTextOnly.Name = "checkBoxShowTextOnly";
            this.checkBoxShowTextOnly.Size = new System.Drawing.Size(106, 23);
            this.checkBoxShowTextOnly.TabIndex = 22;
            this.checkBoxShowTextOnly.Text = "Text Only Mode";
            this.checkBoxShowTextOnly.UseVisualStyleBackColor = true;
            // 
            // statsTimer
            // 
            this.statsTimer.Interval = 1000;
            this.statsTimer.Tick += new System.EventHandler(this.statsTimer_Tick);
            // 
            // checkBoxConvertCMYK
            // 
            this.checkBoxConvertCMYK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxConvertCMYK.Location = new System.Drawing.Point(322, 215);
            this.checkBoxConvertCMYK.Name = "checkBoxConvertCMYK";
            this.checkBoxConvertCMYK.Size = new System.Drawing.Size(124, 23);
            this.checkBoxConvertCMYK.TabIndex = 23;
            this.checkBoxConvertCMYK.Text = "Convert To Image";
            this.checkBoxConvertCMYK.UseVisualStyleBackColor = true;
            this.checkBoxConvertCMYK.CheckedChanged += new System.EventHandler(this.checkBoxConvertCMYK_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 590);
            this.Controls.Add(this.checkBoxConvertCMYK);
            this.Controls.Add(this.checkBoxShowTextOnly);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.checkBoxAutoTag);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.grpBoxLog);
            this.Controls.Add(this.groupBoxTarget);
            this.Controls.Add(this.groupBoxSource);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Microsoft® Visual Studio®";
            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            this.groupBoxTarget.ResumeLayout(false);
            this.groupBoxTarget.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLog)).EndInit();
            this.panelLogHeader.ResumeLayout(false);
            this.panelLogHeader.PerformLayout();
            this.grpBoxLog.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label lblInputDirectory;
		private System.Windows.Forms.Button browseInputDirectoryButton;
		private System.Windows.Forms.Label lblSelectedInputDirectory;
		private System.Windows.Forms.Label lblSelectedOutputDirectory;
		private System.Windows.Forms.Button browseOutputDirectoryButton;
		private System.Windows.Forms.Label lblOutputDirectory;
		private System.Windows.Forms.Button btnSubmit;
		private System.Windows.Forms.Button browseInputFileButton;
		private System.Windows.Forms.GroupBox groupBoxSource;
		private System.Windows.Forms.GroupBox groupBoxTarget;
		private System.Windows.Forms.Label lblOr;
		private System.Windows.Forms.DataGridView dataGridViewLog;
		private System.Windows.Forms.GroupBox grpBoxLog;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.CheckBox chkErrorFilter;
		private System.Windows.Forms.CheckBox chkInformationFilter;
		private System.Windows.Forms.CheckBox chkWarningFilter;
		private System.Windows.Forms.Panel panelLogHeader;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem userGuideToolStripMenuItem;
		private System.Windows.Forms.Button btnClearLog;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ToolStripMenuItem contactUsToolStripMenuItem;
		private System.Windows.Forms.CheckBox checkBoxAutoTag;
		private System.Windows.Forms.ToolStripMenuItem releaseNotesToolStripMenuItem;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.CheckBox checkBoxShowTextOnly;
		private System.Windows.Forms.ToolStripMenuItem dataFlowStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
		private System.Windows.Forms.Label pagesProcessedLabel;
		private System.Windows.Forms.Label labelFilesProcessed;
		private System.Windows.Forms.Label filesProcessedLabel;
		private System.Windows.Forms.Label labelPagesProcessed;
		private System.Windows.Forms.Label fileErrorsLabel;
		private System.Windows.Forms.Label labelFileErrorCount;
        private System.Windows.Forms.ToolStripMenuItem OcrServiceEndpointToolStripMenuItem;
		private System.Windows.Forms.Label timeLabel;
		private System.Windows.Forms.Label labelTime;
		private System.Windows.Forms.Label labelProcessing;
		private System.Windows.Forms.Timer statsTimer;
		private System.Windows.Forms.ToolStripMenuItem privacyPolicyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fAQsToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxConvertCMYK;
    }
}

