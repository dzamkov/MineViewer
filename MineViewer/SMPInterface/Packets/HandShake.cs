
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
    public static class HandShake
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.Handshake, Call);
        }
        public static int ProtoclVersion = 9;
        private static void Call()
        {
            SMPInterface.Reader.ReadByte();
            SMPInterface.Debug("Handshake (0x02)" + "\n");

            string ConnectionHash = SMPInterface.Reader.ReadString();
            SMPInterface.Debug("Got connection hash, length: " + ConnectionHash.Length.ToString() + "\n");
            if (ConnectionHash != "-")
            {
                if (!SMPInterface.AuthConnect(SMPInterface.CaseUsername, ConnectionHash))
                {
                    SMPInterface.Disconnect();
                    SMPInterface.Debug("Failed to auth connect.\n");
                    return;
                }
            }
            else
            {
                SMPInterface.Debug("Server is not authing\n");
            }
            SMPInterface.Debug("Sending Login Request\n");

            SMPInterface.Handler.SetOperationCode(SMPInterface.PacketTypes.LoginReq);
            SMPInterface.Handler.Write((int)ProtoclVersion);
            SMPInterface.Handler.Write(SMPInterface.Username);

            if (SMPInterface.SrvPassword.Length > 0)
                SMPInterface.Handler.Write(SMPInterface.SrvPassword);
            else
                SMPInterface.Handler.Write("Password");

            SMPInterface.Handler._Write(new byte[9]); // stuff we don't need
            SMPInterface.Handler.Flush();
        }
    }

    public static class LoginReq
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.LoginReq, Call);
        }

        private static void Call()
        {
            SMPInterface.Debug("Logged in\n");
            int id = SMPInterface.Reader.ReadInt32();
            SMPInterface.Reader.ReadByte();
            string u1 = SMPInterface.Reader.ReadString();
            SMPInterface.Reader.ReadByte();
            string u2 = SMPInterface.Reader.ReadString();
            long mapseed = SMPInterface.Reader.ReadInt64();
            byte dim = SMPInterface.Reader.ReadByte();
        }
    }

}