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
    public partial class frmKey : Form
    {
        private Scheme _Scheme;
        public frmKey(Scheme scheme)
        {
            InitializeComponent();
            _Scheme = scheme;
        }

        private void frmKey_Load(object sender, EventArgs e)
        {
            this.Controls.Clear();

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHelp));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

            Panel pannelMain = new System.Windows.Forms.Panel();
            pannelMain.Location = new System.Drawing.Point(0, 0);
            pannelMain.Size = new System.Drawing.Size(246 + 12 * 2, 284 + 12 * 2);
            pannelMain.TabIndex = 0;
            //pannelMain.VerticalScroll.Enabled = true;
            pannelMain.AutoScroll = true;
            //pannelMain.VerticalScroll.Visible = true;
            this.Width = pannelMain.Width + 8;
            this.Height = pannelMain.Height + 21 + 10;
            
            
            
            //((System.ComponentModel.ISupportInitialize)(pic)).BeginInit();

            Dictionary<string, byte> Names = MineViewer.Scheme.Names;
            Dictionary<byte, string> RevNames = new Dictionary<byte,string>();

            foreach (KeyValuePair<string, byte> kv in Names)
            {
                byte b = kv.Value;
                string s = kv.Key;

                string ss;
                if(!RevNames.TryGetValue(b, out ss))
                    RevNames.Add(b, s);
            }

            int i = -1;
            foreach (KeyValuePair<byte, Cubia.IMaterial> kv in _Scheme.Materials)
            {
                Cubia.IMaterial mat = kv.Value;
                byte id = kv.Key;
                
                string name;
                RevNames.TryGetValue(id, out name);
                if (name == null) name = "Block ID: " + id.ToString();

                i++;
                

                Panel panel = new
                    Panel();
                panel.SuspendLayout();
                panel.Location = new System.Drawing.Point(3, 0 + (70 + 3) * i);
                panel.Size = new System.Drawing.Size(240 + 12 - 3, 70);
                panel.BackColor = Color.LightGray;
                //panel.BorderStyle = BorderStyle.FixedSingle;

                PictureBox pic = new PictureBox();
                pic.Location = new System.Drawing.Point(3, 2);
                pic.Size = new System.Drawing.Size(64, 64);
                pic.TabIndex = 0;
                pic.TabStop = false;
                pic.Image = mat.MakeBitmap(64);

                Label lblName = new Label();
                lblName.AutoSize = true;
                lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                lblName.Location = new System.Drawing.Point(73, 26);
                lblName.Size = new System.Drawing.Size(51, 20);
                lblName.Text = name;

                panel.Controls.Add(pic);
                panel.Controls.Add(lblName);

                pannelMain.Controls.Add(panel);
            }
            this.Controls.Add(pannelMain);
            
            
        }
    }
}
