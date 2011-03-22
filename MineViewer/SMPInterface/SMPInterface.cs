using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;


namespace MineViewer
{
    class PostData
    {
        private Dictionary<string, string> _Dict = new Dictionary<string, string>();
        public string URL;
        public PostData(string url)
        {
            URL = url;
        }
        public void Add(string arg, string data)
        {
            _Dict.Add(arg, data);
        }

        public string GetResult(bool get)
        {
            string postdata = "?";
            string and = "";
            foreach (KeyValuePair<string, string> kv in _Dict)
            {
                string str = string.Format("{0}={1}{2}", kv.Key, kv.Value, "&");
                postdata += str;
            }
            postdata = postdata.Remove(postdata.Length - 1, 1);

            WebClient cl = new WebClient();
            return cl.DownloadString(URL + postdata);
        }

        public string GetResult()
        {
            string postdata = "";
            string and = "";
            foreach (KeyValuePair<string, string> kv in _Dict)
            {
                string str = string.Format("{0}={1}{2}", kv.Key, kv.Value, "&");
                postdata += str;
            }
            postdata = postdata.Remove(postdata.Length - 1, 1);
            
            byte[] buffer = Encoding.ASCII.GetBytes(postdata);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = buffer.Length;
            req.Accept = "text/html, image/gif, image/jpeg, *; q=.2, */*; q =\x2e2"; // bs x2e
            req.UserAgent = "Java/1.6.0_16";

            Stream reqstream = req.GetRequestStream();
            reqstream.Write(buffer, 0, buffer.Length);
            reqstream.Close();
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();

            StreamReader retstream = new StreamReader(response.GetResponseStream());
            return retstream.ReadToEnd();
        }
    }

    public class MapChunkReader
    {
        private bool[] _Bits;
        public long Position = 0;
        public long End = 0;
        public bool EOS = false;
        public MapChunkReader(byte[] values)
        {
            _Bits = new bool[values.Length * 8];
            long writepos = 0;
            foreach (byte b in values)
                for (int i = 0; i < 8; i++)
                    _Bits[writepos++] = IsBitSet(b, i);
            End = writepos - 1;
            if(Position >= End)
                EOS = true;
        }

        public int Read(int bits)
        {
            int retval = 0;
            int n = 0;
            for (long i = Position; i < Position + bits; i++)
            {
                try
                {
                    if (_Bits[i])
                        retval += 1 << n;
                    n++;
                }catch{}
            }

            Position += bits;
            if (Position - 1 >= End) EOS = true;
            return retval;
        }

        private bool IsBitSet(byte val, int bit)
        {
            return (val & (1 << bit)) != 0;
        }
    }

    public class SMPInterface
    {
        public static string LastError = "";
        public static void Debug(string s)
        {
            System.Diagnostics.Debug.Write(s);
            try
            {
                Action d = new Action(delegate()
                {
                    if (s.EndsWith("\n"))
                        s = s.Substring(0, s.Length - 1);
                    string[] lines = new string[con.tbText.Lines.Length + 1];
                    if (con.tbText.Lines.Length > 0)
                        Array.Copy(con.tbText.Lines, lines, con.tbText.Lines.Length);
                    lines[lines.Length - 1] = s;
                    con.tbText.Lines = lines;

                    con.tbText.SelectionStart = con.tbText.Text.Length;
                    con.tbText.ScrollToCaret();
                    //con.tbText.Refresh();
                });
                con.Invoke(d);
            }
            catch { }
        }

        public static bool IsSMP;
        public static bool Kicked;
        public static bool Connected;
        public static double PlayerX;
        public static double PlayerY;
        public static double PlayerZ;

        public static TcpClient Connection;
        public static BinaryReader Reader;
        private static BinaryWriter Writer;
        public static PacketHandler Handler;

        private static Thread PacketResponder;

        public static Dictionary<Cubia.Point<int>, MinecraftLevel.Chunk> Chunks = new Dictionary<Cubia.Point<int>, MinecraftLevel.Chunk>();
        public static Dictionary<Cubia.Point<int>, MinecraftLevel.Chunk> TransChunks = new Dictionary<Cubia.Point<int>, MinecraftLevel.Chunk>();
        public static Dictionary<Cubia.Point<int>, bool> ChunksNeeded = new Dictionary<Cubia.Point<int>, bool>();
        public static MinecraftLevel.Chunk GetChunk(Cubia.Point<int> pos, bool trans)
        {
            MinecraftLevel.Chunk c;
            if (!trans)
                Chunks.TryGetValue(pos, out c);
            else
                TransChunks.TryGetValue(pos, out c);
            return c;
        }

        public static string Username;
        public static string SrvPassword;

        public static string CaseUsername;
        public static string SessionID;
        public static string SessionHash;

