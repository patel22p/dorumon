using System;
using System.Collections.Generic;
using System.Linq;
using doru;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using System.Net.Sockets;
using System.Net;
using System.Web;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace ChatBox2
{
    public class Program
    {
        //public class User
        //{
        //    public string _room { get; set; }
        //    public string ip { get; set; }
        //    public string nick { get; set; }
        //    public string _Info { get; set; }
        //    public DateTime _DateTime { get; set; }
        //    public DateTime _Banned { get; set; }
        //    [XmlIgnore]
        //    
        //    public User()
        //    {
        //        _DateTime = DateTime.Now;
        //    }
        //}
        public enum Type { Icq, Http };
        public class User 
        {            
            public StringCollection _MsgsToSend = new StringCollection();            
            public string _PasswordHash { get { return _DUser._PasswordHash; } set { _DUser._PasswordHash = value; } }
            public string _ip { get { return _DUser._Ip; } set { _DUser._Ip= value; } }
            public DateTime _Mute { get { return _DUser._Mute; } set { _DUser._Mute = value; } }
            public bool _notifydisabled { get { return _DUser._notifydisabled; } set { _DUser._notifydisabled = value; } }
            public string _room { get { return _DUser._room; } set { _DUser._room = value; } }
            public string _Info { get { return _DUser.info; } set { _DUser.info = value; } }
            public Icq _Account;
            public Database.User _DUser = new Database.User();
            public DateTime _DateTime { get { return _DUser._DateTime; } set { _DUser._DateTime = value; } }
            public string uin { get { return _DUser.uin; } set { _DUser.uin = value; } }
            public string nick { get { return _DUser.nick; } set { _DUser.nick = value; } }
            public DateTime _Banned { get { return _DUser._Banned; } set { _DUser._Banned = value; } }
            public bool _HideUin { get { return _DUser._HideUin; } set { _DUser._HideUin = value; } }
            public void SendMessage(string msg)
            {
                if (msg.StartsWith("<<notify>>") && _notifydisabled)
                    return;
                else
                    _Account.SendMessage(uin, msg);
            }
        }
        public static List<Icq> _Accounts = new List<Icq>();
        public class Database
        {
            public string IpAddress;
            public List<string> rooms = new List<string>();// { "general", "dev", "site" };
            public List<string> _RemovedUsers = new List<string>();
            public class User
            {
                public string _Ip;
                public Type _Type = Type.Icq;
                public string _PasswordHash;
                public bool _notifydisabled;
                public string _room;
                public string info;
                public DateTime _DateTime;
                public string uin;
                public string nick;
                public DateTime _Banned;
                public DateTime _Mute;
                public bool _HideUin;
            }
            public int limit = 5;
            public List<Icq> _Accounts = new List<Icq>();
            public List<User> _Users = new List<User>();
            public class Icq
            {
                public string uin;
                public string pass;
                public List<User> _Users = new List<User>();
            }

        }
        
        public static Dictionary<string, List<string>> _Dictionary2 = new Dictionary<string, List<string>>();
        public static List<string> GetRoom(string file)
        {
            file = file + ".txt";
            if (_Dictionary2.ContainsKey(file))
                return _Dictionary2[file];
            else
            {
                if (!File.Exists(file)) File.WriteAllText(file, "");
                List<string> ss = File.ReadAllText(file, Encoding.Default).Split("\r\n").ToList();
                _Dictionary2.Add(file, ss);
                return ss;
            }
        }
        
        static void Main(string[] args)
        {
            Spammer3.Beep = false;
            Spammer3.Setup("../../");
            Spammer3.StartRemoteConsoleAsync(5999);            
            ChatBox _IcqChat = new ChatBox();
            _IcqChat.Start();
        }
        
        
        
        public static List<string> _console { get { return Spammer3._console; } }
        public static void SendMessageToAll(string room, string uin, string msg2)
        {
            SendMessageToAll(room, uin, msg2, false);
        }
        public static void SendMessageToAll(string room, string uin, string msg2, bool fromirc)
        {            
            if (!fromirc)
            {
                //irc
                _IrcChat.SendMessage(room, msg2);
            }
            //////////icq
            int i = 0;
            foreach (User _User2 in (from a in _Accounts from u in a._Users where u._room == room && u.uin != uin select u))
            {
                _User2.SendMessage(msg2);
                i++;
            }
            Trace2("Sended msgs: " + i);

            /////////web
            string msg = DateTime.Now + " " + msg2.Replace("\r\n", "\n");
            foreach (User _User in _UserList)
                if (_User._room == room)
                {
                    _User._MsgsToSend.Add(msg);
                    if (_User._MsgsToSend.Count > 50)
                        _User._MsgsToSend.RemoveAt(0);
                }
            GetRoom(room).Add(msg);
            File.AppendAllText(room + ".txt", msg + "\r\n", Encoding.Default);
        }
        public static Database _Database;
        public class IrcChat
        {
            NetworkStream _NetworkStream;
            public void Start()
            {
                TcpClient _TcpClient = new TcpClient("irc.kgts.ru", 6667);
                Socket _Socket = _TcpClient.Client;
                _NetworkStream = new NetworkStream(_Socket);
                Thread _Thread = new Thread(Read);
                _Thread.Start();
                _NetworkStream.WriteLine(Trace2(string.Format("NICK {0}", "ChatBox2")));
                _NetworkStream.WriteLine(Trace2("USER " + "ChatBox2" + " " + "ChatBox2" + " server :" + "cslive"));
                _NetworkStream.WriteLine(Trace2("codepage cp1251"));
            }
            public bool _Connected;
            public void Leave(string room)
            {
                _NetworkStream.WriteLine(Trace2("part #" + prefix + room));
            }
            public void Join(string room)
            {
                _NetworkStream.WriteLine(Trace2("join #" + prefix+room));
            }
            public class IrcIm
            {
                public string room;
                public string msg;
                public string user;
            }
            List<IrcIm> messages = new List<IrcIm>();
            public const string prefix = "cs-";
            public List<IrcIm> GetMessages()
            {
                List<IrcIm> msgs = messages;
                messages = new List<IrcIm>();
                return msgs;
            }
            public void SendMessage(string room, string msg)
            {
                foreach (string s in msg.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    _NetworkStream.WriteLine(String.Format("PRIVMSG #{0} :{1}", prefix + room, s));
            }

            public void Read()
            {
                try
                {
                    while (true)
                    {
                        string s = Trace2(_NetworkStream.ReadLine());
                        Match _Match = Regex.Match(s, @"PING \:(\w+)", RegexOptions.IgnoreCase);
                        if (_Match.Success)
                            _NetworkStream.WriteLine(("PONG :" + Trace2(_Match.Groups[1])));
                        _Match = Regex.Match(s, @":(.+?)!.+? PRIVMSG #(.+?) :(.+)");
                        if (_Match.Success)
                        {
                            messages.Add(new IrcIm
                            {
                                room = _Match.Groups[2].Value.TrimStart(prefix),
                                user = _Match.Groups[1].Value,
                                msg = _Match.Groups[3].Value
                            });

                        }
                        if (Regex.Match(s, @":.+? 005").Success) _Connected = true;
                    }
                }
                catch (IOException) { }
                _Connected = false;
            }
            public static T Trace2<T>(T t)
            {
                using (var fs = File.Create("irc.txt")) fs.WriteLine(t.ToString());
                return t;
            }
        }
        public static IrcChat _IrcChat;

        public class ChatBox
        {
            public void Start()
            {
                if (File.Exists("db.xml"))
                    Load();
                else throw new Exception("Database not Found");

                HttpChat _HttpChat = new HttpChat();
                _HttpChat._ChatBox = this;
                foreach (Database.User u in _Database._Users)
                    _UserList.Add(new User { _DUser = u });
                _HttpChat.StartAsync();
                _IrcChat = new IrcChat();
                _IrcChat.Start();
                foreach (string room in _rooms)
                    _IrcChat.Join(room);
                List<string> dcheck = new List<string>();
                foreach (Database.Icq _Dicq in _Database._Accounts) //load database
                {
                    if (dcheck.Contains(_Dicq.uin)) throw new Exception();
                    dcheck.Add(_Dicq.uin);
                    Icq _Icq = new Icq();
                    _Icq._DIcq = _Dicq;
                    Add(_Icq);
                    Thread.Sleep(500);
                }

                while (true) //Update()
                {
                    
                    foreach (IrcChat.IrcIm msg2 in _IrcChat.GetMessages())
                    {
                        SendMessageToAll(msg2.room, null, msg2.user + "(irc):" + msg2.msg, true);
                    }
                    foreach (Icq _Icq in _Accounts) //////OnMessage
                        foreach (Im _IM in _Icq.Update())
                        {
                            string s = OnMessage(_Icq, _IM.uin, _IM.msg);
                            if (s != null) _Icq.SendMessage(_IM.uin, Trace2(s));
                        }
                    if (STimer.TimeElapsed(8000))
                        using (FileStream _FileStream = new FileStream("db.xml", FileMode.Create, FileAccess.Write))
                            _XmlSerializer.Serialize(_FileStream, _Database);

                    System.Text.StringBuilder sb = new System.Text.StringBuilder(_IrcChat._Connected ? "0" : "1"); //title
                    foreach (Icq icq in _Accounts)
                        sb.Append((byte)icq._ICQAPP._ConnectionStatus);
                    Spammer3._Title = sb.ToString();
                    string msg = _console.FirstOrDefault(); // console read
                    if (msg != null)
                    {
                        ReadConsole(msg).Trace();
                        _console.Remove(msg);
                    }
                    STimer.Update();
                    Thread.Sleep(20);
                }
            }
            private static IEnumerable<User> _OrderedByDateUsers
            {
                get
                {
                    return (from icq in _Accounts
                            from a in icq._Users orderby a._DateTime descending
                            select a).ToList();
                }
            }
            private static IEnumerable<User> _Users
            {
                get
                {
                    return (from icq in _Accounts
                            from a in icq._Users
                            select a).ToList();
                }
            }
            private static IEnumerable<User> GetOldUsers(TimeSpan t)
            {
                return (from a in _UserList where DateTime.Now - a._DateTime > t select a);
            }

            private string ReadConsole(string msg)
            {
                Match m;
                m = Regex.Match(msg, @"^/send (.+)");
                if (m.Success)
                {
                    foreach (string room in _rooms)
                        SendMessageToAll(room,null, m.Groups[1].Value);
                    
                    return "success";
                }
                if ((m = Regex.Match(msg, @"^/remove (\d+)")).Success)
                {
                    foreach (Icq icq in _Accounts.ToArray())
                        if (icq._uin == m.Groups[1].Value)
                        {
                            
                            _Accounts.Remove(icq);                            
                            return "success";
                        }   
                    foreach (Database.Icq icq in _Database._Accounts)
                        if (icq.uin == m.Groups[1].Value)
                        {
                            _Database._Accounts.Remove(icq);
                            return "success";
                        }
                    return "failed";
                }
                if (Regex.Match(msg, @"^/reconnect$").Success)
                {
                    foreach (Icq icq in _Accounts)
                        if (icq._ICQAPP._ConnectionStatus == ConnectionStatus.LoginError)
                        {
                            icq._ICQAPP.StartAsync();
                            Trace2("reconnecting" + icq._uin);
                        }                                                            
                    return "success";
                }                
                if ((m = Regex.Match(msg, @"^/flush (\d+)")).Success)
                {
                    foreach (User user in _OrderedByDateUsers.Take(int.Parse(m.Groups[1].Value)))
                    {
                        _Database._RemovedUsers.Add(user.nick + ";" + user.uin);
                        user.SendMessage(Res.removed);
                        user._Account.Remove(user);
                    }                    
                }
                if (Regex.Match(msg, @"^/flush").Success)
                {
                    foreach (User user in GetOldUsers(TimeSpan.FromDays(3)).ToArray())
                        RemoveUser(user);
                }
                m = Regex.Match(msg, @"^/op (.+)");
                if (m.Success)
                {
                    User user = FindUserbyNick(m.Groups[1].Value);
                    if (user != null && !user.nick.StartsWith("@"))
                    {
                        user.nick = "@" + user.nick;
                        return "success";
                    }
                }
                m = Regex.Match(msg, @"^/register (\d+);(\w+)");
                if (m.Success)
                {                    
                    Add(new Icq { _uin = m.Groups[1].Value, _pass = m.Groups[2].Value });
                    return "success";
                }
                m = Regex.Match(msg, @"^/removeroom (\w+)");
                if (m.Success)
                {
                    string room = m.Groups[1].Value;
                    _rooms.Remove(room);
                    foreach (User ht in GetUserRoom(room))
                        ht._room = "general";
                    _IrcChat.Leave(room);
                    return "success";
                }
                m = Regex.Match(msg, @"^/addroom (\w+)");
                if (m.Success)
                {
                    string room = m.Groups[1].Value;
                    _rooms.Add(room);
                    _IrcChat.Join(room);
                    return "success";
                }
                return ("unknown command");

            }

            private static IEnumerable<User> GetUserRoom(string room)
            {
                return (from a in _Accounts from u in a._Users where u._room == room select u);
            }


            XmlSerializer _XmlSerializer = Helper.CreateSchema("ChatBox2", typeof(Database));

            public void Add(Icq icq)
            {
                _Accounts.AddUnique(icq);
                _Database._Accounts.AddUnique(icq._DIcq);
                icq.StartAsync();
            }

            private void Load()
            {
                //_XmlSerializer.Serialize(File.Create("db.xml"), new Database());
                using (FileStream _FileStream = new FileStream("db.xml", FileMode.Open, FileAccess.Read))
                    _Database = (Database)_XmlSerializer.Deserialize(_FileStream);
            }

            public string OnMessage(Icq _Icq, string uin, string msg) //icqcommands
            {
                msg = msg.Trim();
                Trace2(String.Format(_Icq._uin + " message received from {0}: {1}", uin, msg));
                User _User = FindUserByUin(uin);

                string _ret = GlobalCommands(msg, _User);

                if (_ret != null) return _ret;
                else if (_User == null)
                {
                    if (_Icq._Users.Count >= _Database.limit)
                    {
                        Icq _Icq2 = From();
                        if (_Icq2 == null)
                            return (Res.nouins);
                        else
                            return (string.Format(Res.limitreached,_Icq2._uin));
                    }
                    Match _registernick = Regex.Match(msg, Res.registerMatch);
                    if (_registernick.Success)
                    {
                        string nick = _registernick.Groups[1].Value;
                        if (FindUserbyNick(nick) == null)
                        {
                            _User = new User { uin = uin };
                            _User._room = "general";
                            _User.nick = nick;
                            _User._Account = _Icq;
                            _Icq.Add(_User);
                            return GetLastMessages(_User._room) + Res.youareregistered;
                        }
                        else
                            return (nick + Res.alreadyregistered);
                    }
                    else if (Regex.IsMatch(msg, Res.unknowncommandMatch))
                        return (Res.unknowncommand);
                    else
                        return (Res.welcome);
                }
                else //user found
                {
                    _User._DateTime = DateTime.Now;
                    if (_User._Banned > DateTime.Now) return "you have been banned, please wait " + (_User._Banned - DateTime.Now).ToString().Split('.').First();

                    if (Regex.IsMatch(msg, "^/hideuin$"))
                    {
                        _User._HideUin = true;
                        return "success";
                    }
                    if (Regex.Match(msg, "^/disablenotify").Success)
                    {
                        if (_User._notifydisabled == false)
                        {
                            _User._notifydisabled = true;
                            return "disabled";
                        }
                        else
                        {
                            _User._notifydisabled = false;
                            return "enabled";
                        }
                    }
                    Match setinfo = Regex.Match(msg, Res.setinfoMatch, RegexOptions.Multiline);
                    if (setinfo.Success)
                    {
                        _User._Info = setinfo.Groups[1].Value;
                        return ("Success");
                    }
                    else if (Regex.Match(msg, @"^/exit2$").Success)
                    {
                        throw new Exception("exit");
                    }                    
                    else if (Regex.Match(msg, Res.unknowncommandMatch).Success)
                        return Res.unknowncommand;
                    else
                    {
                        string msg2 = _User.nick + ": " + msg;

                        SendMessageToAll(_User._room, uin, msg2);
                        return null;
                    }
                }
            }





            private Icq From()
            {
                return (from a in _Accounts where a._Users.Count < _Database.limit && a._ICQAPP._ConnectionStatus == ConnectionStatus.Connected select a).FirstOrDefault();
            }

            private User FindUserByUin(string ScreenName)
            {
                User _User = (from account in _Accounts from user in account._Users where user.uin == ScreenName select user).FirstOrDefault();
                return _User;
            }
        }
        public static User FindUserbyNick(string nick)
        {
            User u = (from account in _Accounts from user in account._Users where user.nick == nick select user).FirstOrDefault();
            if (u != null) return u;
            return _UserList.FirstOrDefault(a => a.nick == nick);

        }
        public class Icq
        {


            public override string ToString()
            {
                return _ICQAPP._uin + "->" + _ICQAPP._ConnectionStatus;
            }
            public List<Im> _messages = new List<Im>();

            public Database.Icq _DIcq = new Database.Icq();
            public List<User> _Users = new List<User>();
            public void Add(User _User)
            {
                if (!_Users.Contains(_User)) _Users.Add(_User);
                if (!_DIcq._Users.Contains(_User._DUser)) _DIcq._Users.Add(_User._DUser);
            }
            public void Remove(User _User)
            {
                _Users.Remove(_User);
                _DIcq._Users.Remove(_User._DUser);
            }
            public string _uin { get { return _DIcq.uin; } set { _DIcq.uin = value; } }
            public string _pass { get { return _DIcq.pass; } set { _DIcq.pass = value; } }
            public ICQAPP _ICQAPP;

            bool started;
            public void StartAsync()
            {
                if (started == true) throw new Exception("sd");
                started = true;
                foreach (Database.User _Duser in _DIcq._Users)
                {
                    if (_Duser._room == null)
                        _Duser._room = "general";
                    Add(new User { _DUser = _Duser, _Account = this });
                }
                _ICQAPP = new ICQAPP { _uin = _uin, _passw = _pass };
                _ICQAPP._onMessage += new ICQAPP.Message(ICQAPP__onMessage);
                _ICQAPP._onMessageStatusChanged += new ICQAPP.Message(ICQAPP__onMessageStatusChanged);
                _ICQAPP.StartAsync();
            }

            void ICQAPP__onMessageStatusChanged(Im Im)
            {
                Trace2(_ICQAPP._uin + "->" + Im.uin + ":" + Im.Status);
            }

            void ICQAPP__onMessage(Im Im)
            {
                _ims.Add(Im);
            }

            List<Im> _ims = new List<Im>();

            public void SendMessage(string uin, string msg)
            {
                Im im = _messages.FirstOrDefault(a => a.uin == uin && a.Status == MessageStatus.None);
                if (im == null)
                    _messages.Add(new Im { msg = msg, uin = uin, Status = MessageStatus.None });
                else
                    im.msg += "\r\n" + msg;
            }
            DateTime _DateTime = DateTime.Now;
            public List<Im> Update()
            {
                Im Im = null;
                //try
                //{
                Im = _messages.FirstOrDefault(Func2());
                //}
                //catch { }
                ConnectionStatus c = _ICQAPP._ConnectionStatus;
                if (c == ConnectionStatus.Connected && DateTime.Now - _DateTime > TimeSpan.FromSeconds(5) && Im != null) //interval
                {
                    _DateTime = DateTime.Now;
                    _ICQAPP.SendMessage(Im);
                }

                if (c != ConnectionStatus.Connected && c != ConnectionStatus.Connecting && c != ConnectionStatus.LoginError)
                {
                    _ICQAPP.StartAsync();
                }

                try
                {
                    if (null != _messages.FirstOrDefault(Func1()))
                    {
                        Trace2(_ICQAPP._uin + "Message Sending Failed");
                        foreach (Im im2 in _messages)
                            if (im2.Status == MessageStatus.Sending) im2.Status = MessageStatus.None;

                        _ICQAPP.Disconnect();
                    }
                }
                catch { }
                List<Im> ims = _ims;
                _ims = new List<Im>();
                return ims;
            }

            private static Func<Im, bool> Func2()
            {
                return m => m.Status == MessageStatus.None;
            }

            private static Func<Im, bool> Func1()
            {
                return m => m.Status == MessageStatus.Sending && DateTime.Now - m._DateTime > TimeSpan.FromSeconds(10);
            }
        }
        public class HttpChat
        {
            public void StartAsync()
            {                
                TcpListener _TcpListener = new TcpListener(IPAddress.Any, 5200);
                _TcpListener.Start();
                Thread _Thread = new Thread(delegate()
                {
                    while (true)
                    {
                        Socket _Socket = _TcpListener.AcceptSocket();
                        HttpClient _HttpClient = new HttpClient { _Socket = _Socket };
                        new Thread(_HttpClient.Start).Start();
                    }
                });
                _Thread.IsBackground = true;
                _Thread.Start();
            }
            class HttpClient
            {
                public Socket _Socket;
                string _ip;
                User _User;
                public void Start()
                {
                    _ip = (((IPEndPoint)_Socket.RemoteEndPoint).Address).ToString();
                    _User = _UserList.FirstOrDefault(a => a._ip == _ip); //
                    Trace2("con"+_ip);
                    NetworkStream _NetworkStream = new NetworkStream(_Socket);
                    _NetworkStream.ReadTimeout = 30000;
                    try
                    {
                        while (true)
                        {
                            string s = _Socket.Receive().ToStr();// ; //_NetworkStream.Cut("\r\n\r\n").ToStr();
                            //Match cookie = Regex.Match(s, @"Cookie: .*ses=(\d.+)%3a(.+?)[;\n\r]", RegexOptions.IgnoreCase);
                            if (Regex.Match(s, @"GET /(Default.htm)? HTTP", RegexOptions.IgnoreCase).Success)
                            {
                                string s3 = File.ReadAllText("Default.htm",Encoding.Default);
                                Send(s3);
                                Trace2("Send1");
                            }
                            else 
                            {                                
                                Send(OnMessage1(s));
                            }
                            
                        }
                    }
                    catch (IOException) { }
                    catch (SocketException) { }
                    Trace2("disc");
                }
                public string OnMessage1(string s)
                {
                    Match m;

                    if ((Regex.Match(s, @"GET /\?getlog&r=.+? HTTP", RegexOptions.IgnoreCase)).Success)                                            
                        return GetLastMessages(_User==null?"general":_User._room);
                    
                    if ((m = Regex.Match(s, @"GET /\?send=(.+?)&r=.+? HTTP", RegexOptions.IgnoreCase)).Success) //send msg
                    {
                        return Trace2(OnMessage2(HttpUtility.UrlDecode(m.Groups[1].Value)));
                    }
                    if ((m = Regex.Match(s, @"GET /\?q=(.+?) HTTP", RegexOptions.IgnoreCase)).Success)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder(@"<form action=""/"" method=""get""><input name=""q"" type=""text"" /><input type=""submit"" value=""Search"" /></form>");
                        foreach (string room in _rooms)
                            for (int line = 0; line < GetRoom(room).Count; line++)
                            {
                                string s4 = GetRoom(room)[line];
                                string req = HttpUtility.UrlDecode(m.Groups[1].Value, Encoding.Default);
                                req = Regex.Replace(req, @"\b(?:(?!\w).)+\b", ".*");
                                Match m2 = Regex.Match(s4, req, RegexOptions.IgnoreCase);
                                if (m2.Success)
                                    sb.Append(string.Format("<br /><a href=?line={0}&room={2}>{1}</a>", line, HttpUtility.HtmlEncode(s4),room));
                            }
                        Trace2("search");
                        return (sb.ToString());
                    }
                    if ((m = Regex.Match(s, @"GET /\?line=(\d+)&room=(.+?) HTTP")).Success)
                        if (_rooms.Contains(m.Groups[2].Value))
                        {
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            int i2 = int.Parse(m.Groups[1].Value);
                            for (int i = i2; i < Math.Min(GetRoom(m.Groups[2].Value).Count, i2 + 100); i++)
                            {
                                sb.AppendLine(HttpUtility.HtmlEncode(GetRoom(m.Groups[2].Value)[i]) + "<br />");
                            }
                            Trace2("getlog");
                            return (sb.ToString());
                        }
                    if (_User != null)
                    {
                        if ((Regex.Match(s, @"GET /\?get&r=.+? HTTP", RegexOptions.IgnoreCase)).Success)
                        {
                            StringCollection ss = _MsgsToSend;
                            _MsgsToSend = new StringCollection();
                            return ss.ToString("\r\n");
                        }
                    }
                    return "";
                }

                public StringCollection _MsgsToSend { get { return _User._MsgsToSend; } set { _User._MsgsToSend = value; } }

                private void Send(string text)
                {
                    string s = string.Format(Res.httpsend, text.Length, text);
                    _Socket.Send(s);
                }
                private void Send(string text, string cookie)
                {
                    _Socket.Send(string.Format(Res.httpsendcookie, text.Length, text, cookie));
                }
                private string OnMessage2(string msg)
                {
                    Match m;
                    Trace2("OnMessage");
                    string s2 = GlobalCommands(msg, _User);
                    if (s2 != null) return s2;
                    if (_User == null)
                    {
                        m = Regex.Match(msg, Res.registerMatch);
                        if (m.Success)
                        {
                            string nick = "(web)" + m.Groups[1].Value;
                            if (FindUserbyNick(nick) == null)
                            {
                                AddUser(new User { nick = nick, _ip = _ip, _room = "general"});
                                return Res.youareregistered;
                            }
                            else
                                Send(Res.alreadyregistered);
                        }
                        if (Regex.IsMatch(msg, Res.unknowncommandMatch))
                            return Res.unknowncommand;
                        return Res.welcome;
                    }
                    else
                    {
                        _User._DateTime = DateTime.Now;
                        if (_User._Banned > DateTime.Now) return "banned " + (_User._Banned - DateTime.Now);                        

                        m = Regex.Match(msg, Res.setinfoMatch);
                        if (m.Success)
                        {
                            _User._Info = m.Groups[1].Value;
                            return "success";
                        }
                        if (Regex.IsMatch(msg, Res.unknowncommandMatch))
                            return Res.unknowncommand;

                        //message send
                        msg = _User.nick + ":" + msg;
                        SendMessageToAll(_User._room, null, msg);
                        return "";

                    }

                    throw new Exception("wtf");
                }
            }
            public ChatBox _ChatBox;



            public static T Trace2<T>(T t)
            {
                Helper.Trace2(t.ToString(), "HttpChat");
                return t;
            }
        }
        public static List<User> _UserList = new List<User>();
        public static void AddUser(User u)
        {
            _UserList.Add(u);
            
            _Database._Users.Add(u._DUser);
        }
        public static void RemoveUser(User u)
        {
            _UserList.Remove(u);
            _Database._Users.Remove(u._DUser);
        }
        public static T Trace2<T>(T t)
        {
            Helper.Trace2(t.ToString(),"Chatbox");
            return t;
        }
        public static List<string> _rooms { get { return _Database.rooms; } }
        public static string GlobalCommands(string msg, User _user)
        {
            Match m;
            Match ping = Regex.Match(msg, @"^.?ping(.*)");
            if (ping.Success)
                return "pong " + ping.Groups[1].Value;
            if (Regex.Match(msg, "^.?help$").Success)
                return Res.help;
            
            if ((m = Regex.Match(msg, @"^.?search( .*)")).Success)
                return "http://" + _Database.IpAddress + ":5200/?q=" + HttpUtility.UrlEncode(m.Groups[1].Value.Trim());            
            if (_user != null)
            {
                if ((m = Regex.Match(msg, @"^/mute ?(\d+)")).Success)
                {
                    TimeSpan ts;
                    if (m.Groups[1].Value == "")
                        ts = TimeSpan.FromHours(3);
                    else
                        ts = TimeSpan.FromMinutes(Math.Min(int.Parse(m.Groups[1].Value),180));
                    _user._Mute=DateTime.Now + ts;
                }
                if (Regex.Match(msg, "^/rooms").Success)
                {
                    return _rooms.ToString("\r\n");
                }
                m = Regex.Match(msg, @"^/join (.+)");
                if (m.Success)
                {
                    if (_rooms.Contains(m.Groups[1].Value))
                    {
                        string room = m.Groups[1].Value;
                        _user._room = m.Groups[1].Value;
                        return GetLastMessages(room);
                    }
                    else
                        return "room not found";
                }
                if (_user.nick.StartsWith("@"))//admin
                {                    
                    m = Regex.Match(msg, @"^/ban (.+?) (\d*\.?\d+)$");
                    if (m.Success)
                    {
                        User _User = FindUserbyNick(m.Groups[1].Value);
                        if (_User != null && _User._Banned > DateTime.Now) return "Already Banned for" + (DateTime.Now - _User._Banned);
                        if (_User != null && !_User.nick.StartsWith("@"))
                        {
                            TimeSpan ts = TimeSpan.FromMinutes(float.Parse(m.Groups[2].Value.Replace(".", ",")));
                            _User._Banned = DateTime.Now + ts;
                            SendMessageToAll(_User._room, null, "<<"+_User.nick + " banned for " + ts+">>");
                            return "success";
                        }
                        else
                            return "Failded, user not found";
                    }
                    m = Regex.Match(msg, @"^/unban (.+)");
                    if (m.Success && _user.nick.StartsWith("@"))
                    {
                        User _User = FindUserbyNick(m.Groups[1].Value);
                        if (_User != null)
                        {
                            _User._Banned = DateTime.MinValue;
                            SendMessageToAll(_User._room, null, "<<"+_User.nick + " unbanned>>");
                            return "success";
                        }
                        else
                            return "user not found";
                    }
                }
            }
            if (Regex.Match(msg, "^.?whois$").Success)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("\r\n");
                foreach (User user in GetOrderedIcqUsers())
                    sb.AppendLine(user.nick + (user._HideUin ? "(icq)" : "(" + user.uin + ")") + "," + user._room + ":" + user._Info);
                foreach (User user in GetOrderedUsers())
                    sb.AppendLine(user.nick + "," + user._room + ":" + user._Info);
                return sb.ToString();
            }
            if (Regex.Match(msg, @"^.?register$").Success)
                return Res.register;
            return null;
        }

        private static string GetLastMessages(string room)
        {
             var a =GetRoom(room).Last(500);
             //int b=a.Count();
             //Debugger.Break();
            return a.ToString("\r\n");
        }
        private static IEnumerable<User> GetOrderedUsers()
        {
            return (from a in _UserList orderby a.nick select a);
        }
        private static IEnumerable<User> GetOrderedIcqUsers()
        {
            return (from a in _Accounts from u in a._Users orderby u.nick select u);
        }

    }
    
}
