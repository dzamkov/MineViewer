using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MineViewer
{
    public partial class frmSchemes : Form
    {
        public ListView Schemes;
        private Action<string> action;
        public frmSchemes(Action<string> onclick)
        {
            InitializeComponent();
            action = onclick;
            Schemes = lvSchemes;
            
            lvSchemes.SelectedIndexChanged += delegate(object sender, EventArgs e)
            {
                if (lvSchemes.SelectedItems.Count == 0) return;
                string str = lvSchemes.SelectedItems[0].Text;
                action.Invoke(str);
            };
        }

        private void lvSchemes_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void frmSchemes_Load(object sender, EventArgs e)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHelp));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lvSchemes_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
}
