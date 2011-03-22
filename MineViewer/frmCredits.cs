using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MineViewer
{
    public partial class frmCredits : Form
    {
        public frmCredits()
        {
            InitializeComponent();
        }

        private void frmCredits_Load(object sender, EventArgs e)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHelp));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
