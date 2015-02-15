namespace Simcraft
{
    partial class ConfigWindow
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.exKey = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.BKey = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.AoeKey = new System.Windows.Forms.ComboBox();
            this.cdKey = new System.Windows.Forms.ComboBox();
            this.cdMod = new System.Windows.Forms.ComboBox();
            this.exMod = new System.Windows.Forms.ComboBox();
            this.bMod = new System.Windows.Forms.ComboBox();
            this.aoeMod = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 137);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(93, 137);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Cooldowns";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Execution";
            // 
            // exKey
            // 
            this.exKey.FormattingEnabled = true;
            this.exKey.Location = new System.Drawing.Point(72, 41);
            this.exKey.Name = "exKey";
            this.exKey.Size = new System.Drawing.Size(42, 21);
            this.exKey.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Burst";
            // 
            // BKey
            // 
            this.BKey.Enabled = false;
            this.BKey.FormattingEnabled = true;
            this.BKey.Location = new System.Drawing.Point(72, 69);
            this.BKey.Name = "BKey";
            this.BKey.Size = new System.Drawing.Size(42, 21);
            this.BKey.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 101);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "AoE";
            // 
            // AoeKey
            // 
            this.AoeKey.FormattingEnabled = true;
            this.AoeKey.Location = new System.Drawing.Point(72, 97);
            this.AoeKey.Name = "AoeKey";
            this.AoeKey.Size = new System.Drawing.Size(42, 21);
            this.AoeKey.TabIndex = 20;
            // 
            // cdKey
            // 
            this.cdKey.FormattingEnabled = true;
            this.cdKey.Location = new System.Drawing.Point(72, 12);
            this.cdKey.Name = "cdKey";
            this.cdKey.Size = new System.Drawing.Size(42, 21);
            this.cdKey.TabIndex = 25;
            // 
            // cdMod
            // 
            this.cdMod.FormattingEnabled = true;
            this.cdMod.Location = new System.Drawing.Point(126, 12);
            this.cdMod.Name = "cdMod";
            this.cdMod.Size = new System.Drawing.Size(92, 21);
            this.cdMod.TabIndex = 26;
            // 
            // exMod
            // 
            this.exMod.FormattingEnabled = true;
            this.exMod.Location = new System.Drawing.Point(126, 41);
            this.exMod.Name = "exMod";
            this.exMod.Size = new System.Drawing.Size(92, 21);
            this.exMod.TabIndex = 27;
            // 
            // bMod
            // 
            this.bMod.Enabled = false;
            this.bMod.FormattingEnabled = true;
            this.bMod.Location = new System.Drawing.Point(126, 68);
            this.bMod.Name = "bMod";
            this.bMod.Size = new System.Drawing.Size(92, 21);
            this.bMod.TabIndex = 28;
            // 
            // aoeMod
            // 
            this.aoeMod.FormattingEnabled = true;
            this.aoeMod.Location = new System.Drawing.Point(126, 97);
            this.aoeMod.Name = "aoeMod";
            this.aoeMod.Size = new System.Drawing.Size(92, 21);
            this.aoeMod.TabIndex = 29;
            // 
            // ConfigWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(230, 166);
            this.Controls.Add(this.aoeMod);
            this.Controls.Add(this.bMod);
            this.Controls.Add(this.exMod);
            this.Controls.Add(this.cdMod);
            this.Controls.Add(this.cdKey);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.AoeKey);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.exKey);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "ConfigWindow";
            this.Text = "ConfigWindow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox exKey;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox BKey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox AoeKey;
        private System.Windows.Forms.ComboBox cdKey;
        private System.Windows.Forms.ComboBox cdMod;
        private System.Windows.Forms.ComboBox exMod;
        private System.Windows.Forms.ComboBox bMod;
        private System.Windows.Forms.ComboBox aoeMod;
    }
}