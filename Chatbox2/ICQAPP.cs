using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace ChatBox2
{
    public enum MessageStatus : byte { UnknownError, AcceptedForDelivery, BuddyOffline, OfflineMessagesNotSupported, OfflineStorageFull, MessageTimeout, BuddyBlocked, Received, Sending, None }
    public enum ConnectionStatus : byte { Connected, LoginError, Kicked, Disconnected, Connecting }

    public class Im
    {
        string _msg;
        public string msg
        {
            get
            {
                if (_msg.Length > 4000) return _msg.Substring(0, 4000);
                else return _msg;
            }
            set { _msg = value; }
        }
        public string uin;
        public MessageStatus Status = MessageStatus.None;
        public DateTime _DateTime = DateTime.Now;
    }
    public class ICQAPP
    {
        public void Disconnect()
        {
            _ConnectionStatus = ConnectionStatus.Disconnected;
            _Thread.Abort();
            _Socket.Close();            
        }
        public ConnectionStatus _ConnectionStatus = ConnectionStatus.Disconnected;
        public static Random _Random = new Random();
        public T Trace2<T>(T t)
        {
            return Trace(t.Trace());
        }
        public T Trace<T>(T t)
        {
            if (!File.Exists(_uin + ".txt")) File.WriteAllText(_uin + ".txt", "");
            File.AppendAllText(_uin + ".txt", t + "\r\n", Encoding.Default);
            return t;
        }
        public delegate void Message(Im im);
        public event Message _onMessage;
        public event Message _onMessageStatusChanged;
        public string _uin;
        public string _passw;
        ushort _srvseq;

        ushort _cliseq = (ushort)_Random.Next(ushort.MaxValue);
        Socket _Socket;
        public Thread _Thread;
        ConnectionStatus _oldConnectionStatus;
        public void StartAsync()
        {
            if (_Thread != null && _Thread.ThreadState != System.Threading.ThreadState.Stopped)
            {
                Trace2("Error:Thread Already exists");
                return;
            }
            _oldConnectionStatus = _ConnectionStatus;
            _ConnectionStatus = ConnectionStatus.Connecting;
            _Thread = new Thread(StartListen);
            _Thread.Name = _uin;
            _Thread.Start();
        }
        private void StartListen()
        {
            try
            {
                try
                {
                    Connect();
                }
                catch (IOException) { throw new ExceptionB("conn error2"); }
                catch (SocketException) { throw new ExceptionB("conn error2"); }
                Thread.Sleep(2000);
                _ConnectionStatus = ConnectionStatus.Connected;
                Trace2(Trace("Connected " + _uin));
                while (true)
                {
                    Flap _Flap = new Flap { _ICQ = this };
                    _Flap.Receive();
                    Trace("FlapReceived");
                    Trace(_Flap._data.ToArray().ToHex());
                    Trace("Snac Type Of" + _Flap.ch);
                    switch (_Flap.ch)
                    {
                        default:
                            break;
                        case 2:
                            {
                                _Flap.ReadSnac();
                                if (_Flap._Snac.ID1 == 4)
                                {
                                    Im _Im = null;
                                    if (_Dictionary.Keys.Contains(_Flap._Snac.req))
                                        _Im = _Dictionary[_Flap._Snac.req];

                                    if (_Flap._Snac.ID2 == 7) //message
                                    {
                                        ReadMsg1(_Flap);
                                    }
                                    if (_Flap._Snac.ID2 == 12 && _Im != null) //accepted
                                    {
                                        _Im.Status = MessageStatus.AcceptedForDelivery;
                                        OnMessageStatusChanged(_Im);
                                        _Dictionary.Remove(_Flap._Snac.req);

                                    }
                                    if (_Flap._Snac.ID2 == 1 && _Im != null)
                                    {
                                        _Im.Status = MessageStatus.UnknownError;
                                        _Dictionary.Remove(_Flap._Snac.req);
                                        int errorcode = _Flap._data.ReadUInt16();
                                        _Flap.ReadTvl();
                                        int subcode = 0;
                                        Tvl _Tvl = _Flap.GetTvl(0x0008);
                                        if (_Tvl != null)
                                            subcode = BitConverter.ToUInt16(_Tvl.data.ReverseA(2), 0);
                                        switch (errorcode)
                                        {
                                            case 0x0004:
                                                _Im.Status = MessageStatus.BuddyOffline;
                                                break;
                                            case 0x0010:
                                                _Im.Status = MessageStatus.BuddyBlocked;
                                                break;
                                        }
                                        switch (subcode)
                                        {
                                            case 0x000E:
                                                _Im.Status = MessageStatus.OfflineMessagesNotSupported;
                                                break;
                                            case 0x000F:
                                                _Im.Status = MessageStatus.OfflineStorageFull;
                                                break;
                                        }
                                        OnMessageStatusChanged(_Im);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (ExecutionEngineException) { }
            catch (IOException e)
            {
                Trace(e);                
                _ConnectionStatus = ConnectionStatus.Kicked;
            }
            catch (SocketException e)
            {
                Trace(e);                
                _ConnectionStatus = ConnectionStatus.Kicked;

            }
            catch (ExceptionB e)
            {
                Trace(e);                                
                _ConnectionStatus = ConnectionStatus.LoginError;
            }
            Trace2("disconnected:" + _ConnectionStatus);
            errors++;
        }
        public int errors;
        public void OnMessageStatusChanged(Im im)
        {
            if (_onMessageStatusChanged != null) _onMessageStatusChanged(im);
            Trace(im.uin + "->" + im.Status);
        }
        public Dictionary<uint, Im> _Dictionary = new Dictionary<uint, Im>();
        private void ReadMsg1(Flap _Flap)
        {
            _Flap._data.Read(8); //cookie
            int channel = _Flap._data.ReadUInt16();

            if (channel == 1)
            {
                //readuserinfo
                int len = _Flap._data.ReadB();
                string uin = _Flap._data.Read(len).ToStr();
                _Flap._data.ReadUInt16(); //WarningLevel                                    
                int tvlcount = _Flap._data.ReadUInt16();
                _Flap.ReadTvl(tvlcount);
                _Flap.ReadTvl(); //readTvltoend
                Tvl _Tvl = _Flap.GetTvl(0x0002);
                using (MemoryStream _MemoryStream = new MemoryStream(_Tvl.data))
                    while (_MemoryStream.Position != _MemoryStream.Length)
                    {
                        int id = _MemoryStream.ReadB();
                        _MemoryStream.ReadB();//version
                        byte[] _bytes = _MemoryStream.Read(_MemoryStream.ReadUInt16());
                        if (id == 1)//identifier                                                
                            ReadMsg2(_bytes, uin, _Flap);
                    }
            }
        }
        private void ReadMsg2(byte[] _bytes, string uin, Flap _Flap)
        {
            using (MemoryStream _MemoryStream = new MemoryStream(_bytes))
            {

                ushort charset = _MemoryStream.ReadUInt16();
                ushort language = _MemoryStream.ReadUInt16();
                _bytes = _MemoryStream.Read();
                Encoding incoming = charset == 2 ? Encoding.BigEndianUnicode : Encoding.ASCII;
                incoming = Encoding.GetEncoding(incoming.CodePage, new EncoderExceptionFallback(), new DecoderExceptionFallback());
                string msg;
                try { msg = incoming.GetString(_bytes); }
                catch (DecoderFallbackException) { msg = Encoding.Default.GetString(_bytes); }
                Im _IM = new Im { msg = msg, uin = uin, Status = MessageStatus.Received };
                if (_onMessage != null) _onMessage(_IM);

            }
        }

        public void SendMessage(Im im)
        {
            try
            {
                if (_ConnectionStatus != ConnectionStatus.Connected) throw new Exception("Not Connected");
                Flap _Flap = new Flap { _ICQ = this };
                _Flap.ch = 2;
                _Flap._Snac.ID1 = 4;
                _Flap._Snac.ID2 = 6;
                _Flap.WriteSnac();
                _Dictionary.Add(_Flap._Snac.req, im);
                _Flap._data.Write("00 00 00 00 00 00 00 00 00 01".Hex());
                byte[] sendto = ASCIIEncoding.ASCII.GetBytes(im.uin);
                _Flap._data.WriteByte((byte)sendto.Length);
                _Flap._data.Write(sendto);
                _Flap._data.Write(new byte[] { 0, 6, 0, 0 });
                Tvl _Tvl02 = new Tvl() { data = new byte[] { 05, 01, 00, 02, 01, 01 }, type = 2 };
                Tvl _Tvl0101 = new Tvl() { type = 257, data = ("\0\0\0\0" + im.msg).ToBytes() };
                _Tvl02.data = _Tvl02.data.Join(_Tvl0101.ToBytes());
                _Flap._Tvls.Add(_Tvl02);
                _Flap.WriteTvl().Send();
                im.Status = MessageStatus.Sending;
                im._DateTime = DateTime.Now;
            }
            catch (IOException) { Trace2(_uin + "Error msg send failed"); }
        }
        public static byte[] XOR(string s)
        {
            byte[] chars = new byte[] { 0xF3, 0x26, 0x81, 0xC4, 0x39, 0x86, 0xDB, 0x92, 0x71, 0xA3 };
            List<byte> list = new List<byte>();
            for (int i = 0; i < s.Length; i++)
            {
                list.Add((byte)(s[i] ^ chars[i]));
            }
            return list.ToArray();
        }


        public static List<string> servers = new List<string>() { "ibucp-vip-m.blue.aol.com", "ibucp-vip-d.blue.aol.com", "login.oscar.aol.com", "login.icq.com", "login.messaging.aol.com" };

        private void Connect()
        {

            if (_Socket != null)
            {
                _Socket.Close();                
            }
            _NetworkStream = new MemoryStream();
            TcpClient _TcpClient = new TcpClient(servers.Pop(), 5190);
            _Socket = _TcpClient.Client;
            _NetworkStream = new NetworkStream(_Socket);
            new Flap { _ICQ = this }.Receive(); // receive hello
            Flap _Flap2 = new Flap { _ICQ = this };
            _Flap2.ch = 1;
            _Flap2._data.Write(new byte[] { 0, 0, 0, 1 });
            _Flap2._Tvls.Add(new Tvl { type = 1, data = _uin.ToBytes() });
            _Flap2._Tvls.Add(new Tvl { type = 2, data = XOR(_passw) });
            _Flap2._Tvls.Add(new Tvl { type = 3, data = "chatbot".ToBytes() });
            _Flap2.WriteTvl();
            _Flap2.Send(@"00 16 00 02 01 0A                           
   00 17 00 02 00 05                        
   00 18 00 02 00 25                        
   00 19 00 02 00 01                        
   00 1A 00 02 0E 90                        
   00 14 00 04 00 00 00 55                  
   00 0F 00 02 65 6E                        
   00 0E 00 02 75 73".Hex());

            Flap _Flap = new Flap { _ICQ = this }.Receive().ReadTvl(); //1 uin //2 ip 3 cookie
            Match m = Regex.Match(_Flap._Tvls[2].data.ToStr(), @"(.+?):(\d+)");
            if (!m.Success) throw new ExceptionB("cannot Connect");
            ip = m.Groups[1].Value;

            port = int.Parse(m.Groups[2].Value);
            _Cookie = _Flap._Tvls[3].data;
            _Socket.Close();
            BossSimple();            
        }
        private string ip;
        private int port;
        private byte[] _Cookie;
        public void KeepAllive()
        {
            new Flap { _ICQ = this, ch = 5 }.Send();
        }
        public void BossSimple()
        {
            TcpClient _TcpClient = new TcpClient(ip, port);
            _Socket = _TcpClient.Client;
            _NetworkStream = new NetworkStream(_Socket);
            Flap hello = new Flap { _ICQ = this }.Receive(); //receive hello

            Flap _Flap1 = new Flap { _ICQ = this, ch = 1 };
            _Flap1._data.Write(new byte[] { 0, 0, 0, 1 });
            _Flap1._Tvls.Add(new Tvl { data = _Cookie, type = 6 });
            _Flap1.WriteTvl().Send();
            Flap _Flap2 = new Flap { _ICQ = this }.Receive().ReadSnac();
            Flap _Flap3 = new Flap { _ICQ = this };
            _Flap3.ch = 2;
            _Flap3._Snac.ID1 = 1;
            _Flap3._Snac.ID2 = 2;
            _Flap3._Snac.flag1 = _Flap3._Snac.flag2 = 0;
            _Flap3.WriteSnac();//.Write();
            while (_Flap2._data.Position != _Flap2._data.Length)
            {
                UInt16 a = _Flap2._data.ReadUInt16();
                _Flap3._data.WriteUint16(a);
                _Flap3._data.Write("00 01 01 10 04 7B".Hex());
            }
            _Flap3.Send();
            Flap _Flap4 = new Flap { _ICQ = this };
            _Flap4.Receive().ReadSnac();
            if (_Flap4._Snac.ID1 != 11 || _Flap4._Snac.ID2 != 2) throw new Exception();
            Thread.Sleep(1000);            
        }
        public void SendInfo()
        {
            string s = @"00 15-00 02 00 00 00 1D 00 02
00 01 01 A5 A3 01 30 73-A2 1B D0 07 00 1D 3A 0C
52 03 02 00 E3 04 5E 01-04 00 01 00 00 00 5E 01
04 00 01 00 00 00 5E 01-04 00 01 00 00 00 48 03
01 00 00 76 02 03 00 01-00 00 80 02 03 00 01 00
00 8A 02 03 00 01 00 00-C6 02 03 00 01 00 00 D0
02 03 00 01 00 00 54 01-0A 00 08 00 43 68 61 74
42 6F 78 00 40 01 07 00-05 00 43 68 61 74 00 4A
01 06 00 04 00 76 33 33-00 58 02 20 00 1E 00 C4
EE E1 F0 EE 20 EF EE E6-E0 EB EE E2 E0 F2 FC 20
E2 20 F7 E0 F2 20 43 53-4C 49 56 45 00 72 01 02
00 00 00 7C 01 01 00 02-3A 02 06 00 00 00 00 00
00 00 86 01 02 00 00 00-86 01 02 00 00 00 86 01
02 00 00 00 3E 03 01 00-00 B8 01 03 00 01 00 00
C2 01 03 00 01 00 00 AE-01 03 00 01 00 00 94 02
03 00 01 00 00 A8 02 03-00 01 00 00 9E 02 03 00
01 00 00 DA 02 03 00 01-00 00 BD 02 03 00 01 00
00 B2 02 02 00 00 00 CC-01 02 00 00 00 90 01 03
00 01 00 00 9A 01 03 00-01 00 00 A4 01 02 00 66
01 20 03 03 00 01 00 00-2A 03 03 00 01 00 00 34
03 02 00 00 00 62 02 03-00 01 00 00 6D 02 03 00
01 00 00 13 02 03 00 01-00 00 16 03 01 00 00 EA
01 05 00 00 00 01 00 00-EA 01 05 00 00 00 01 00
00 EA 01 05 00 00 00 01-00 00 EA 01 05 00 00 00
01 00 00 D6 01 05 00 00-00 01 00 00 D6 01 05 00
00 00 01 00 00 D6 01 05-00 00 00 01 00 00 FE 01
05 00 00 00 01 00 00 FE-01 05 00 00 00 01 00 00
FE 01 05 00 00 00 01 00-00";
            new Flap { _ICQ = this, ch = 2 }.Send(s.Hex());
        }
        private Stream _NetworkStream;
        public class Tvl
        {
            public ushort type;
            public byte[] data;

            public byte[] ToBytes()
            {
                using (MemoryStream _MemoryStream = new MemoryStream(data.Length + 4))
                {
                    _MemoryStream.WriteUint16(type);
                    _MemoryStream.WriteUint16((UInt16)data.Length);
                    _MemoryStream.Write(data);
                    return _MemoryStream.ToArray();
                }
            }
        }
        class Flap
        {
            public Tvl GetTvl(ushort type)
            {
                Tvl _Tvl = _Tvls.SingleOrDefault(a => a.type == type);
                return _Tvl;
            }
            public ICQAPP _ICQ;
            public Stream _NetworkStream { get { return _ICQ._NetworkStream; } }
            public ushort _srvseq { get { return _ICQ._srvseq; } set { _ICQ._srvseq = value; } }
            public ushort _cliseq { get { return _ICQ._cliseq; } set { _ICQ._cliseq = value; } }
            public byte ch;
            public MemoryStream _data = new MemoryStream();
            public List<Tvl> _Tvls = new List<Tvl>();

            public Flap Receive()
            {
                if (_NetworkStream.ReadB() != 0x2A) throw new Exception();
                ch = _NetworkStream.ReadB();
                _srvseq = _NetworkStream.ReadUInt16();
                int length = _NetworkStream.ReadUInt16();
                _data.Write(_NetworkStream.Read(length));
                _data.Seek(0, SeekOrigin.Begin);
                return this;
            }
            public class Snac
            {
                public ushort ID1;
                public ushort ID2;
                public byte flag1;
                public byte flag2;
                public uint req = (uint)_Random.Next(int.MaxValue);
                public bool Equals(ushort a, ushort b)
                {
                    return ID1 == a && ID2 == b;
                }
            }
            public Snac _Snac = new Snac();

            public Flap ReadSnac()
            {
                _Snac.ID1 = _data.ReadUInt16();
                _Snac.ID2 = _data.ReadUInt16();
                _Snac.flag1 = _data.ReadB();
                _Snac.flag2 = _data.ReadB();
                _Snac.req = _data.ReadUInt32();
                return this;
            }
            public Flap WriteSnac()
            {
                _data.WriteUint16(_Snac.ID1);
                _data.WriteUint16(_Snac.ID2);
                _data.WriteByte(_Snac.flag1);
                _data.WriteByte(_Snac.flag2);
                _data.WriteUint32(_Snac.req);
                return this;
            }
            public static List<Tvl> ReadTvl(byte[] _data)
            {
                using (MemoryStream _MemoryStream = new MemoryStream(_data))
                    return ReadTvl(_MemoryStream);
            }

            public static List<Tvl> ReadTvl(MemoryStream _data)
            {
                return ReadTvl(_data, -1);
            }

            public static List<Tvl> ReadTvl(MemoryStream _data, int tvlscount)
            {
                if (tvlscount == 0) throw new Exception("0 tvls");
                List<Tvl> _tvls = new List<Tvl>();
                while ((_data.Length != _data.Position && tvlscount == -1) || tvlscount-- > 0)
                {
                    Tvl _Tvl = new Tvl();
                    _Tvl.type = _data.ReadUInt16();
                    int length = _data.ReadUInt16();
                    if (length > 0) _Tvl.data = _data.Read(length);
                    _tvls.Add(_Tvl);
                }
                return _tvls;
            }
            public Flap ReadTvl(int count)
            {
                _Tvls = ReadTvl(_data, count);
                return this;
            }
            public Flap ReadTvl()
            {
                _Tvls = ReadTvl(_data);
                return this;
            }
            public Flap WriteTvl()
            {
                foreach (Tvl _Tvl in _Tvls)
                {
                    _data.WriteUint16((ushort)_Tvl.type);
                    if (_Tvl.data != null)
                    {
                        _data.WriteUint16((ushort)_Tvl.data.Length);
                        _data.Write(_Tvl.data);
                    }
                    else _data.WriteUint16((ushort)0);
                }
                return this;
            }
            public void Send()
            {
                Send(null);
            }
            public void Send(byte[] _bytes)
            {

                if (ch == 0) throw new Exception("Channel is null");
                if (_bytes != null) _data.Write(_bytes);
                _NetworkStream.WriteByte(0x2A);
                _NetworkStream.WriteByte(ch);
                _NetworkStream.WriteUint16((UInt16)(++_cliseq));
                _NetworkStream.WriteUint16((UInt16)_data.Length);
                if (_data.Length != 0) _NetworkStream.Write(_data.ToArray());

            }
        }
        public void BossAdv()
        {
            TcpClient _TcpClient = new TcpClient(ip, port);
            _Socket = _TcpClient.Client;
            _NetworkStream = new NetworkStream(_Socket);
            Flap hello = new Flap { _ICQ = this }.Receive(); //receive hello

            Flap _Flap1 = new Flap { _ICQ = this, ch = 1 }; // отправляем куки
            _Flap1._data.Write(new byte[] { 0, 0, 0, 1 });
            _Flap1._Tvls.Add(new Tvl { data = _Cookie, type = 6 });
            _Flap1.WriteTvl().Send();
            Flap _Flap2 = new Flap { _ICQ = this }.Receive().ReadSnac(); // SRV_FAMILIES
            new Flap { _ICQ = this, ch = 2 }.Send(@"00 01-00 17 00 00 00 00 00 00   
00 01 00 03 00 13 00 02-00 02 00 01 00 03 00 01   
00 15 00 01 00 04 00 01-00 06 00 01 00 09 00 01
00 0A 00 01 00 0B 00 01-".Hex()); // CLI_FAMILIES

            Flap _Flap4 = new Flap { _ICQ = this }.Receive();// SRV_FAMILIES2
            Flap _Flap5 = new Flap { _ICQ = this }.Receive();// SRV_MOTD                
            new Flap() { _ICQ = this, ch = 2 }.Send("00 01-00 06 00 00 00 00 00 00".Hex()); // CLI_RATESREQUEST              
            Flap _Flap6 = new Flap() { _ICQ = this }.Receive(); // SRV_RATES
            new Flap() { _ICQ = this, ch = 2 }.Send("00 01-00 08 00 00 00 00 00 00 00 01 00 02 00 03 00 04-00 05".Hex());// CLI_ACKRATES (ответим серверу что мы получили его SRV_RATES)                

            new Flap() { _ICQ = this, ch = 2 }.Send("00 04-00 02 00 00 00 00 00 00 00 00 00 00 00 03 1F 40-03 E7 03 E7 00 00 00 00".Hex());// CLI_SETICBM                                 
            new Flap() { _ICQ = this, ch = 2 }.Send("00 01-00 0E 00 00 00 00 00 00".Hex());// CLI_REQINFO
            new Flap() { _ICQ = this, ch = 2 }.Send("00 02-00 02 00 00 00 00 00 00".Hex());// CLI_REQLOCATION
            new Flap() { _ICQ = this, ch = 2 }.Send("00 03-00 02 00 00 00 00 00 00".Hex());// CLI_REQBUDDY
            new Flap() { _ICQ = this, ch = 2 }.Send("00 04-00 04 00 00 00 00 00 00".Hex());// CLI_REQICBM
            new Flap() { _ICQ = this, ch = 2 }.Send("00 09-00 02 00 00 00 00 00 00".Hex());// CLI_REQBOS
            Flap _Flap7 = new Flap { _ICQ = this }.Receive(); //SRV_REPLYINFO
            Flap _Flap8 = new Flap { _ICQ = this }.Receive(); //SRV_REPLYLOCATION
            Flap _Flap9 = new Flap { _ICQ = this }.Receive(); //SRV_REPLYBUDDY
            Flap _Flap10 = new Flap { _ICQ = this }.Receive();//SRV_REPLYICBM
            Flap _Flap11 = new Flap { _ICQ = this }.Receive();//SRV_REPLYBOS
            new Flap() { _ICQ = this }.Send(@"00 02-00 04 00 00 00 00 00 00   
                  00 05 00 10 09 46 13 41-4C 7F 11 D1 82 22 44 45   
                  53 54 00 00".Hex());// CLI_SETUSERINFO - капабилды клиента (в TLV5)


            new Flap { _ICQ = this, ch = 2 }.Send("00 09-00 07 00 00 00 00 00 00".Hex());// CLI_ADDINVISIBLE 
            new Flap { _ICQ = this, ch = 2 }.Send("00 01-00 11 00 00 00 00 00 00 00 00 00 00".Hex());//SNAC(1,11)(зх шоза снэк)
            new Flap { _ICQ = this, ch = 2 }.Send(@"00 01 00 1E 00 00   
00 00 00 00 00 06 00 04-00 00 00 00 00 08 00 02   
00 00 00 0C 00 25 59 BD-9B DD 00 00 0B B8 04 00   
08 2D A8 4E 56 00 00 00-50 00 00 00 03 00 00 00   
00 00 00 00 00 00 00 00-00 00 00".Hex());// CLI_REQINFO 

            new Flap { _ICQ = this, ch = 2 }.Send(@"00 01 00 02 00 00 00-00 00 00 00 01 00 03 01   
10 04 7B 00 13 00 02 01-10 04 7B 00 02 00 01 01   
01 04 7B 00 03 00 01 01-10 04 7B 00 15 00 01 01   
10 04 7B 00 04 00 01 01-10 04 7B 00 06 00 01 01   
10 04 7B 00 09 00 01 01-10 04 7B 00 0A 00 01 01   
10 04 7B 00 0B 00 01 01-10 04 7B".Hex());//CLI_READY         

            Flap _Flap12 = new Flap { _ICQ = this }.Receive(); // Как видим, тут у нас есть SNAC B,2 (00 0B 00 02) - следовательно мы успешно зашли =)


        }
    }
}
