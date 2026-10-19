namespace AddUserInterface
{
    partial class Form1
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
            this.txtID = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtSpecies = new System.Windows.Forms.TextBox();
            this.txtAge = new System.Windows.Forms.TextBox();
            this.txtRecoveryScore = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.lblID = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblSpecies = new System.Windows.Forms.Label();
            this.lblAge = new System.Windows.Forms.Label();
            this.lblRecoveryScore = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtID
            // 
            this.txtID.Location = new System.Drawing.Point(209, 63);
            this.txtID.Name = "txtID";
            this.txtID.Size = new System.Drawing.Size(100, 22);
            this.txtID.TabIndex = 0;
            this.txtID.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(209, 103);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(100, 22);
            this.txtName.TabIndex = 1;
            this.txtName.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // txtSpecies
            // 
            this.txtSpecies.Location = new System.Drawing.Point(209, 152);
            this.txtSpecies.Name = "txtSpecies";
            this.txtSpecies.Size = new System.Drawing.Size(100, 22);
            this.txtSpecies.TabIndex = 2;
            this.txtSpecies.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // txtAge
            // 
            this.txtAge.Location = new System.Drawing.Point(209, 202);
            this.txtAge.Name = "txtAge";
            this.txtAge.Size = new System.Drawing.Size(100, 22);
            this.txtAge.TabIndex = 3;
            this.txtAge.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // txtRecoveryScore
            // 
            this.txtRecoveryScore.Location = new System.Drawing.Point(209, 246);
            this.txtRecoveryScore.Name = "txtRecoveryScore";
            this.txtRecoveryScore.Size = new System.Drawing.Size(100, 22);
            this.txtRecoveryScore.TabIndex = 4;
            this.txtRecoveryScore.TextChanged += new System.EventHandler(this.textBox5_TextChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(356, 333);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 5;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(23, 333);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(75, 23);
            this.btnBack.TabIndex = 6;
            this.btnBack.Text = "Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.button2_Click);
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(69, 70);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(23, 16);
            this.lblID.TabIndex = 7;
            this.lblID.Text = "ID:";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(69, 107);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(47, 16);
            this.lblName.TabIndex = 8;
            this.lblName.Text = "Name:";
            // 
            // lblSpecies
            // 
            this.lblSpecies.AutoSize = true;
            this.lblSpecies.Location = new System.Drawing.Point(69, 156);
            this.lblSpecies.Name = "lblSpecies";
            this.lblSpecies.Size = new System.Drawing.Size(60, 16);
            this.lblSpecies.TabIndex = 9;
            this.lblSpecies.Text = "Species:";
            // 
            // lblAge
            // 
            this.lblAge.AutoSize = true;
            this.lblAge.Location = new System.Drawing.Point(69, 206);
            this.lblAge.Name = "lblAge";
            this.lblAge.Size = new System.Drawing.Size(35, 16);
            this.lblAge.TabIndex = 10;
            this.lblAge.Text = "Age:";
            // 
            // lblRecoveryScore
            // 
            this.lblRecoveryScore.AutoSize = true;
            this.lblRecoveryScore.Location = new System.Drawing.Point(69, 249);
            this.lblRecoveryScore.Name = "lblRecoveryScore";
            this.lblRecoveryScore.Size = new System.Drawing.Size(106, 16);
            this.lblRecoveryScore.TabIndex = 11;
            this.lblRecoveryScore.Text = "Recovery score:";
            this.lblRecoveryScore.Click += new System.EventHandler(this.lblRecoveryScore_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 390);
            this.Controls.Add(this.lblRecoveryScore);
            this.Controls.Add(this.lblAge);
            this.Controls.Add(this.lblSpecies);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtRecoveryScore);
            this.Controls.Add(this.txtAge);
            this.Controls.Add(this.txtSpecies);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtID);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtID;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtSpecies;
        private System.Windows.Forms.TextBox txtAge;
        private System.Windows.Forms.TextBox txtRecoveryScore;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblSpecies;
        private System.Windows.Forms.Label lblAge;
        private System.Windows.Forms.Label lblRecoveryScore;
    }
}

