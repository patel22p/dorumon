using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public class HttpUser
        {

            public virtual string _room { get; set; }
            public virtual string ip { get; set; }
            public virtual string nick { get; set; }
            public virtual string _Info { get; set; }
            public virtual DateTime _DateTime { get; set; }
            public virtual DateTime _Banned { get; set; }
            public StringCollection _MsgsToSend = new StringCollection();
            public HttpUser()
            {
                _DateTime = DateTime.Now;
            }
        }
        public class User : HttpUser
        {
            public User()
                : base()
            {

            }
            public override string _room { get { return _DUser._room; } set { _DUser._room = value; } }
            public override string _Info { get { return _DUser.info; } set { _DUser.info = value; } }
            public Icq _Account;
            public Database.User _DUser = new Database.User();
            public override DateTime _DateTime { get { return _DUser._DateTime; } set { _DUser._DateTime = value; } }
            public string uin { get { return _DUser.uin; } set { _DUser.uin = value; } }
            public override string nick { get { return _DUser.nick; } set { _DUser.nick = value; } }
            public override DateTime _Banned { get { return _DUser._Banned; } set { _DUser._Banned = value; } }
            public bool _HideUin { get { return _DUser._HideUin; } set { _DUser._HideUin = value; } }
            public void SendMessage(string msg)
            {
                _Account.SendMessage(uin, msg);
            }
        }
        public static List<Icq> _Accounts = new List<Icq>();
        public class Database
        {
            public List<string> rooms = new List<string>();// { "general", "dev", "site" };
            public List<string> _RemovedUsers = new List<string>();
            public class User
            {
                public string _room;
                public string info;
                public DateTime _DateTime;
                public string uin;
                public string nick;
                public DateTime _Banned;
                public bool _HideUin;
            }
            public int limit = 5;
            public List<Icq> _Accounts = new List<Icq>();
            public SerializableDictionary<string, HttpUser> _Dictionary = new SerializableDictionary<string, HttpUser>();
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
        public static void AddMessage(string s, string room)
        {
            string msg = DateTime.Now + " " + s;
            foreach (HttpUser _HttpUser in _Dictionary.Values)
                if (_HttpUser._room == room)
                {
                    _HttpUser._MsgsToSend.Add(msg);
                    if (_HttpUser._MsgsToSend.Count > 50)
                        _HttpUser._MsgsToSend.RemoveAt(0);
                }
            GetRoom(room).Add(msg);
            File.AppendAllText(room + ".txt", msg + "\r\n", Encoding.Default);
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
        public static void SendIcqMessages(string room, string uin, string msg2)
        {
            SendIcqMessages(room, uin, msg2, false);
        }
        public static void SendIcqMessages(string room, string uin, string msg2, bool fromirc)
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
            foreach (HttpUser _HttpUser in _Dictionary.Values)
                if (_HttpUser._room == room)
                {
                    _HttpUser._MsgsToSend.Add(msg);
                    if (_HttpUser._MsgsToSend.Count > 50)
                        _HttpUser._MsgsToSend.RemoveAt(0);
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

                HttpChat _ChatHttpServer = new HttpChat();
                _ChatHttpServer._ChatBox = this;
                _ChatHttpServer.StartAsync();
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

                    if (STimer.TimeElapsed(20000))
                    {

                        foreach (User user in GetOldIcqUsers(TimeSpan.FromDays(3)))
                        {
                            _Database._RemovedUsers.Add(user.nick + ";" + user.uin);
                            user.SendMessage(Res.removed);
                            user._Account.Remove(user);
                        }
                        foreach (HttpUser user in GetOldHttpUsers(TimeSpan.FromDays(3)).ToArray())
                            _Dictionary.Remove(user.ip);
                    }
                    foreach (IrcChat.IrcIm msg2 in _IrcChat.GetMessages())
                    {
                        SendIcqMessages(msg2.room, null, msg2.user + "(irc):" + msg2.msg, true);
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

                    StringBuilder sb = new StringBuilder(_IrcChat._Connected?"1":"0"); //title
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

            private static User[] GetOldIcqUsers(TimeSpan t)
            {
                return (from icq in _Accounts
                        from a in icq._Users
                        where DateTime.Now - a._DateTime > t
                        select a
                                         ).ToArray();
            }

            private static IEnumerable<HttpUser> GetOldHttpUsers(TimeSpan t)
            {
                return (from a in _Dictionary.Values where DateTime.Now - a._DateTime > t select a);
            }

            private string ReadConsole(string msg)
            {
                Match m;
                m = Regex.Match(msg, @"^/send (.+)");
                if (m.Success)
                {
                    foreach (Icq icq in _Accounts)
                        foreach (User u in icq._Users)
                            icq.SendMessage(u.uin, m.Groups[1].Value);
                    return "success";
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
                m = Regex.Match(msg, @"^/op (.+)");
                if (m.Success)
                {
                    HttpUser user = FindUserbyNick(m.Groups[1].Value);
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
                    foreach (HttpUser ht in GetUserRoom(room))
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

            public string OnMessage(Icq _Icq, string uin, string msg)
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
                            return (Res.limitreached + _Icq2._uin);
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
                    else if (Regex.Match(msg, Res.unregisterMatch).Success)
                    {
                        _Icq.Remove(_User);
                        return (Res.unregister);
                    }
                    else if (Regex.Match(msg, Res.unknowncommandMatch).Success)
                        return Res.unknowncommand;
                    else
                    {
                        string msg2 = _User.nick + ": " + msg;

                        SendIcqMessages(_User._room, uin, msg2);
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
        public static HttpUser FindUserbyNick(string nick)
        {
            HttpUser u = (from account in _Accounts from user in account._Users where user.nick == nick select user).FirstOrDefault();
            if (u != null) return u;
            return _Dictionary.Values.FirstOrDefault(a => a.nick == nick);

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

                //try
                //{
                    if (null != _messages.FirstOrDefault(Func1()))
                    {
                        Trace2(_ICQAPP._uin + "Message Sending Failed");
                        foreach (Im im2 in _messages)
                            if (im2.Status == MessageStatus.Sending) im2.Status = MessageStatus.None;

                        _ICQAPP.Disconnect();
                    }
                //}
                //catch { }
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
                HttpUser _User;
                public void Start()
                {
                    _ip = (((IPEndPoint)_Socket.RemoteEndPoint).Address).ToString();
                    Trace2("con"+_ip);
                    NetworkStream _NetworkStream = new NetworkStream(_Socket);
                    _NetworkStream.ReadTimeout = 30000;
                    try
                    {
                        while (true)
                        {
                            string s = _NetworkStream.Cut("\r\n\r\n").ToStr();
                            Match cookie = Regex.Match(s, @"Cookie: .*ses=(\d.+)%3a(.+?)[;\n\r]", RegexOptions.IgnoreCase);
                            if (Regex.Match(s, @"GET /(Default.htm)? HTTP", RegexOptions.IgnoreCase).Success)
                            {
                                string s3 = File.ReadAllText("Default.htm",Encoding.Default);
                                Send(s3);
                                Trace2("Send1");
                            }
                            else if (cookie.Success && HttpUtility.UrlDecode(cookie.Groups[2].Value) == ICQAPP.XOR(cookie.Groups[1].Value).ToStr())
                            {
                                _ip = cookie.Groups[1].Value;
                                _User = _Dictionary.TryGetValue(_ip);
                                Send(OnMessage1(s));
                            }
                            else
                            {
                                Send("", "ses=" + HttpUtility.UrlEncode(_ip + ":" + ICQAPP.XOR(_ip).ToStr()));
                                return;
                            }
                        }
                    }
                    catch (IOException) { }
                    catch (SocketException) { }
                    Trace2("disc");
                }
                public string OnMessage1(string s)
                {
                    Match getlog = Regex.Match(s, @"GET /\?line=(\d+)");
                    Match get = Regex.Match(s, @"GET /\?get&r=.+? HTTP", RegexOptions.IgnoreCase);
                    Match PostMessage = Regex.Match(s, @"GET /\?send=(.+?)&r=.+? HTTP", RegexOptions.IgnoreCase);

                    Match search = Regex.Match(s, @"GET /\?q=(.+?) HTTP", RegexOptions.IgnoreCase);

                    if (PostMessage.Success) //send msg
                    {
                        return Trace2(OnMessage2(HttpUtility.UrlDecode(PostMessage.Groups[1].Value)));
                    }
                    if (_User != null)
                    {
                        if (search.Success)
                        {
                            StringBuilder sb = new StringBuilder(@"<form action=""/"" method=""get""><input name=""q"" type=""text"" /><input type=""submit"" value=""Search"" /></form>");
                            for (int line = 0; line < GetRoom(_User._room).Count; line++)
                            {
                                string s4 = GetRoom(_User._room)[line];
                                string req = HttpUtility.UrlDecode(search.Groups[1].Value, Encoding.Default);
                                req = Regex.Replace(req, @"\b(?:(?!\w).)+\b", ".*");
                                Match m = Regex.Match(s4, req, RegexOptions.IgnoreCase);
                                if (m.Success)
                                    sb.Append(string.Format("<br /><a href=?line={0}>{1}</a>", line, HttpUtility.HtmlEncode(s4)));
                            }
                            Trace2("search");
                            return (sb.ToString());
                        }
                        if (getlog.Success)
                        {
                            StringBuilder sb = new StringBuilder();
                            int i2 = int.Parse(getlog.Groups[1].Value);
                            for (int i = i2; i < Math.Min(GetRoom(_User._room).Count, i2 + 100); i++)
                            {
                                sb.AppendLine(HttpUtility.HtmlEncode(GetRoom(_User._room)[i]) + "<br />");
                            }
                            Trace2("getlog");
                            return (sb.ToString());
                        }
                        if (get.Success)
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
                    _Socket.Send(string.Format(Res.httpsend, text.Length, text));
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
                                _Dictionary.Add(_ip, new HttpUser { nick = nick, ip = _ip, _room = "general" });
                                return GetLastMessages("general") + Res.youareregistered;
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

                        if (Regex.IsMatch(msg, Res.unregisterMatch))
                        {
                            _Dictionary.Remove(_ip);
                            return Res.unregister;
                        }

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
                        SendIcqMessages(_User._room, null, msg);
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
        public static SerializableDictionary<string, HttpUser> _Dictionary { get { return _Database._Dictionary; } }
        public static T Trace2<T>(T t)
        {
            Helper.Trace2(t.ToString(),"Chatbox");
            return t;
        }
        public static List<string> _rooms { get { return _Database.rooms; } }
        public static string GlobalCommands(string msg, HttpUser _user)
        {
            Match m;
            Match ping = Regex.Match(msg, @"^.?ping(.*)");
            if (ping.Success)
                return "pong " + ping.Groups[1].Value;
            if (Regex.Match(msg, "^.?help$").Success)
                return Res.help;
            if (_user != null)
            {
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
                        HttpUser _HttpUser = FindUserbyNick(m.Groups[1].Value);
                        if (_HttpUser != null && _HttpUser._Banned > DateTime.Now) return "Already Banned for" + (DateTime.Now - _HttpUser._Banned);
                        if (_HttpUser != null && !_HttpUser.nick.StartsWith("@"))
                        {
                            _HttpUser._Banned = DateTime.Now + TimeSpan.FromMinutes(float.Parse(m.Groups[2].Value.Replace(".", ",")));
                            return "success";
                        }
                        else
                            return "Failded, user not found";
                    }
                    m = Regex.Match(msg, @"^/unban (.+)");
                    if (m.Success && _user.nick.StartsWith("@"))
                    {
                        HttpUser _HttpUser = FindUserbyNick(m.Groups[1].Value);
                        _HttpUser._Banned = DateTime.MinValue;
                    }
                }
            }
            if (Regex.Match(msg, "^.?whois$").Success)
            {
                StringBuilder sb = new StringBuilder("\r\n");
                foreach (User user in GetOrderedIcqUsers())
                    sb.AppendLine(user.nick + (user._HideUin ? "(icq)" : "(" + user.uin + ")") + "," + user._room + ":" + user._Info);
                foreach (HttpUser user in GetOrderedHttpUsers())
                    sb.AppendLine(user.nick + "," + user._room + ":" + user._Info);
                return sb.ToString();
            }
            if (Regex.Match(msg, @"^.?register$").Success)
                return Res.register;
            return null;
        }

        private static string GetLastMessages(string room)
        {
            return GetRoom(room).Skip(Math.Min(0, GetRoom(room).Count - 1000)).ToString("\r\n");
        }
        private static IEnumerable<HttpUser> GetOrderedHttpUsers()
        {
            return (from a in _Dictionary.Values orderby a.nick select a);
        }
        private static IEnumerable<User> GetOrderedIcqUsers()
        {
            return (from a in _Accounts from u in a._Users orderby u.nick select u);
        }

    }
    
}
