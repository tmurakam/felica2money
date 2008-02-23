namespace FeliCa2Money
{
    partial class OptionDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkIgnoreZeroTransaction = new System.Windows.Forms.CheckBox();
            this.checkManualOfxPath = new System.Windows.Forms.CheckBox();
            this.checkAutoKickOfxFile = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textSfcPeepPath = new System.Windows.Forms.TextBox();
            this.buttonSfcPath = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // checkIgnoreZeroTransaction
            // 
            this.checkIgnoreZeroTransaction.AutoSize = true;
            this.checkIgnoreZeroTransaction.Location = new System.Drawing.Point(24, 23);
            this.checkIgnoreZeroTransaction.Name = "checkIgnoreZeroTransaction";
            this.checkIgnoreZeroTransaction.Size = new System.Drawing.Size(158, 16);
            this.checkIgnoreZeroTransaction.TabIndex = 0;
            this.checkIgnoreZeroTransaction.Text = "金額が 0 の取引を無視する";
            this.checkIgnoreZeroTransaction.UseVisualStyleBackColor = true;
            // 
            // checkManualOfxPath
            // 
            this.checkManualOfxPath.AutoSize = true;
            this.checkManualOfxPath.Location = new System.Drawing.Point(24, 56);
            this.checkManualOfxPath.Name = "checkManualOfxPath";
            this.checkManualOfxPath.Size = new System.Drawing.Size(178, 16);
            this.checkManualOfxPath.TabIndex = 1;
            this.checkManualOfxPath.Text = "OFXファイル名を手動で指定する";
            this.checkManualOfxPath.UseVisualStyleBackColor = true;
            // 
            // checkAutoKickOfxFile
            // 
            this.checkAutoKickOfxFile.AutoSize = true;
            this.checkAutoKickOfxFile.Location = new System.Drawing.Point(24, 89);
            this.checkAutoKickOfxFile.Name = "checkAutoKickOfxFile";
            this.checkAutoKickOfxFile.Size = new System.Drawing.Size(222, 16);
            this.checkAutoKickOfxFile.TabIndex = 2;
            this.checkAutoKickOfxFile.Text = "保存後にOFXファイルを自動的に起動する";
            this.checkAutoKickOfxFile.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 136);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "SFCPeep.exeの場所";
            // 
            // textSfcPeepPath
            // 
            this.textSfcPeepPath.Location = new System.Drawing.Point(134, 133);
            this.textSfcPeepPath.Name = "textSfcPeepPath";
            this.textSfcPeepPath.Size = new System.Drawing.Size(235, 19);
            this.textSfcPeepPath.TabIndex = 4;
            // 
            // buttonSfcPath
            // 
            this.buttonSfcPath.Location = new System.Drawing.Point(390, 131);
            this.buttonSfcPath.Name = "buttonSfcPath";
            this.buttonSfcPath.Size = new System.Drawing.Size(44, 23);
            this.buttonSfcPath.TabIndex = 5;
            this.buttonSfcPath.Text = "参照";
            this.buttonSfcPath.UseVisualStyleBackColor = true;
            this.buttonSfcPath.Click += new System.EventHandler(this.buttonSfcPath_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(268, 177);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(359, 177);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.Filter = "SFCPeep|SFCPeep.exe";
            this.openFileDialog.RestoreDirectory = true;
            // 
            // OptionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(462, 217);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonSfcPath);
            this.Controls.Add(this.textSfcPeepPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkAutoKickOfxFile);
            this.Controls.Add(this.checkManualOfxPath);
            this.Controls.Add(this.checkIgnoreZeroTransaction);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "OptionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "オプション";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkIgnoreZeroTransaction;
        private System.Windows.Forms.CheckBox checkManualOfxPath;
        private System.Windows.Forms.CheckBox checkAutoKickOfxFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textSfcPeepPath;
        private System.Windows.Forms.Button buttonSfcPath;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}