        public enum PacketTypes : byte
        {
            KeepAlive   = 0x00,
            LoginReq    = 0x01,
            Handshake   = 0x02,
            ChatMsg     = 0x03,
            TimeUpdate  = 0x04,
            EntityEquipment = 0x05,
            PlayerSpawnPos = 0x06,
            UseEntity   = 0x07,
            UpdateHealth= 0x08,
            Respawn     = 0x09,
            Player      = 0x0a,
            PlayerPosition = 0x0b,
            PlayerLook  = 0x0c,
            PlayerPosLook = 0x0d,
            PlayerDigging = 0x0e,
            PlayerBlockPlace = 0x0f,
            HoldingChange = 0x10,
            Animation   = 0x12,
            EntityAction = 0x13,
            NamedEntitySpawn = 0x14,
            PickupSpawn = 0x15,
            CollectItem = 0x16,
            AddObject   = 0x17,
            MobSpawn    = 0x18,
            Unknown2    = 0x19,
            EntVel      = 0x1c,
            DestroyEnt  = 0x1d,
            Entity      = 0x1e,
            EntRelMove  = 0x1f,
            EntLook     = 0x20,
            EntLookRelMove = 0x21,
            EntTeleport = 0x22,
            EntStatus   = 0x26,
            AttachEntity = 0x27,
            UnknownMetadata = 0x28,
            PreChunk    = 0x32,
            Chunk       = 0x33,
            MultiBlockChange = 0x34,
            BlockChange = 0x35,
            Unknown36     = 0x36,
            Explodie    = 0x3c,
            OpenWindow  = 0x64,
            CloseWindow = 0x65,
            SetSlot     = 0x67,
            WindowItems = 0x68,
            UpdateProgBar = 0x69,
            Transaction = 0x6a,
            UpdateSign  = 0x82,
            Kick        = 0xff
        }

        public static int SwapByteOrder(int val)
        {
            byte[] v = new byte[4]
            {
                (byte)(val>>24),
                (byte)(val>>16),
                (byte)(val>>8),
                (byte)val
            };
            return BitConverter.ToInt32(v, 0);
        }
        public static double SwapByteOrder(double val)
        {
            byte[] old = BitConverter.GetBytes(val);
            byte[] v = new byte[8]
            {
                old[7],
                old[6],
                old[5],
                old[4],
                old[3],
                old[2],
                old[1],
                old[0],
            };
            return BitConverter.ToDouble(v, 0);
        }
        public static short SwapByteOrder(short val)
        {
            byte[] v = new byte[2]
            {
                (byte)(val>>8),
                (byte)val
            };
            return BitConverter.ToInt16(v, 0);
        }

        public static int[] PacketTypeLengths = new int[255];


        static bool Loaded = false;
        static void LoadPackets()
        {
            if (Loaded) return;
            #region LENGTHS
            PacketTypeLengths[(int)PacketTypes.Transaction] = 4;
            PacketTypeLengths[(int)PacketTypes.UpdateProgBar] = 5;
            PacketTypeLengths[(int)PacketTypes.PreChunk] = 9;
            PacketTypeLengths[(int)PacketTypes.AttachEntity] = 8;
            PacketTypeLengths[(int)PacketTypes.EntStatus] = 5;
            PacketTypeLengths[(int)PacketTypes.EntTeleport] = 18;
            PacketTypeLengths[(int)PacketTypes.EntLookRelMove] = 9;
            PacketTypeLengths[(int)PacketTypes.EntLook] = 6;
            PacketTypeLengths[(int)PacketTypes.EntRelMove] = 7;
            PacketTypeLengths[(int)PacketTypes.Entity] = 4;
            PacketTypeLengths[(int)PacketTypes.DestroyEnt] = 4;
            PacketTypeLengths[(int)PacketTypes.EntVel] = 10;
            PacketTypeLengths[(int)PacketTypes.MobSpawn] = 19;
            PacketTypeLengths[(int)PacketTypes.AddObject] = 17;
            PacketTypeLengths[(int)PacketTypes.CollectItem] = 8;
            PacketTypeLengths[(int)PacketTypes.PickupSpawn] = 24;
            PacketTypeLengths[(int)PacketTypes.Animation] = 5;
            PacketTypeLengths[(int)PacketTypes.TimeUpdate] = 8;
            PacketTypeLengths[(int)PacketTypes.EntityEquipment] = 10;
            PacketTypeLengths[(int)PacketTypes.UseEntity] = 9;
            PacketTypeLengths[(int)PacketTypes.UpdateHealth] = 2;
            PacketTypeLengths[(int)PacketTypes.Respawn] = 0;
            PacketTypeLengths[(int)PacketTypes.Player] = 1;
            PacketTypeLengths[(int)PacketTypes.PlayerPosition] = 33;
            PacketTypeLengths[(int)PacketTypes.PlayerLook] = 9;
            PacketTypeLengths[(int)PacketTypes.PlayerPosLook] = 41;
            PacketTypeLengths[(int)PacketTypes.PlayerDigging] = 11;
            PacketTypeLengths[(int)PacketTypes.PlayerBlockPlace] = 12;
            PacketTypeLengths[(int)PacketTypes.HoldingChange] = 6;
            PacketTypeLengths[(int)PacketTypes.EntityAction] = 5;
            PacketTypeLengths[(int)PacketTypes.Unknown36] = sizeof(int) + sizeof(short) + sizeof(int) + sizeof(byte) + sizeof(byte);
            #endregion


            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (Type type in asm.GetTypes())
            {
                if (type.Namespace == "MineViewer.SMPPackets")
                {
                    MethodInfo info = type.GetMethod("Init");
                    info.Invoke(null, null);
                }
            }
            Loaded = true;
        }

