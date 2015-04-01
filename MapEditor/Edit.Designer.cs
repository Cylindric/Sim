namespace MapEditor
{
    partial class Edit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Edit));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.FileOpenButton = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ToolDisplay = new OpenTK.GLControl();
            this.MapDisplay = new OpenTK.GLControl();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileOpenButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1205, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // FileOpenButton
            // 
            this.FileOpenButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FileOpenButton.Image = global::MapEditor.Properties.Resources.map_edit;
            this.FileOpenButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FileOpenButton.Name = "FileOpenButton";
            this.FileOpenButton.Size = new System.Drawing.Size(23, 22);
            this.FileOpenButton.Text = "toolStripButton1";
            this.FileOpenButton.Click += new System.EventHandler(this.FileOpenButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ToolDisplay);
            this.splitContainer1.Panel1MinSize = 40;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.MapDisplay);
            this.splitContainer1.Size = new System.Drawing.Size(1205, 517);
            this.splitContainer1.SplitterDistance = 192;
            this.splitContainer1.TabIndex = 2;
            // 
            // ToolDisplay
            // 
            this.ToolDisplay.BackColor = System.Drawing.Color.Black;
            this.ToolDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ToolDisplay.Location = new System.Drawing.Point(0, 0);
            this.ToolDisplay.Name = "ToolDisplay";
            this.ToolDisplay.Size = new System.Drawing.Size(192, 517);
            this.ToolDisplay.TabIndex = 0;
            this.ToolDisplay.VSync = false;
            this.ToolDisplay.Load += new System.EventHandler(this.ToolDisplay_Load);
            this.ToolDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.ToolDisplay_Paint);
            this.ToolDisplay.Resize += new System.EventHandler(this.ToolDisplay_Resize);
            // 
            // MapDisplay
            // 
            this.MapDisplay.BackColor = System.Drawing.Color.Black;
            this.MapDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MapDisplay.Location = new System.Drawing.Point(0, 0);
            this.MapDisplay.Name = "MapDisplay";
            this.MapDisplay.Size = new System.Drawing.Size(1009, 517);
            this.MapDisplay.TabIndex = 1;
            this.MapDisplay.VSync = false;
            this.MapDisplay.Load += new System.EventHandler(this.mapDisplay_Load);
            this.MapDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.mapDisplay_Paint);
            this.MapDisplay.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mapDisplay_KeyDown);
            this.MapDisplay.Resize += new System.EventHandler(this.mapDisplay_Resize);
            // 
            // Edit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1205, 542);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Edit";
            this.Text = "Map Editor";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton FileOpenButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private OpenTK.GLControl MapDisplay;
        private OpenTK.GLControl ToolDisplay;
    }
}

