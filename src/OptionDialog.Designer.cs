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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.radioSuica = new System.Windows.Forms.RadioButton();
            this.radioIcoca = new System.Windows.Forms.RadioButton();
            this.radioIruca = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonCsvRulesUpdate = new System.Windows.Forms.Button();
            this.checkOfxVer2 = new System.Windows.Forms.CheckBox();
            this.buttonAssoc = new System.Windows.Forms.Button();
            this.buttonDeAssoc = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelCsvVersion = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkIgnoreZeroTransaction
            // 
            this.checkIgnoreZeroTransaction.AutoSize = true;
            this.checkIgnoreZeroTransaction.Location = new System.Drawing.Point(24, 94);
            this.checkIgnoreZeroTransaction.Name = "checkIgnoreZeroTransaction";
            this.checkIgnoreZeroTransaction.Size = new System.Drawing.Size(158, 16);
            this.checkIgnoreZeroTransaction.TabIndex = 1;
            this.checkIgnoreZeroTransaction.Text = "金額が 0 の取引を無視する";
            this.checkIgnoreZeroTransaction.UseVisualStyleBackColor = true;
            // 
            // checkManualOfxPath
            // 
            this.checkManualOfxPath.AutoSize = true;
            this.checkManualOfxPath.Location = new System.Drawing.Point(24, 116);
            this.checkManualOfxPath.Name = "checkManualOfxPath";
            this.checkManualOfxPath.Size = new System.Drawing.Size(178, 16);
            this.checkManualOfxPath.TabIndex = 2;
            this.checkManualOfxPath.Text = "OFXファイル名を手動で指定する";
            this.checkManualOfxPath.UseVisualStyleBackColor = true;
            // 
            // checkAutoKickOfxFile
            // 
            this.checkAutoKickOfxFile.AutoSize = true;
            this.checkAutoKickOfxFile.Location = new System.Drawing.Point(24, 138);
            this.checkAutoKickOfxFile.Name = "checkAutoKickOfxFile";
            this.checkAutoKickOfxFile.Size = new System.Drawing.Size(222, 16);
            this.checkAutoKickOfxFile.TabIndex = 3;
            this.checkAutoKickOfxFile.Text = "保存後にOFXファイルを自動的に起動する";
            this.checkAutoKickOfxFile.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(272, 318);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(357, 318);
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
            // radioSuica
            // 
            this.radioSuica.AutoSize = true;
            this.radioSuica.Checked = true;
            this.radioSuica.Location = new System.Drawing.Point(16, 18);
            this.radioSuica.Name = "radioSuica";
            this.radioSuica.Size = new System.Drawing.Size(96, 16);
            this.radioSuica.TabIndex = 0;
            this.radioSuica.TabStop = true;
            this.radioSuica.Text = "Suica/PASMO";
            this.radioSuica.UseVisualStyleBackColor = true;
            // 
            // radioIcoca
            // 
            this.radioIcoca.AutoSize = true;
            this.radioIcoca.Location = new System.Drawing.Point(134, 18);
            this.radioIcoca.Name = "radioIcoca";
            this.radioIcoca.Size = new System.Drawing.Size(58, 16);
            this.radioIcoca.TabIndex = 1;
            this.radioIcoca.Text = "ICOCA";
            this.radioIcoca.UseVisualStyleBackColor = true;
            // 
            // radioIruca
            // 
            this.radioIruca.AutoSize = true;
            this.radioIruca.Location = new System.Drawing.Point(231, 18);
            this.radioIruca.Name = "radioIruca";
            this.radioIruca.Size = new System.Drawing.Size(50, 16);
            this.radioIruca.TabIndex = 2;
            this.radioIruca.Text = "IruCa";
            this.radioIruca.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.radioSuica);
            this.groupBox1.Controls.Add(this.radioIruca);
            this.groupBox1.Controls.Add(this.radioIcoca);
            this.groupBox1.Location = new System.Drawing.Point(10, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 62);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "物販店舗検索優先エリア";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(265, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "※店舗コード重複時に優先するエリアを指定してください";
            // 
            // buttonCsvRulesUpdate
            // 
            this.buttonCsvRulesUpdate.Location = new System.Drawing.Point(24, 193);
            this.buttonCsvRulesUpdate.Name = "buttonCsvRulesUpdate";
            this.buttonCsvRulesUpdate.Size = new System.Drawing.Size(178, 34);
            this.buttonCsvRulesUpdate.TabIndex = 5;
            this.buttonCsvRulesUpdate.Text = "CSV変換定義のオンライン更新";
            this.buttonCsvRulesUpdate.UseVisualStyleBackColor = true;
            this.buttonCsvRulesUpdate.Click += new System.EventHandler(this.buttonCsvRulesUpdate_Click);
            // 
            // checkOfxVer2
            // 
            this.checkOfxVer2.AutoSize = true;
            this.checkOfxVer2.Location = new System.Drawing.Point(24, 160);
            this.checkOfxVer2.Name = "checkOfxVer2";
            this.checkOfxVer2.Size = new System.Drawing.Size(220, 16);
            this.checkOfxVer2.TabIndex = 4;
            this.checkOfxVer2.Text = "Ver 2.0 の OFX ファイル(XML)を生成する";
            this.checkOfxVer2.UseVisualStyleBackColor = true;
            // 
            // buttonAssoc
            // 
            this.buttonAssoc.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAssoc.Location = new System.Drawing.Point(14, 18);
            this.buttonAssoc.Name = "buttonAssoc";
            this.buttonAssoc.Size = new System.Drawing.Size(134, 34);
            this.buttonAssoc.TabIndex = 8;
            this.buttonAssoc.Text = "関連付けする";
            this.buttonAssoc.UseVisualStyleBackColor = true;
            this.buttonAssoc.Click += new System.EventHandler(this.onAgrAssociateClick);
            // 
            // buttonDeAssoc
            // 
            this.buttonDeAssoc.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonDeAssoc.Location = new System.Drawing.Point(161, 18);
            this.buttonDeAssoc.Name = "buttonDeAssoc";
            this.buttonDeAssoc.Size = new System.Drawing.Size(134, 34);
            this.buttonDeAssoc.TabIndex = 9;
            this.buttonDeAssoc.Text = "関連付け解除";
            this.buttonDeAssoc.UseVisualStyleBackColor = true;
            this.buttonDeAssoc.Click += new System.EventHandler(this.onAgrUnAssociateClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonAssoc);
            this.groupBox2.Controls.Add(this.buttonDeAssoc);
            this.groupBox2.Location = new System.Drawing.Point(10, 245);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(322, 67);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Agurippa電子明細の関連付け";
            // 
            // labelCsvVersion
            // 
            this.labelCsvVersion.AutoSize = true;
            this.labelCsvVersion.Location = new System.Drawing.Point(211, 204);
            this.labelCsvVersion.Name = "labelCsvVersion";
            this.labelCsvVersion.Size = new System.Drawing.Size(163, 12);
            this.labelCsvVersion.TabIndex = 11;
            this.labelCsvVersion.Text = "CSV定義バージョン: xxxxxxxxxx";
            // 
            // OptionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 353);
            this.Controls.Add(this.labelCsvVersion);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.checkOfxVer2);
            this.Controls.Add(this.buttonCsvRulesUpdate);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.checkAutoKickOfxFile);
            this.Controls.Add(this.checkManualOfxPath);
            this.Controls.Add(this.checkIgnoreZeroTransaction);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "OptionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "オプション";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkIgnoreZeroTransaction;
        private System.Windows.Forms.CheckBox checkManualOfxPath;
        private System.Windows.Forms.CheckBox checkAutoKickOfxFile;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.RadioButton radioSuica;
        private System.Windows.Forms.RadioButton radioIcoca;
        private System.Windows.Forms.RadioButton radioIruca;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonCsvRulesUpdate;
        private System.Windows.Forms.CheckBox checkOfxVer2;
        private System.Windows.Forms.Button buttonAssoc;
        private System.Windows.Forms.Button buttonDeAssoc;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label labelCsvVersion;
    }
}