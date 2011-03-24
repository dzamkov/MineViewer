#define RELEASEMODE

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace MineViewer
{
    /// <summary>
    /// Main class.
    /// </summary>
    public static class Program
    {
        
        /// <summary>
        /// Program main entry point.
        /// </summary>
        [STAThread]
        public static void Main(string[] Args)
        {
            Application.EnableVisualStyles();
            // Load schemes from lua files
            Dictionary<string, Scheme> schemes = null;
            try
            {
                string path = Application.StartupPath + Path.DirectorySeparatorChar + "scheme.lua";
                
                schemes = Scheme.Load(File.OpenRead(path));
                if (!schemes.ContainsKey("Default"))
                {
                    MessageBox.Show("Scheme.lua does not define a scheme named \"Default\"", "Scheme load error");
                    schemes = null;
                }
            }
            catch (LuaException le)
            {
                string error = "";
                error += le.Run ? "Runtime lua " : "Lua syntax ";
                error += "error in scheme.lua: ";
                error += le.LuaDesc;
                MessageBox.Show(error, "Scheme load error");
            }
            catch (FileNotFoundException fnfe)
            {
                MessageBox.Show("Scheme.lua cannot be found", "Scheme load error");
            }

            if (schemes != null)
            {
                WorldSelect ws = new WorldSelect();
                ws.Show();
                while (ws.Visible)
                {
                    Application.DoEvents();
                }

                string world = ws.World;

                if (world != null)
                {
#if RELEASEMODE
                    try
                    {
#endif
                        Window win = new Window(world, ws.Nether, schemes);
                        win.TargetUpdateFrequency = 60.0;
                        win.VSync = OpenTK.VSyncMode.On;
                        win.Run(60.0, 60.0);
#if RELEASEMODE
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error has occurred and the error contents has been dumped to \"error.txt\"\nPlease report this bug on bugs.xiatek.org (please make sure it has not been posted yet)", "Unhandeled Error");
                        File.AppendAllText("error.txt", "\nERROR:\n" + ex.Message + "\nSTACKTRACE:\n" + ex.StackTrace);
                    }
#endif
                }
            }
        }
    }
}