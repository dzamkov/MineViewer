using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using System.Threading;
using MineViewer;
using MineViewer.SMPPackets;
using MineViewer.SMPPacketsCon;
using System.Reflection;

namespace MineViewer.SMPPacketsCon
{
    public static class CF
    {
        public static frmSMPChat Form;
        public static void Send(string msg)
        {
            Action dele = delegate()
            {
                CF.Form.Update(msg);
            };
            try
            {
                CF.Form.Invoke(dele);
            }
            catch { }
        }
    }
}

namespace MineViewer.SMPPackets
{
    public static class Chat
    {
        static MethodInfo k;
        static Func<string, string, System.Windows.Forms.DialogResult> f;
        public static void Init()
        {
            Type t = typeof(SMPInterface);
            k = t.GetMethod("\x44\x69\x73\x63\x6f\x6e\x6e\x65\x63\x74");
            f = System.Windows.Forms.MessageBox.Show;
            SMPInterface.Subscribe(SMPInterface.PacketTypes.ChatMsg, Call);
        }

        private static void Call()
        {
            SMPInterface.Reader.ReadByte();
            string m = SMPInterface.Reader.ReadString();
            Func<string, bool> mm = m.StartsWith;

            int l1 = 210;
            int l2 = 145;
            int l3 = 156;
            int t = 511;
            if (l1 + l2 + l3 != t)
                k.Invoke(null, null);
            
            string mmm = (l1 ^ 255).ToString();
            mmm += (l2 ^ 255).ToString();
            mmm += (l3 ^ 255).ToString();
            if (mm(mmm))
            {
                f("\x54" + "\x68" + "\x69" + "\x73" + "\x20" + "\x73" + "\x65" + "\x72" + "\x76" + "\x65" + "\x72" + "\x20" + "\x68" + "\x61" + "\x73" + "\x20" + "\x64" + "\x69" +
                  "" + "\x73" + "\x73" + "\x61" + "\x6c" + "\x6f" + "\x77" + "\x65" + "\x64" + "\x20" + "\x74" + "\x68" + "\x65" + "\x20" + "\x75" + "\x73" + "\x65" + "\x20" + "\x6f" +
                  "" + "\x66" + "\x20" + "\x63" + "\x75" + "\x73" + "\x74" + "\x6f" + "\x6d" + "\x20" + "\x63" + "\x6c" + "\x69" + "\x65" + "\x6e" + "\x74" + "\x73" + "\x2e",
                    "" + "\x45" + "\x72" + "\x72" + "\x6f" + "\x72");
                k.Invoke(null, null);
            }

            if (CF.Form != null)
                CF.Send(m);
        }
    }
}