using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cubia;
using System.IO;
using System.Windows.Forms;
namespace MineViewer
{
    class Bookmarks
    {
        private string _World;
        private string[] _Bookmarks;
        public Action<Vector<double>, Vector<double>> _Action;

        /// <summary>
        /// Creates a new bookmarks object.
        /// </summary>
        public Bookmarks(string world, Action<Vector<double>, Vector<double>> action)
        {
            _World = world;
            _Action = action;
            try
            {
                _Bookmarks = File.ReadAllLines(world + "warps.txt");
            }
            catch
            {
                _Bookmarks = new string[0];
            }
        }

        ~Bookmarks()
        {
            // Do not save here
        }

        /// <summary>
        /// Show the form
        /// </summary>
        public void ShowForm(Vector<double> pos, Vector<double> ang)
        {
            //_Action.Invoke(new Vector<double>(0.0, 0.0, 0.0));
            #region DESIGNER CODE
            ListBox lbBookmarks = new ListBox();
            Button btnGo = new Button();
            Button btnAdd = new Button();
            Button btnRemove = new Button();
            // 
            // lbBookmarks
            // 
            lbBookmarks.FormattingEnabled = true;
            lbBookmarks.Location = new System.Drawing.Point(12, 12);
            lbBookmarks.Size = new System.Drawing.Size(187, 199);
            lbBookmarks.TabIndex = 0;
            // 
            // btnGo
            // 
            btnGo.Location = new System.Drawing.Point(124, 217);
            btnGo.Size = new System.Drawing.Size(75, 23);
            btnGo.TabIndex = 1;
            btnGo.Text = "Go";
            btnGo.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.Location = new System.Drawing.Point(12, 217);
            btnAdd.Size = new System.Drawing.Size(23, 23);
            btnAdd.TabIndex = 3;
            btnAdd.Text = "+";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnRemove
            // 
            btnRemove.Location = new System.Drawing.Point(41, 217);
            btnRemove.Size = new System.Drawing.Size(23, 23);
            btnRemove.TabIndex = 4;
            btnRemove.Text = "-";
            btnRemove.UseVisualStyleBackColor = true;
            // 
            // BookmarksForm
            // 
            Form bookmarksform = new Form();
            bookmarksform.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            bookmarksform.AutoScaleMode = AutoScaleMode.Font;
            bookmarksform.ClientSize = new System.Drawing.Size(211, 249);
            bookmarksform.Controls.Add(btnRemove);
            bookmarksform.Controls.Add(btnAdd);
            bookmarksform.Controls.Add(btnGo);
            bookmarksform.Controls.Add(lbBookmarks);
            bookmarksform.FormBorderStyle = FormBorderStyle.FixedSingle;
            bookmarksform.Text = "Bookmarks";
            bookmarksform.MaximizeBox = false;
            bookmarksform.MinimizeBox = false;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHelp));
            bookmarksform.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            #endregion

            lbBookmarks.Items.Clear();
            foreach (string str in _Bookmarks)
            {
                try
                {
                    string name = str.Split(":".ToCharArray())[0]; // select the first value
                    lbBookmarks.Items.Add(name);
                }
                catch { }
            }

            btnGo.Click += delegate(object sender, EventArgs args)
            {

                object obj = lbBookmarks.SelectedItem;
                if (obj != null)
                {
                    bookmarksform.Close();
                    //bookmarksform.Dispose(); // minimizes ma form!!!!, go get him garbage collecter
                    _Action.Invoke(VectorFromName(obj.ToString()), AngleFromName(obj.ToString()));
                }
            };

            btnRemove.Click += delegate(object sender, EventArgs args)
            {
                object obj = lbBookmarks.SelectedItem;
                if (obj != null)
                {
                    this.Remove(obj.ToString());

                    lbBookmarks.Items.Clear();
                    foreach (string str in _Bookmarks)
                    {
                        try
                        {
                            string name = str.Split(":".ToCharArray())[0]; // select the first value
                            lbBookmarks.Items.Add(name);
                        }
                        catch { }
                    }
                }
            };

