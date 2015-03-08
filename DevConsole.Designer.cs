namespace Simcraft
{
    partial class DevConsole
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

        #region Windows Form Designer generated fullExpression

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the fullExpression editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.DebuffGrid = new System.Windows.Forms.DataGridView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.BuffGrid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.DebuffGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BuffGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // DebuffGrid
            // 
            this.DebuffGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DebuffGrid.Location = new System.Drawing.Point(21, 12);
            this.DebuffGrid.Name = "DebuffGrid";
            this.DebuffGrid.Size = new System.Drawing.Size(1244, 238);
            this.DebuffGrid.TabIndex = 0;
            this.DebuffGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.DebuffGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DebuffGrid_DataError);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // BuffGrid
            // 
            this.BuffGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BuffGrid.Location = new System.Drawing.Point(21, 256);
            this.BuffGrid.Name = "BuffGrid";
            this.BuffGrid.Size = new System.Drawing.Size(1244, 238);
            this.BuffGrid.TabIndex = 1;
            this.BuffGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick_1);
            this.BuffGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.BuffGrid_DataError);
            // 
            // DevConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1277, 584);
            this.Controls.Add(this.BuffGrid);
            this.Controls.Add(this.DebuffGrid);
            this.Name = "DevConsole";
            this.Text = "DevConsole";
            this.Load += new System.EventHandler(this.DevConsole_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.DevConsole_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.DebuffGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BuffGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView DebuffGrid;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.DataGridView BuffGrid;
    }
}