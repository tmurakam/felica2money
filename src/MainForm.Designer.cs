namespace FeliCa2Money
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.buttonEdy = new System.Windows.Forms.Button();
            this.buttonSuica = new System.Windows.Forms.Button();
            this.buttonQuit = new System.Windows.Forms.Button();
            this.buttonOption = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonManual = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonNanaco = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.buttonCSV = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.buttonWaon = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonEdy
            // 
            this.buttonEdy.Image = global::FeliCa2Money.Properties.Resources.Edy;
            this.buttonEdy.Location = new System.Drawing.Point(12, 12);
            this.buttonEdy.Name = "buttonEdy";
            this.buttonEdy.Size = new System.Drawing.Size(58, 58);
            this.buttonEdy.TabIndex = 0;
            this.buttonEdy.UseVisualStyleBackColor = true;
            this.buttonEdy.Click += new System.EventHandler(this.buttonEdy_Click);
            // 
            // buttonSuica
            // 
            this.buttonSuica.Image = global::FeliCa2Money.Properties.Resources.Suica;
            this.buttonSuica.Location = new System.Drawing.Point(12, 76);
            this.buttonSuica.Name = "buttonSuica";
            this.buttonSuica.Size = new System.Drawing.Size(58, 58);
            this.buttonSuica.TabIndex = 1;
            this.buttonSuica.UseVisualStyleBackColor = true;
            this.buttonSuica.Click += new System.EventHandler(this.buttonSuica_Click);
            // 
            // buttonQuit
            // 
            this.buttonQuit.Location = new System.Drawing.Point(133, 538);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(94, 30);
            this.buttonQuit.TabIndex = 7;
            this.buttonQuit.Text = "終了";
            this.buttonQuit.UseVisualStyleBackColor = true;
            this.buttonQuit.Click += new System.EventHandler(this.buttonQuit_Click);
            // 
            // buttonOption
            // 
            this.buttonOption.Location = new System.Drawing.Point(12, 396);
            this.buttonOption.Name = "buttonOption";
            this.buttonOption.Size = new System.Drawing.Size(58, 58);
            this.buttonOption.TabIndex = 5;
            this.buttonOption.Text = "設定";
            this.buttonOption.UseVisualStyleBackColor = true;
            this.buttonOption.Click += new System.EventHandler(this.buttonOption_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(76, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(266, 29);
            this.label1.TabIndex = 8;
            this.label1.Text = "PaSoRiを使って Edy の利用履歴を Microsoft Money に取り込みます。";
            this.label1.UseWaitCursor = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(76, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(266, 29);
            this.label2.TabIndex = 9;
            this.label2.Text = "PaSoRiを使って Suica/ICOCAなど交通系電子マネーの利用履歴を Microsoft Money に取り込みます。";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(76, 419);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 12);
            this.label5.TabIndex = 13;
            this.label5.Text = "各種設定を行います。";
            // 
            // buttonManual
            // 
            this.buttonManual.Location = new System.Drawing.Point(12, 460);
            this.buttonManual.Name = "buttonManual";
            this.buttonManual.Size = new System.Drawing.Size(58, 58);
            this.buttonManual.TabIndex = 6;
            this.buttonManual.Text = "マニュアル";
            this.buttonManual.UseVisualStyleBackColor = true;
            this.buttonManual.Click += new System.EventHandler(this.buttonManual_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(76, 483);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(193, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "FeliCa2Money のマニュアルを開きます。";
            // 
            // buttonNanaco
            // 
            this.buttonNanaco.Image = global::FeliCa2Money.Properties.Resources.Nanaco;
            this.buttonNanaco.Location = new System.Drawing.Point(12, 140);
            this.buttonNanaco.Name = "buttonNanaco";
            this.buttonNanaco.Size = new System.Drawing.Size(58, 58);
            this.buttonNanaco.TabIndex = 2;
            this.buttonNanaco.UseVisualStyleBackColor = true;
            this.buttonNanaco.Click += new System.EventHandler(this.buttonNanaco_Click);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(76, 154);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(266, 29);
            this.label7.TabIndex = 10;
            this.label7.Text = "PaSoRiを使って nanaco の利用履歴を Microsoft Money に取り込みます。";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "OFX File(*.ofx)|*.ofx";
            // 
            // buttonCSV
            // 
            this.buttonCSV.Image = global::FeliCa2Money.Properties.Resources.CSV;
            this.buttonCSV.Location = new System.Drawing.Point(12, 268);
            this.buttonCSV.Name = "buttonCSV";
            this.buttonCSV.Size = new System.Drawing.Size(58, 58);
            this.buttonCSV.TabIndex = 4;
            this.buttonCSV.UseVisualStyleBackColor = true;
            this.buttonCSV.Click += new System.EventHandler(this.buttonCSV_Click);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(76, 284);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(266, 28);
            this.label9.TabIndex = 12;
            this.label9.Text = "CSVファイルを読み込んで Microsoft Money に取り込みます。";
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "csv";
            this.openFileDialog.Filter = "CSVファイル|*.csv|すべてのファイル|*.*";
            // 
            // buttonWaon
            // 
            this.buttonWaon.Image = ((System.Drawing.Image)(resources.GetObject("buttonWaon.Image")));
            this.buttonWaon.Location = new System.Drawing.Point(12, 204);
            this.buttonWaon.Name = "buttonWaon";
            this.buttonWaon.Size = new System.Drawing.Size(58, 58);
            this.buttonWaon.TabIndex = 3;
            this.buttonWaon.UseVisualStyleBackColor = true;
            this.buttonWaon.Click += new System.EventHandler(this.buttonWaon_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(76, 217);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(266, 29);
            this.label3.TabIndex = 11;
            this.label3.Text = "PaSoRiを使って WAON の利用履歴を Microsoft Money に取り込みます。";
            // 
            // button1
            // 
            this.button1.Image = global::FeliCa2Money.Properties.Resources.Agurippa;
            this.button1.Location = new System.Drawing.Point(12, 332);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(58, 58);
            this.button1.TabIndex = 15;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonAGR_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(76, 346);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(266, 28);
            this.label4.TabIndex = 16;
            this.label4.Text = "Agurippa電子明細ファイルを読み込んで Microsoft Money に取り込みます。";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 580);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonWaon);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.buttonCSV);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.buttonNanaco);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.buttonManual);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOption);
            this.Controls.Add(this.buttonQuit);
            this.Controls.Add(this.buttonSuica);
            this.Controls.Add(this.buttonEdy);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FeliCa2Money ver 3.0 beta 3";
            this.Load += new System.EventHandler(this.onLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonEdy;
        private System.Windows.Forms.Button buttonSuica;
        private System.Windows.Forms.Button buttonQuit;
        private System.Windows.Forms.Button buttonOption;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonManual;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonNanaco;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button buttonCSV;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button buttonWaon;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
    }
}