            btnAdd.Click += delegate(object sender, EventArgs args)
            {
                string input = "";
                if (IB.InputBox("Enter a name", "Please enter a name for this bookmark", ref input) == DialogResult.OK && input.Length > 0)
                {
                    Vector<double> vec2 = new Vector<double>(pos.X, pos.Z, pos.Y);
                    this.Add(input, vec2, ang);

                    lbBookmarks.Items.Clear();
                    foreach (string str in _Bookmarks)
                    {
                        try
                        {
                            string name = str.Split(":".ToCharArray())[0]; // select the first value
                            lbBookmarks.Items.Add(name);
                        }
                        catch { }
                    }
                }
            };

            bookmarksform.ShowDialog();
        }

        private bool Exists(string name)
        {
            foreach (string str in _Bookmarks)
                if (str.StartsWith(name + ":"))
                    return true;
            return false;
        }

        public void Save()
        {
            if (!SMPInterface.IsSMP)
                File.WriteAllLines(_World + "warps.txt", _Bookmarks);
        }

        private Vector<double> VectorFromName(string name)
        {
            foreach (string str in _Bookmarks)
            {
                if (str.StartsWith(name + ":"))
                {
                    try
                    {
                        string[] split = str.Split(":".ToCharArray());
                        double x = double.Parse(split[1]); // Yeah, Y is up, in MineViewer, Z is up
                        double y = double.Parse(split[2]);
                        double z = double.Parse(split[3]);
                        return new Vector<double>(z, x, y);
                    }catch{}
                    break;
                }
            }
            return new Vector<double>(0.0, 0.0, 0.0);
        }

        private Vector<double> AngleFromName(string name)// this shit dont work, why?!!?!?
        {
            foreach (string str in _Bookmarks)
            {
                if (str.StartsWith(name + ":"))
                {
                    try
                    {
                        string[] split = str.Split(":".ToCharArray());
                        double x = double.Parse(split[4]) % 360.0;
                        double z = double.Parse(split[5]) % 360.0;

                        x = (x * Math.PI) / 180.0;
                        z = (z * Math.PI) / 180.0;

                        x -= Math.PI / 2.0;
                        z -= Math.PI / 2.0;

                        return new Vector<double>(x, 0.0, z);
                    }
                    catch { }
                    break;
                }
            }
            return new Vector<double>(0.0, 0.0, 0.0);
        }

        /// <summary>
        /// Add a new Bookmark
        /// </summary>
        public void Add(string name, Vector<double> pos, Vector<double> ang)
        {
            name = name.Replace(":", "");
            if(this.Exists(name)) // Is there a better way to do this?
                return;
            string[] new_Bookmarks = new string[_Bookmarks.Length + 1];

            for (int i = 0; i < _Bookmarks.Length; i++)
                new_Bookmarks[i] = _Bookmarks[i];

            ang.X += Math.PI / 2.0;
            ang.Z += Math.PI / 2.0;

            double yaw = ((ang.X * 180) / Math.PI );
            double pitch = ((ang.Z * 180) / Math.PI );

            string serialized = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:", name, pos.Z, pos.Y, pos.X, yaw, pitch);
            new_Bookmarks[new_Bookmarks.Length - 1] = serialized;
            _Bookmarks = new_Bookmarks;
        }
        /// <summary>
        /// Remove an existing bookmark
        /// </summary>
        public void Remove(string name)
        {
            if (!this.Exists(name)) // Yeah, inefficent, I dont care, doesen't need to be.
                return;
            int i = 0;
            int tcount = 0;
            foreach (string str in _Bookmarks)
            {
                if (str.StartsWith(name + ":"))
                {
                    _Bookmarks[i] = null;
                    tcount++;
                }
                i++;
            }
            string[] new_Bookmarks = new string[_Bookmarks.Length - tcount];
            int t = 0;
            int real_t = 0;
            foreach (string str in _Bookmarks)
            {
                if (str != null)
                {
                    new_Bookmarks[t] = _Bookmarks[real_t];
                    t++;
                }
                real_t++;
            }
            _Bookmarks = new_Bookmarks;
        }
    }
}