        private static frmConsole con = new frmConsole();
        
        /// <summary>
        /// Connect to a server with the given IPEndPoint
        /// </summary>
        public static bool Connect(string IP, string _Username, string _Password, string _SrvPassword)
        {
            System.Net.ServicePointManager.MaxServicePointIdleTime = 100000;

            con = new frmConsole();
            con.Show();

            SMPInterface.IsSMP = true;
            LoadPackets();
            
            Connection = new TcpClient();
            Connection.SendTimeout = 500;
            Connection.NoDelay = false;
            Connection.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
            
            
            PacketResponder = new Thread(RespondWorker);
            PacketResponder.IsBackground = true;

            if (Auth(_Username, _Password))
            {
                Debug("Auth Successful\n");
                MineViewer.SMPPacketsCon.CF.Form = new frmSMPChat(delegate(string msg)
                {
                    SMPInterface.Handler.SetOperationCode(SMPInterface.PacketTypes.ChatMsg);
                    SMPInterface.Handler.Write(msg);
                });
                MineViewer.SMPPacketsCon.CF.Form.Show();
                Username = _Username;
                SrvPassword = _SrvPassword;

                string[] split = IP.Split(":".ToCharArray());
                if(split.Length > 1)
                    Connection.Connect(split[0], int.Parse(split[1]));
                else
                    Connection.Connect(IP, 25565);

                Reader = new BinaryReader(Connection.GetStream());
                Writer = new BinaryWriter(Connection.GetStream());

                Handler = new PacketHandler(Writer);

                Handler.SetOperationCode(PacketTypes.Handshake);
                Handler.Write(Username);
                Handler.Flush();

                PacketResponder.Start();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool AuthConnect(string _Username, string _ServerID)
        {
            string returns = "";
            try
            {
                PostData post = new PostData("http://www.minecraft.net/game/joinserver.jsp");
                post.Add("user", _Username);
                post.Add("sessionId", SessionID);
                post.Add("serverId", _ServerID);
                returns = post.GetResult(true);
            }
            catch { SMPInterface.LastError = "Could not connect to minecraft.net"; return false; }

            SMPInterface.Debug("Connect Authecation: " + returns + "\n");

            if (returns == "OK")
                return true;
            else
                return false;
        }

        /// <summary>
        /// Authecate the user to join
        /// </summary>
        private static bool Auth(string _Username, string _Password)
        {
            WebClient Client = new WebClient();
            string returns;
            try
            {
                Debug("Connecting to minecraft.net...\n");
                PostData post = new PostData("http://minecraft.net/game/getversion.jsp");
                post.Add("user", _Username);
                post.Add("password", _Password);
                post.Add("version", "12");
                returns = post.GetResult(true);
            }
            catch { SMPInterface.LastError = "Could not connect to minecraft.net"; return false; }
            SMPInterface.Debug("Authecation: " + returns + "\n");

            if (returns != "Bad login" && returns != "Old version" && returns != "Error")
            {
                string[] Bits = returns.Split(":".ToCharArray());

                SessionHash = Bits[1];
                CaseUsername = Bits[2];
                SessionID = Bits[3];

                return true;
            }
            else
            {
                SMPInterface.LastError = returns;
                return false;
            }

        }

        private static Dictionary<byte, Action> Subscribers = new Dictionary<byte, Action>();
        public static void Subscribe(PacketTypes packet, Action onCall)
        {
            Subscribers.Add((byte)packet, onCall);
        }

        /// <summary>
        /// Respond to the incomming stuff.
        /// </summary>
        private static void RespondWorker()
        {
            byte LastOPCode = 0;
            while (true)
            {
                
                byte OperationCode = (byte)PacketTypes.KeepAlive;

                if (Connection.Available == 0)
                    continue;
                try
                {
                    OperationCode = Reader.ReadByte();
                }
                catch
                {
                    continue;
                }
                /*
                if(OperationCode != (byte)PacketTypes.KeepAlive &&
                    OperationCode != (byte)PacketTypes.PreChunk &&
                    OperationCode != (byte)PacketTypes.Chunk)
                    SMPInterface.Debug("Packet Opcode: " + OperationCode.ToString("X8") + "\n");
                */
                Action act;
                if (Subscribers.TryGetValue(OperationCode, out act))
                    act.Invoke();
                else
                {
                    int len = PacketTypeLengths[OperationCode];
                    if (len == 0)
                    {
                        byte[] data = Reader.ReadBytes(100);
                        throw new Exception("Unhandeled packet: " + OperationCode.ToString("X8"));
                    }
                    Reader.ReadBytes(len);
                }
                LastOPCode = OperationCode;
            }
        }

        public static void Disconnect()
        {
            SMPInterface.Debug("Disconnected " + CaseUsername + "\n");

            PacketResponder.Abort();
            Reader.Close();
            Writer.Close();
        }

    }

    public class PacketHandler
    {
        public byte[] Buffer = new byte[1024];
        int Pointer = 0;

        BinaryWriter Writer;

        public PacketHandler(BinaryWriter _Writer)
        {
            this.Writer = _Writer;
        }

        /// <summary>
        /// Set the operation code (0x01 etc...)
        /// </summary>
        public void SetOperationCode(SMPInterface.PacketTypes OpCode)
        {
            Buffer[0] = (byte)OpCode;
            Pointer++;
            Flush();
            Pointer = 0; // 0;
        }

        /// <summary>
        /// Write a byte to the buffer
        /// </summary>
        public void Write(byte Value)
        {
            Buffer[Pointer] = Value;
            Pointer++;
        }

        /// <summary>
        /// Write a short to the buffer
        /// </summary>
        public void Write(short Value)
        {
            /*
            _Write(new byte[2] { 
                (byte)(Value), (byte)(Value >> 8) 
            });
            */

            _Write(new byte[2] { 
                (byte)(Value >> 8), (byte)(Value) 
            });
        }

        /// <summary>
        /// Write an integer to the buffer
        /// </summary>
        public void Write(int Value)
        {
            //_Write(new byte[4] { 
            //    (byte)(Value), (byte)(Value >> 8), (byte)(Value >> 16), (byte)(Value >> 24)
            //});

            _Write(new byte[4] { 
                (byte)(Value >> 24), (byte)(Value >> 16), (byte)(Value >> 8), (byte)(Value)
            });
        }

        /// <summary>
        /// Write a long to the buffer
        /// </summary>
        public void Write(long Value)
        {
            _Write(new byte[8] { 
                (byte)(Value), (byte)(Value >> 8), (byte)(Value >> 16), (byte)(Value >> 24), 
                (byte)(Value >> 32), (byte)(Value >> 40), (byte)(Value >> 48), (byte)(Value >> 56)
            });
        }

        /// <summary>
        /// Write a float to the buffer
        /// </summary>
        public void Write(float Value)
        {
            _Write(System.BitConverter.GetBytes(Value));
        }

        /// <summary>
        /// Write a double to the buffer
        /// </summary>
        public void Write(double Value)
        {
            _Write(System.BitConverter.GetBytes(Value));
        }

        /// <summary>
        /// Write a string to the buffer
        /// </summary>
        public void Write(string Value)
        {
            byte[] byteValue = Encoding.UTF8.GetBytes(Value);

            Write((short)Value.Length);
            _Write(byteValue);
        }

        /// <summary>
        /// Write a bool to the buffer
        /// </summary>
        public void Write(bool Value)
        {
            _Write(System.BitConverter.GetBytes(Value));
        }

        /// <summary>
        /// Private write to the buffer, suggested by c0bra
        /// </summary>
        public void _Write(byte[] Values)
        {
            foreach (byte b in Values)
            {
                Buffer[Pointer] = b;
                Pointer++;
            }
        }
        
        /// <summary>
        /// Send the shit down the toilet
        /// </summary>
        public void Flush()
        {
            byte[] BufferSend = new byte[Pointer];
            for (int i = 0; i < Pointer; i++)
            {
                BufferSend[i] = Buffer[i];
                //char[] Stuff = Buffer[i].ToString("X8").ToCharArray();
                //SMPInterface.Debug(Stuff[6].ToString() + Stuff[7].ToString() + " ");
            }
            
            //SMPInterface.Debug("\n");
            try
            {
                Writer.Write(BufferSend);
            }
            catch (Exception ex) { SMPInterface.Debug(ex.Message); }
            // Reset Buffer
            Buffer = new byte[1024];
            Pointer = 0;
        }
    }
}
