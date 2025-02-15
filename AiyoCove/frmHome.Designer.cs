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
            wavFileDialog = new OpenFileDialog();
            txtWavFile = new TextBox();
            btnBrowse = new Button();
            txtResult = new TextBox();
            SuspendLayout();
            // 
            // wavFileDialog
            // 
            wavFileDialog.FileName = "openFileDialog1";
            // 
            // txtWavFile
            // 
            txtWavFile.Location = new Point(12, 12);
            txtWavFile.Name = "txtWavFile";
            txtWavFile.Size = new Size(335, 24);
            txtWavFile.TabIndex = 1;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(353, 12);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(137, 24);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // txtResult
            // 
            txtResult.Location = new Point(12, 42);
            txtResult.Multiline = true;
            txtResult.Name = "txtResult";
            txtResult.ScrollBars = ScrollBars.Vertical;
            txtResult.Size = new Size(680, 333);
            txtResult.TabIndex = 3;
            // 
            // frmHome
            // 
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(704, 387);
            Controls.Add(txtResult);
            Controls.Add(btnBrowse);
            Controls.Add(txtWavFile);
            Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "frmHome";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private OpenFileDialog wavFileDialog;
        private TextBox txtWavFile;
        private Button btnBrowse;
        private TextBox txtResult;
    }
}
