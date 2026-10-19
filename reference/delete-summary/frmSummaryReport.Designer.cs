namespace DeleteUserInterface
{
    partial class frmSummaryReport
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
            this.rtbSummary = new System.Windows.Forms.RichTextBox();
            this.btnBackToMain = new System.Windows.Forms.Button();
            this.btnSummary = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbSummary
            // 
            this.rtbSummary.Location = new System.Drawing.Point(174, 36);
            this.rtbSummary.Name = "rtbSummary";
            this.rtbSummary.Size = new System.Drawing.Size(458, 296);
            this.rtbSummary.TabIndex = 0;
            this.rtbSummary.Text = "";
            // 
            // btnBackToMain
            // 
            this.btnBackToMain.Location = new System.Drawing.Point(174, 360);
            this.btnBackToMain.Name = "btnBackToMain";
            this.btnBackToMain.Size = new System.Drawing.Size(131, 43);
            this.btnBackToMain.TabIndex = 1;
            this.btnBackToMain.Text = "Return To Main";
            this.btnBackToMain.UseVisualStyleBackColor = true;
            this.btnBackToMain.Click += new System.EventHandler(this.btnBackToMain_Click);
            // 
            // btnSummary
            // 
            this.btnSummary.Location = new System.Drawing.Point(486, 360);
            this.btnSummary.Name = "btnSummary";
            this.btnSummary.Size = new System.Drawing.Size(146, 43);
            this.btnSummary.TabIndex = 2;
            this.btnSummary.Text = "Generate Summary";
            this.btnSummary.UseVisualStyleBackColor = true;
            this.btnSummary.Click += new System.EventHandler(this.btnSummary_Click);
            // 
            // frmSummaryReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnSummary);
            this.Controls.Add(this.btnBackToMain);
            this.Controls.Add(this.rtbSummary);
            this.Name = "frmSummaryReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Summary Report";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbSummary;
        private System.Windows.Forms.Button btnBackToMain;
        private System.Windows.Forms.Button btnSummary;
    }
}