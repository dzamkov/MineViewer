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
    public partial class frmSMPChat : Form
    {
        private Action<string> SendAction;
        public frmSMPChat(Action<string> sendAction)
        {
            SendAction = sendAction;
            InitializeComponent();
            ChatCols.Add('0', Color.White);
            ChatCols.Add('1', Color.DarkBlue);
            ChatCols.Add('2', Color.DarkGreen);
            ChatCols.Add('3', Color.DarkCyan);
            ChatCols.Add('4', Color.DarkRed);
            ChatCols.Add('5', Color.DarkMagenta);
            ChatCols.Add('6', Color.DarkOrange);
            ChatCols.Add('7', Color.Gray);
            ChatCols.Add('8', Color.DarkGray);
            ChatCols.Add('9', Color.Blue);
            ChatCols.Add('a', Color.Green);
            ChatCols.Add('b', Color.Cyan);
            ChatCols.Add('c', Color.Red);
            ChatCols.Add('d', Color.Magenta);
            ChatCols.Add('e', Color.Yellow);
            ChatCols.Add('f', Color.White); // white, but pointless
        }
        Dictionary<char, Color> ChatCols = new Dictionary<char, Color>();
        Dictionary<Cubia.Point<int>, Color> ParsedChatCols = new Dictionary<Cubia.Point<int>, Color>();
        string sText = "";
        public void Update(string msg)
        {
            sText += "\n";
            string[] Msg = msg.Split("§".ToCharArray());
            bool startswithparchar = msg.StartsWith("§");
            foreach (string newcol in Msg)
            {
                if (startswithparchar == false)
                {

                    ParsedChatCols.Add(new Cubia.Point<int>(sText.Length, newcol.Length),
                    Color.White);

                    sText += newcol;
                    startswithparchar = true;
                    continue;
                }
                if (newcol.Length == 0)
                    continue;
                

                char key = newcol[0];
                string Newcol = newcol.Substring(1, newcol.Length - 1);

                Color c;
                if (!ChatCols.TryGetValue(key, out c))
                    c = Color.White;

                ParsedChatCols.Add(new Cubia.Point<int>(sText.Length, Newcol.Length),
                    c);

                sText += Newcol;
            }
            
            /*
            string[] lines = Text.Split("\n".ToCharArray());
            rtbHistory.Lines = lines;*/
            rtbHistory.Text = sText;
            foreach (KeyValuePair<Cubia.Point<int>, Color> kv in ParsedChatCols)
            {
                rtbHistory.Select(kv.Key.X, kv.Key.Y);
                rtbHistory.SelectionColor = kv.Value;
            }
            rtbHistory.SelectionStart = sText.Length;
            rtbHistory.ScrollToCaret();
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        Timer tmr;
        private void frmSMPChat_Load(object sender, EventArgs e)
        {
            
            tmr = new Timer();
            tmr.Interval = 50;
            DateTime last = DateTime.Now;
            tmr.Tick += delegate(object s, EventArgs a)
            {
                double msago = (DateTime.Now - last).TotalMilliseconds;
                if (ToSend.Count > 0 && msago > 1000)
                {
                    string first = ToSend.First.Value;
                    ToSend.RemoveFirst();
                    SendAction.Invoke(first);
                    last = DateTime.Now;
                }
                else if(ToSend.Count == 0)
                {
                    tmr.Enabled = false;
                }
                
            };
        }

        LinkedList<string> ToSend = new LinkedList<string>();
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (tbMsg.Text.Length > 0)
            {
                ToSend.AddLast(tbMsg.Text);
                tmr.Enabled = true;
                //SendAction.Invoke(tbMsg.Text);
            }
            tbMsg.Text = "";
        }


        private void tbHistory_TextChanged(object sender, EventArgs e)
        {
            rtbHistory.SelectionStart = rtbHistory.Text.Length;
            rtbHistory.ScrollToCaret();
            rtbHistory.Refresh();
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            ContextStrip.Show(btnMenu, 0, 0);
            
        }
        float rotation = 0f;
        private void tmrSpin_Tick(object sender, EventArgs e)
        {
            float movement = 11;
            rotation = (rotation + movement) % 360f;

            if (rotation % 2 == 0)
            {
                PacketHandler h = SMPInterface.Handler;
                h.SetOperationCode(SMPInterface.PacketTypes.PlayerPosition);
                
                double x = SMPInterface.PlayerX;
                double y = 126.0;
                double z = SMPInterface.PlayerZ;

                x += Math.Cos((rotation / 1.0) * Math.PI * 2.0) * 10.0;
                z += Math.Sin((rotation / 1.0) * Math.PI * 2.0) * 10.0;

                h.Write(SMPInterface.SwapByteOrder(x));
                h.Write(SMPInterface.SwapByteOrder(y));
                h.Write(SMPInterface.SwapByteOrder(y + 0.5));
                h.Write(SMPInterface.SwapByteOrder(z));
                h.Write(false);
                h.Flush();
            }
            else
            {

                float x = rotation;
                float y = 0f; // horizontal?
                PacketHandler h = SMPInterface.Handler;
                h.SetOperationCode(SMPInterface.PacketTypes.PlayerLook);
                h.Write(SMPInterface.SwapByteOrder(x));
                h.Write(SMPInterface.SwapByteOrder(y));
                h.Write(false);
                h.Flush();
            }
        }

        private void rtbHistory_TextChanged(object sender, EventArgs e)
        {

        }

        private void ContextStrip_Opening(object sender, CancelEventArgs e)
        {

        }

        private void forceMapDownloadMovePlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tmrSpin.Enabled = forceMapDownloadMovePlayerToolStripMenuItem.Checked;
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SMPInterface.Disconnect();
        }
    }
}
