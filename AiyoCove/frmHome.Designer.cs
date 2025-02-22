namespace AiyoCove
{
    partial class frmHome
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHome));
            btnLoad = new Button();
            wavFileDialog = new OpenFileDialog();
            txtWavFile = new TextBox();
            btnBrowse = new Button();
            txtResult = new TextBox();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            label3 = new Label();
            tabPage2 = new TabPage();
            btPoseDetectModelPathBrowse = new Button();
            label4 = new Label();
            txtPoseDetectModelPath = new TextBox();
            btnPoseDDetectStart = new Button();
            cbPoseModel = new ComboBox();
            label2 = new Label();
            label1 = new Label();
            txtPoseDetectMessages = new TextBox();
            txtPoseImagesPath = new TextBox();
            btnPoseImagesPath = new Button();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(8, 26);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(137, 24);
            btnLoad.TabIndex = 0;
            btnLoad.Text = "Load Model";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // wavFileDialog
            // 
            wavFileDialog.FileName = "openFileDialog1";
            // 
            // txtWavFile
            // 
            txtWavFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtWavFile.Location = new Point(343, 26);
            txtWavFile.Name = "txtWavFile";
            txtWavFile.Size = new Size(276, 24);
            txtWavFile.TabIndex = 1;
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Location = new Point(625, 26);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(137, 24);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // txtResult
            // 
            txtResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtResult.Location = new Point(8, 56);
            txtResult.Multiline = true;
            txtResult.Name = "txtResult";
            txtResult.ScrollBars = ScrollBars.Vertical;
            txtResult.Size = new Size(754, 368);
            txtResult.TabIndex = 3;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(778, 462);
            tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(btnLoad);
            tabPage1.Controls.Add(txtResult);
            tabPage1.Controls.Add(txtWavFile);
            tabPage1.Controls.Add(btnBrowse);
            tabPage1.Location = new Point(4, 26);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(770, 432);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Whisper";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(270, 30);
            label3.Name = "label3";
            label3.Size = new Size(67, 17);
            label3.TabIndex = 8;
            label3.Text = "Audio File";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(btPoseDetectModelPathBrowse);
            tabPage2.Controls.Add(label4);
            tabPage2.Controls.Add(txtPoseDetectModelPath);
            tabPage2.Controls.Add(btnPoseDDetectStart);
            tabPage2.Controls.Add(cbPoseModel);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(label1);
            tabPage2.Controls.Add(txtPoseDetectMessages);
            tabPage2.Controls.Add(txtPoseImagesPath);
            tabPage2.Controls.Add(btnPoseImagesPath);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(770, 434);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Pose Detection";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // btPoseDetectModelPathBrowse
            // 
            btPoseDetectModelPathBrowse.Location = new Point(635, 56);
            btPoseDetectModelPathBrowse.Name = "btPoseDetectModelPathBrowse";
            btPoseDetectModelPathBrowse.Size = new Size(88, 24);
            btPoseDetectModelPathBrowse.TabIndex = 13;
            btPoseDetectModelPathBrowse.Text = "Browse";
            btPoseDetectModelPathBrowse.UseVisualStyleBackColor = true;
            btPoseDetectModelPathBrowse.Click += btPoseDetectModelPathBrowse_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(285, 59);
            label4.Name = "label4";
            label4.Size = new Size(78, 17);
            label4.TabIndex = 12;
            label4.Text = "Model Path";
            // 
            // txtPoseDetectModelPath
            // 
            txtPoseDetectModelPath.Location = new Point(369, 56);
            txtPoseDetectModelPath.Name = "txtPoseDetectModelPath";
            txtPoseDetectModelPath.Size = new Size(260, 24);
            txtPoseDetectModelPath.TabIndex = 11;
            // 
            // btnPoseDDetectStart
            // 
            btnPoseDDetectStart.Location = new Point(546, 26);
            btnPoseDDetectStart.Name = "btnPoseDDetectStart";
            btnPoseDDetectStart.Size = new Size(88, 24);
            btnPoseDDetectStart.TabIndex = 10;
            btnPoseDDetectStart.Text = "Start";
            btnPoseDDetectStart.UseVisualStyleBackColor = true;
            btnPoseDDetectStart.Click += btnPoseDDetectStart_Click;
            // 
            // cbPoseModel
            // 
            cbPoseModel.FormattingEnabled = true;
            cbPoseModel.Items.AddRange(new object[] { "HRNetPose", "Movenet", "YOLO11" });
            cbPoseModel.Location = new Point(127, 56);
            cbPoseModel.Name = "cbPoseModel";
            cbPoseModel.Size = new Size(140, 25);
            cbPoseModel.TabIndex = 9;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(74, 60);
            label2.Name = "label2";
            label2.Size = new Size(47, 17);
            label2.TabIndex = 8;
            label2.Text = "Model";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 29);
            label1.Name = "label1";
            label1.Size = new Size(105, 17);
            label1.TabIndex = 7;
            label1.Text = "Image Directory";
            // 
            // txtPoseDetectMessages
            // 
            txtPoseDetectMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtPoseDetectMessages.Location = new Point(8, 87);
            txtPoseDetectMessages.Multiline = true;
            txtPoseDetectMessages.Name = "txtPoseDetectMessages";
            txtPoseDetectMessages.ScrollBars = ScrollBars.Vertical;
            txtPoseDetectMessages.Size = new Size(754, 293);
            txtPoseDetectMessages.TabIndex = 6;
            // 
            // txtPoseImagesPath
            // 
            txtPoseImagesPath.Location = new Point(127, 26);
            txtPoseImagesPath.Name = "txtPoseImagesPath";
            txtPoseImagesPath.Size = new Size(319, 24);
            txtPoseImagesPath.TabIndex = 4;
            // 
            // btnPoseImagesPath
            // 
            btnPoseImagesPath.Location = new Point(452, 26);
            btnPoseImagesPath.Name = "btnPoseImagesPath";
            btnPoseImagesPath.Size = new Size(88, 24);
            btnPoseImagesPath.TabIndex = 5;
            btnPoseImagesPath.Text = "Browse";
            btnPoseImagesPath.UseVisualStyleBackColor = true;
            btnPoseImagesPath.Click += btnPoseImagesPath_Click;
            // 
            // frmHome
            // 
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(778, 462);
            Controls.Add(tabControl1);
            Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "frmHome";
            Text = "AiyoCove Testing Bench";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnLoad;
        private OpenFileDialog wavFileDialog;
        private TextBox txtWavFile;
        private Button btnBrowse;
        private TextBox txtResult;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label label1;
        private TextBox txtPoseDetectMessages;
        private TextBox txtPoseImagesPath;
        private Button btnPoseImagesPath;
        private ComboBox cbPoseModel;
        private Label label2;
        private Button btnPoseDDetectStart;
        private Label label3;
        private Button btPoseDetectModelPathBrowse;
        private Label label4;
        private TextBox txtPoseDetectModelPath;
    }
}
