namespace MineViewer
{
    partial class frmSMPChat
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSMPChat));
            this.btnSend = new System.Windows.Forms.Button();
            this.tbMsg = new System.Windows.Forms.TextBox();
            this.btnMenu = new System.Windows.Forms.Button();
            this.tmrSpin = new System.Windows.Forms.Timer(this.components);
            this.rtbHistory = new System.Windows.Forms.RichTextBox();
            this.chatpanel = new System.Windows.Forms.Panel();
            this.ContextStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.forceMapDownloadMovePlayerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chatpanel.SuspendLayout();
            this.ContextStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(408, 286);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(65, 28);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // tbMsg
            // 
            this.tbMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMsg.Location = new System.Drawing.Point(55, 287);
            this.tbMsg.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbMsg.MaxLength = 100;
            this.tbMsg.Name = "tbMsg";
            this.tbMsg.Size = new System.Drawing.Size(344, 22);
            this.tbMsg.TabIndex = 1;
            // 
            // btnMenu
            // 
            this.btnMenu.Location = new System.Drawing.Point(1, 286);
            this.btnMenu.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnMenu.Name = "btnMenu";
            this.btnMenu.Size = new System.Drawing.Size(45, 28);
            this.btnMenu.TabIndex = 3;
            this.btnMenu.Text = "^";
            this.btnMenu.UseVisualStyleBackColor = true;
            this.btnMenu.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // tmrSpin
            // 
            this.tmrSpin.Interval = 750;
            this.tmrSpin.Tick += new System.EventHandler(this.tmrSpin_Tick);
            // 
            // rtbHistory
            // 
            this.rtbHistory.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.rtbHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbHistory.Location = new System.Drawing.Point(0, 0);
            this.rtbHistory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rtbHistory.Name = "rtbHistory";
            this.rtbHistory.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbHistory.Size = new System.Drawing.Size(475, 282);
            this.rtbHistory.TabIndex = 4;
            this.rtbHistory.Text = "";
            this.rtbHistory.TextChanged += new System.EventHandler(this.rtbHistory_TextChanged);
            // 
            // chatpanel
            // 
            this.chatpanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chatpanel.Controls.Add(this.rtbHistory);
            this.chatpanel.Location = new System.Drawing.Point(1, 1);
            this.chatpanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chatpanel.Name = "chatpanel";
            this.chatpanel.Size = new System.Drawing.Size(475, 282);
            this.chatpanel.TabIndex = 5;
            // 
            // ContextStrip
            // 
            this.ContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.forceMapDownloadMovePlayerToolStripMenuItem,
            this.disconnectToolStripMenuItem});
            this.ContextStrip.Name = "ContextStrip";
            this.ContextStrip.ShowCheckMargin = true;
            this.ContextStrip.ShowImageMargin = false;
            this.ContextStrip.Size = new System.Drawing.Size(398, 68);
            this.ContextStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ContextStrip_Opening);
            // 
            // forceMapDownloadMovePlayerToolStripMenuItem
            // 
            this.forceMapDownloadMovePlayerToolStripMenuItem.CheckOnClick = true;
            this.forceMapDownloadMovePlayerToolStripMenuItem.Name = "forceMapDownloadMovePlayerToolStripMenuItem";
            this.forceMapDownloadMovePlayerToolStripMenuItem.Size = new System.Drawing.Size(397, 32);
            this.forceMapDownloadMovePlayerToolStripMenuItem.Text = "Force Map Download (Move Player)";
            this.forceMapDownloadMovePlayerToolStripMenuItem.Click += new System.EventHandler(this.forceMapDownloadMovePlayerToolStripMenuItem_Click);
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(397, 32);
            this.disconnectToolStripMenuItem.Text = "Disconnect";
            this.disconnectToolStripMenuItem.Click += new System.EventHandler(this.disconnectToolStripMenuItem_Click);
            // 
            // frmSMPChat
            // 
            this.AcceptButton = this.btnSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(475, 315);
            this.Controls.Add(this.chatpanel);
            this.Controls.Add(this.btnMenu);
            this.Controls.Add(this.tbMsg);
            this.Controls.Add(this.btnSend);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmSMPChat";
            this.Text = "MineViewer - SMP Chat";
            this.Load += new System.EventHandler(this.frmSMPChat_Load);
            this.chatpanel.ResumeLayout(false);
            this.ContextStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.Button btnMenu;
        private System.Windows.Forms.Timer tmrSpin;
        private System.Windows.Forms.RichTextBox rtbHistory;
        private System.Windows.Forms.Panel chatpanel;
        private System.Windows.Forms.ContextMenuStrip ContextStrip;
        private System.Windows.Forms.ToolStripMenuItem forceMapDownloadMovePlayerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;
    }
}