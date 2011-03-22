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

namespace MineViewer.SMPPackets
{
    public static class Kick
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.Kick, Call);
        }

        private static void Call()
        {
            SMPInterface.Reader.ReadByte();

            string Reason = SMPInterface.Reader.ReadString();
            SMPInterface.Debug("Kicked: " + Reason + "\n");

            SMPInterface.Kicked = true;
            System.Windows.Forms.MessageBox.Show("Kicked: " + Reason);
        }
    }
}