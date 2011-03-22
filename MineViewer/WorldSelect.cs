using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace MineViewer
{
    public partial class WorldSelect : Form
    {
        public WorldSelect()
        {
            InitializeComponent();

            this._PathToMinecraftFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + ".minecraft";

            try
            {
                if (!Directory.Exists(this._PathToMinecraftFolder))
                {
                    string pathtxt = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "path.txt";
                    bool pathIsInvalid = false;
                    if (File.Exists(pathtxt))
                    {
                        StreamReader writer = new StreamReader(pathtxt);
                        string path = writer.ReadLine();
                        writer.Close();

                        pathIsInvalid = !Directory.Exists(path);
                    }
                    if (!File.Exists(pathtxt) || pathIsInvalid)
                    {
                        DialogResult res = MessageBox.Show("\".minecraft\" Folder not found\nPlease browse to your \".minecraft\" folder and open \"options.txt\"", "\".minecraft\" not found", MessageBoxButtons.OKCancel);
                        if (res == DialogResult.OK)
                        {
                            OpenFileDialog ofd = new OpenFileDialog();
                            ofd.FileName = "options.txt";
                            ofd.Filter = "Minecraft options|options.txt";
                            
                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                StreamWriter writer = new StreamWriter(pathtxt);
                                this._PathToMinecraftFolder = Path.GetDirectoryName(ofd.FileName);
                                writer.Write(this._PathToMinecraftFolder);
                                writer.Close(); // Data was not being flushed on mono without this line.
                            }
                        }
                        else
                            if (File.Exists(pathtxt))
                                File.Delete(pathtxt);
                    }
                    else
                    {
                        this._PathToMinecraftFolder = File.ReadAllText(pathtxt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
                this._PathToMinecraftFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + ".minecraft";
            }

            this.BrowseButton.Click += delegate
            {
                this._Browse();
            };
            this._World = null;
            this.cbNether.Click += delegate
            {
                this._Nether = cbNether.Checked;
            };

            DirectoryInfo info = new DirectoryInfo(this._PathToMinecraftFolder + Path.DirectorySeparatorChar + "saves");
            bool DoneOne = false;
            foreach (DirectoryInfo worldinfo in info.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                DoneOne = true;
                string name = worldinfo.Name;
                Button btn = new Button();
                btn.Size = new Size(200, 25);
                btn.Text = name;
                btn.UseVisualStyleBackColor = true;
                btn.Click += delegate
                {
                    this.Select(worldinfo.FullName);
                };
                flowLayoutPanel.Controls.Add(btn);
            }
            if (!DoneOne)
            {
                Button btn = new Button();
                btn.Size = new Size(200, 23);
                btn.Text = "No Worlds Found";
                btn.Enabled = false;
                flowLayoutPanel.Controls.Add(btn);
            }
        }

        private void _Browse()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Minecraft Map Data File|*.dat|All files|*";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string file = ofd.FileName;
                this.Select(Directory.GetParent(file).FullName);
            }
        }

        /// <summary>
        /// Selects the specified world.
        /// </summary>
        public void Select(string World)
        {
            this._World = World;
            this.Close();
        }

        /// <summary>
        /// Gets the currently selected world.
        /// </summary>
        public string World
        {
            get
            {
                return this._World;
            }
        }

        /// <summary>
        /// Get if nether is selected
        /// </summary>
        public bool Nether
        {
            get
            {
                return this._Nether;
            }
        }

        private bool _Nether = false;
        private string _World;

        private void btnServerTest_Click(object sender, EventArgs e)
        {
            frmSMPDetails _SMPDetails = new frmSMPDetails();
            _SMPDetails.ShowDialog();
            this.Select("");
        }

        private void WorldSelect_Load(object sender, EventArgs e)
        {
            

        }

        private string _PathToMinecraftFolder;
    }
}
