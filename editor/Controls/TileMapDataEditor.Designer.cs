namespace sth1edwv.Controls
{
    sealed partial class TileMapDataEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.udX = new System.Windows.Forms.NumericUpDown();
            this.udY = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.udX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udY)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "X";
            // 
            // udX
            // 
            this.udX.Location = new System.Drawing.Point(25, 3);
            this.udX.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.udX.Name = "udX";
            this.udX.Size = new System.Drawing.Size(54, 23);
            this.udX.TabIndex = 1;
            this.udX.ValueChanged += new System.EventHandler(this.udX_ValueChanged);
            // 
            // udY
            // 
            this.udY.Location = new System.Drawing.Point(25, 32);
            this.udY.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.udY.Name = "udY";
            this.udY.Size = new System.Drawing.Size(54, 23);
            this.udY.TabIndex = 3;
            this.udY.ValueChanged += new System.EventHandler(this.udY_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Y";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonRemove);
            this.panel1.Controls.Add(this.buttonAdd);
            this.panel1.Controls.Add(this.udX);
            this.panel1.Controls.Add(this.udY);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(166, 61);
            this.panel1.TabIndex = 4;
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(85, 32);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 5;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(85, 5);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 4;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(166, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(458, 61);
            this.panel2.TabIndex = 5;
            this.panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.panel2_Paint);
            this.panel2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseClick);
            // 
            // TileMapDataEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "TileMapDataEditor";
            this.Size = new System.Drawing.Size(624, 61);
            ((System.ComponentModel.ISupportInitialize)(this.udX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udY)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown udX;
        private System.Windows.Forms.NumericUpDown udY;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Panel panel2;
    }
}
