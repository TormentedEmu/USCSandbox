namespace USC_Winbox
{
  partial class MainForm
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainerMain = new SplitContainer();
            tabControlMain = new TabControl();
            tabPageMain = new TabPage();
            btnBrowseFolders = new Button();
            lblPathID = new Label();
            lblAssetFilename = new Label();
            lblBundlePath = new Label();
            btnExtract = new Button();
            rtbAssetPathID = new RichTextBox();
            rtbAssetFileName = new RichTextBox();
            rtbBundleName = new RichTextBox();
            tabPageSettings = new TabPage();
            rtbLog = new RichTextBox();
            btbnClearLog = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            tabControlMain.SuspendLayout();
            tabPageMain.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(0, 0);
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Orientation = Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(tabControlMain);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(rtbLog);
            splitContainerMain.Panel2.Controls.Add(btbnClearLog);
            splitContainerMain.Size = new Size(800, 650);
            splitContainerMain.SplitterDistance = 400;
            splitContainerMain.SplitterWidth = 10;
            splitContainerMain.TabIndex = 0;
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabPageMain);
            tabControlMain.Controls.Add(tabPageSettings);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Location = new Point(0, 0);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(800, 400);
            tabControlMain.TabIndex = 0;
            // 
            // tabPageMain
            // 
            tabPageMain.Controls.Add(btnBrowseFolders);
            tabPageMain.Controls.Add(lblPathID);
            tabPageMain.Controls.Add(lblAssetFilename);
            tabPageMain.Controls.Add(lblBundlePath);
            tabPageMain.Controls.Add(btnExtract);
            tabPageMain.Controls.Add(rtbAssetPathID);
            tabPageMain.Controls.Add(rtbAssetFileName);
            tabPageMain.Controls.Add(rtbBundleName);
            tabPageMain.Location = new Point(4, 24);
            tabPageMain.Name = "tabPageMain";
            tabPageMain.Padding = new Padding(3);
            tabPageMain.Size = new Size(792, 372);
            tabPageMain.TabIndex = 0;
            tabPageMain.Text = "Menu";
            tabPageMain.UseVisualStyleBackColor = true;
            // 
            // btnBrowseFolders
            // 
            btnBrowseFolders.Location = new Point(552, 43);
            btnBrowseFolders.Name = "btnBrowseFolders";
            btnBrowseFolders.Size = new Size(75, 30);
            btnBrowseFolders.TabIndex = 8;
            btnBrowseFolders.Text = "Browse";
            btnBrowseFolders.UseVisualStyleBackColor = true;
            btnBrowseFolders.Click += btnBrowseFolders_Click;
            // 
            // lblPathID
            // 
            lblPathID.AutoSize = true;
            lblPathID.Location = new Point(8, 165);
            lblPathID.Name = "lblPathID";
            lblPathID.Size = new Size(79, 15);
            lblPathID.TabIndex = 7;
            lblPathID.Text = "Asset Path ID:";
            // 
            // lblAssetFilename
            // 
            lblAssetFilename.AutoSize = true;
            lblAssetFilename.Location = new Point(8, 88);
            lblAssetFilename.Name = "lblAssetFilename";
            lblAssetFilename.Size = new Size(94, 15);
            lblAssetFilename.TabIndex = 6;
            lblAssetFilename.Text = "Asset File Name:";
            // 
            // lblBundlePath
            // 
            lblBundlePath.AutoSize = true;
            lblBundlePath.Location = new Point(8, 25);
            lblBundlePath.Name = "lblBundlePath";
            lblBundlePath.Size = new Size(74, 15);
            lblBundlePath.TabIndex = 5;
            lblBundlePath.Text = "Bundle Path:";
            // 
            // btnExtract
            // 
            btnExtract.Location = new Point(629, 306);
            btnExtract.Name = "btnExtract";
            btnExtract.Size = new Size(155, 60);
            btnExtract.TabIndex = 4;
            btnExtract.Text = "Extract Shaders";
            btnExtract.UseVisualStyleBackColor = true;
            btnExtract.Click += btnExtract_Click;
            // 
            // rtbAssetPathID
            // 
            rtbAssetPathID.Location = new Point(8, 183);
            rtbAssetPathID.Multiline = false;
            rtbAssetPathID.Name = "rtbAssetPathID";
            rtbAssetPathID.Size = new Size(538, 30);
            rtbAssetPathID.TabIndex = 3;
            rtbAssetPathID.Text = "";
            // 
            // rtbAssetFileName
            // 
            rtbAssetFileName.Location = new Point(8, 106);
            rtbAssetFileName.Multiline = false;
            rtbAssetFileName.Name = "rtbAssetFileName";
            rtbAssetFileName.Size = new Size(538, 30);
            rtbAssetFileName.TabIndex = 2;
            rtbAssetFileName.Text = "";
            // 
            // rtbBundleName
            // 
            rtbBundleName.Location = new Point(8, 43);
            rtbBundleName.Multiline = false;
            rtbBundleName.Name = "rtbBundleName";
            rtbBundleName.Size = new Size(538, 30);
            rtbBundleName.TabIndex = 0;
            rtbBundleName.Text = "C:\\";
            // 
            // tabPageSettings
            // 
            tabPageSettings.Location = new Point(4, 24);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Padding = new Padding(3);
            tabPageSettings.Size = new Size(792, 372);
            tabPageSettings.TabIndex = 1;
            tabPageSettings.Text = "Settings";
            tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // rtbLog
            // 
            rtbLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbLog.HideSelection = false;
            rtbLog.Location = new Point(12, 3);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(695, 207);
            rtbLog.TabIndex = 1;
            rtbLog.Text = "";
            // 
            // btbnClearLog
            // 
            btbnClearLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btbnClearLog.Location = new Point(713, 3);
            btbnClearLog.Name = "btbnClearLog";
            btbnClearLog.Size = new Size(75, 36);
            btbnClearLog.TabIndex = 0;
            btbnClearLog.Text = "Clear Log";
            btbnClearLog.UseVisualStyleBackColor = true;
            btbnClearLog.Click += btbnClearLog_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 650);
            Controls.Add(splitContainerMain);
            Name = "MainForm";
            Text = "USC Winbox";
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            tabControlMain.ResumeLayout(false);
            tabPageMain.ResumeLayout(false);
            tabPageMain.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainerMain;
    private TabControl tabControlMain;
    private TabPage tabPageMain;
    private TabPage tabPageSettings;
    private RichTextBox rtbLog;
    private Button btbnClearLog;
    private TextBox textBox1;
    private RichTextBox rtbBundleName;
    private Button btnExtract;
    private RichTextBox rtbAssetPathID;
    private RichTextBox rtbAssetFileName;
        private Button btnBrowseFolders;
        private Label lblPathID;
        private Label lblAssetFilename;
        private Label lblBundlePath;
    }
}
