namespace MineViewer
{
    partial class WorldSelect
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorldSelect));
            this.BrowseButton = new System.Windows.Forms.Button();
            this.cbNether = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnServerTest = new System.Windows.Forms.Button();
            this.panel5 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(267, 14);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BrowseButton.TabIndex = 5;
            this.BrowseButton.Text = "&Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            // 
            // cbNether
            // 
            this.cbNether.AutoSize = true;
            this.cbNether.Location = new System.Drawing.Point(12, 17);
            this.cbNether.Name = "cbNether";
            this.cbNether.Size = new System.Drawing.Size(88, 17);
            this.cbNether.TabIndex = 6;
            this.cbNether.Text = "&Nether Mode";
            this.cbNether.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.panel5);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 172);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(354, 50);
            this.panel1.TabIndex = 7;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnServerTest);
            this.panel2.Controls.Add(this.BrowseButton);
            this.panel2.Controls.Add(this.cbNether);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(354, 49);
            this.panel2.TabIndex = 2;
            // 
            // btnServerTest
            // 
            this.btnServerTest.Location = new System.Drawing.Point(186, 14);
            this.btnServerTest.Name = "btnServerTest";
            this.btnServerTest.Size = new System.Drawing.Size(75, 23);
            this.btnServerTest.TabIndex = 7;
            this.btnServerTest.Text = "&From SMP";
            this.btnServerTest.UseVisualStyleBackColor = true;
            this.btnServerTest.Click += new System.EventHandler(this.btnServerTest_Click);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(354, 1);
            this.panel5.TabIndex = 1;
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.AutoScroll = true;
            this.flowLayoutPanel.BackgroundImage = global::Cubia.Properties.Resources.help_bg;
            this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Padding = new System.Windows.Forms.Padding(73, 10, 0, 10);
            this.flowLayoutPanel.Size = new System.Drawing.Size(354, 222);
            this.flowLayoutPanel.TabIndex = 8;
            // 
            // WorldSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(354, 222);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.flowLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "WorldSelect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MineViewer World Selection";
            this.Load += new System.EventHandler(this.WorldSelect_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.CheckBox cbNether;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnServerTest;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
    }
}