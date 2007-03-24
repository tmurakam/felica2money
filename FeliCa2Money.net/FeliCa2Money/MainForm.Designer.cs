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
            this.buttonQuit.Location = new System.Drawing.Point(153, 249);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(75, 23);
            this.buttonQuit.TabIndex = 2;
            this.buttonQuit.Text = "終了";
            this.buttonQuit.UseVisualStyleBackColor = true;
            this.buttonQuit.Click += new System.EventHandler(this.buttonQuit_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 329);
            this.Controls.Add(this.buttonQuit);
            this.Controls.Add(this.buttonSuica);
            this.Controls.Add(this.buttonEdy);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "FeliCa2Money";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonEdy;
        private System.Windows.Forms.Button buttonSuica;
        private System.Windows.Forms.Button buttonQuit;
    }
}

