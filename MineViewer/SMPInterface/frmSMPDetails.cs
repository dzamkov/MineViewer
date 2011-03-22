using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace MineViewer
{
    public partial class frmSMPDetails : Form
    {

        private static int UsernameKey = 0;

        public frmSMPDetails()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                errorProvider.SetError(ctrl, "");
            
            bool berror = false;
            if (this.txtUsername.Text == "")
            {
                errorProvider.SetError(txtUsername, "Username not specified");
                berror = true;
            }
            if (this.txtPassword.Text == "")
            {
                errorProvider.SetError(txtPassword, "Password not specified");
                berror = true;
            }
            if (this.txtServerIP.Text == "")
            {
                errorProvider.SetError(txtServerIP, "Server not specified");
                berror = true;
            }
            if (berror)
                return;

            btnConnect.Text = "Connecting...";
            btnConnect.Enabled = false;
            
            if (
                SMPInterface.Connect(txtServerIP.Text, txtUsername.Text, txtPassword.Text, txtSrvPassword.Text)
            )
                this.Close();
            else
            {
                btnConnect.Text = "Connect";
                btnConnect.Enabled = true;
                errorProvider.SetError(btnConnect, SMPInterface.LastError);
            }
        }

        private void frmSMPDetails_Load(object sender, EventArgs e)
        {
            
        }
    }
}
