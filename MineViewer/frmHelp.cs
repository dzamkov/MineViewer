using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MineViewer
{
    public partial class frmHelp : Form
    {
        public frmHelp()
        {
            InitializeComponent();
        }
        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void panel6_Click(object sender, EventArgs e)
        {
            ProcessStartInfo info = new ProcessStartInfo("http://xiatek.org/?page_id=115");
            Process.Start(info);
        }

        private void frmHelp_Load(object sender, EventArgs e)
        {

        }

        private void btnCredits_Click(object sender, EventArgs e)
        {
            frmCredits c = new frmCredits();
            c.Show();
        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }
        
    }
}
