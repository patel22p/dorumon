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

namespace ChatBox2
{
    public class Program
    {        
        public class HttpUser
        {
            public virtual string nick { get; set; }
            public virtual string _Info { get; set; }            
            public virtual DateTime _DateTime { get; set; }
            public virtual DateTime _Banned { get; set; }
            public HttpUser()
            {
                _DateTime = DateTime.Now;
            }
        }
        public class User:HttpUser
        {
            public override string _Info { get { return _DUser.info; } set { _DUser.info = value; } }
            public Icq _Account;
            public Database.User _DUser = new Database.User();
            public override DateTime _DateTime { get { return _DUser._DateTime; } set { _DUser._DateTime = value; } }
            public string uin { get { return _DUser.uin; } set { _DUser.uin = value; } }
            public override string nick { get { return _DUser.nick; } set { _DUser.nick = value; } }
            public override DateTime _Banned { get { return _DUser._Banned; } set { _DUser._Banned = value; } }
        }
        public static List<Icq> _Accounts = new List<Icq>();            
        public class Database
        {
            public class User
            {
                public string info;
                public DateTime _DateTime;
                public string uin;
                public string nick;
                public DateTime _Banned;
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
        public static void AddMessage(string s)
        {
            string msg = DateTime.Now + " " + s;
            _list.Add(msg);
            File.AppendAllText("history.txt", msg + "\r\n",Encoding.Default);
        }        
        static void Main(string[] args)
        {
            Spammer3.Beep = false;
            Spammer3.Setup("../../");
            new Thread(ReadConsole).Start();
            IcqChat _IcqChat = new IcqChat();
            _IcqChat.Start();
        }
        public static List<string> _console = new List<string>();
        private static void ReadConsole()
        {
            while (true)
            {
                _console.Add(Console.ReadLine());
            }
        }
        public static List<string> _list;
        public static Database _Database; 
        public class IcqChat
        {                                                
            public void Start()
            {
                _list = File.ReadAllText("history.txt", Encoding.Default).Split("\r\n").ToList();
                HttpChat _ChatHttpServer = new HttpChat();
                _ChatHttpServer._ChatBox = this;
                _ChatHttpServer.StartAsync();

                if (File.Exists("db.xml"))
                    Load();
                else throw new Exception("Database not Found");

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

                    foreach (Icq _Icq in _Accounts)
                        foreach (Im _IM in _Icq.Update())
                        {
                            string s = Trace2(OnMessage(_Icq, _IM.uin, _IM.msg));
                            if (s != null) _Icq.SendMessage(_IM.uin, s);
                        }
                    if (STimer.TimeElapsed(8000))
                        using (FileStream _FileStream = new FileStream("db.xml", FileMode.Create, FileAccess.Write))
                            _XmlSerializer.Serialize(_FileStream, _Database);

                    StringBuilder sb = new StringBuilder(); //title
                    foreach (Icq icq in _Accounts)
                        sb.Append((byte)icq._ICQAPP._ConnectionStatus);
                    Spammer3._Title = sb.ToString();
                    string msg = _console.FirstOrDefault(); // console read
                    if (msg != null)
                    {
                        Console.WriteLine(ReadConsole2(msg));
                        _console.Remove(msg);
                    }
                    STimer.Update();
                    Thread.Sleep(20);
                }
            }

            private string ReadConsole2(string msg)
            {
                Match m;                                
                m= Regex.Match(msg, @"^/send (.+)");
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
                m= Regex.Match(msg, @"^/op (.+)");
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
                return ("unknown command");

            }
            
                                                                        
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Database));
            
            public void Add(Icq icq)
            {
                _Accounts.AddUnique(icq);
                _Database._Accounts.AddUnique(icq._DIcq);
                icq.StartAsync();
            }

            private void Load()
            {

                using (FileStream _FileStream = new FileStream("db.xml", FileMode.Open, FileAccess.Read))
                    _Database = (Database)_XmlSerializer.Deserialize(_FileStream);
            }
            
            public string OnMessage(Icq _Icq, string uin, string msg)
            {
                msg = msg.Trim();
                Trace2(String.Format(_Icq._uin + " message received from {0}: {1}", uin, msg));
                User _User = FindUserByUin(uin);
                
                string _ret = GlobalCommands(msg,_User);
                    
                if (_ret != null) return _ret;
                else if (_User == null)
                {
                    Match _registernick = Regex.Match(msg, Res.registerMatch);
                    if (_registernick.Success)
                    {
                        if (_Icq._Users.Count >= _Database.limit)
                        {
                            Icq _Icq2 = From();
                            if (_Icq2 == null)                            
                                return (Res.nouins);                            
                            else                            
                                return (Res.limitreached + _Icq2._uin);                                                            
                        }
                        else
                        {
                            string nick = _registernick.Groups[1].Value;
                            if (FindUserbyNick(nick) == null)
                            {
                                _User = new User { uin = uin };
                                _User.nick = nick;
                                _User._Account = _Icq;                                
                                _Icq.Add(_User);
                                return (_User.uin + Res.youareregistered);
                            }
                            else                            
                                return (nick + Res.alreadyregistered);                            
                        }
                    }
                    else if (Regex.IsMatch(msg, Res.unknowncommandMatch))                    
                        return (Res.unknowncommand);                    
                    else                    
                        return (Res.welcome);                    
                }
                else
                {
                    _User._DateTime = DateTime.Now;
                    if (_User._Banned > DateTime.Now) return "you have been banned, please wait " + (_User._Banned - DateTime.Now).ToString().Split('.').First();
                    
                    Match setinfo = Regex.Match(msg, Res.setinfoMatch , RegexOptions.Multiline);
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
                        string msg2 = (_User.nick.Length != 0 ? _User.nick : uin) + ": " + msg;
                        AddMessage(msg2.Replace("\r\n", "\n"));

                        int i = 0;
                        foreach (Icq account in _Accounts)
                        {
                            foreach (User _User2 in account._Users)
                                if (_User2.uin != uin)
                                {
                                    account.SendMessage(_User2.uin, msg2);
                                    i++;
                                }
                        }
                        Trace2("Sended msgs: " + i);
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
            public List<Im> _msgstosend = new List<Im>();

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
                Im im = _msgstosend.FirstOrDefault(a => a.uin == uin && a.Status == MessageStatus.None);
                if (im == null)
                    _msgstosend.Add(new Im { msg = msg, uin = uin, Status = MessageStatus.None });
                else 
                    im.msg += "\r\n" + msg;
            }
            DateTime _DateTime = DateTime.Now;
            public List<Im> Update()
            {

                Im Im = _msgstosend.FirstOrDefault(Func2());
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


                if (null != _msgstosend.FirstOrDefault(Func1()))
                {
                    Trace2(_ICQAPP._uin+"Message Sending Failed");
                    foreach (Im im2 in _msgstosend)
                        if (im2.Status == MessageStatus.Sending) im2.Status = MessageStatus.None;

                    _ICQAPP.Disconnect();
                }
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
                public void Start()
                {
                    Trace2("con");                    
                    NetworkStream _NetworkStream = new NetworkStream(_Socket);
                    try
                    {
                        while (true)
                        {
                            string s = _NetworkStream.Cut("\r\n\r\n").ToStr();
                            Match getlog = Regex.Match(s, @"GET /\?line=(\d+)");
                            Match gethistory = Regex.Match(s, @"GET /\?pos=(\d+)", RegexOptions.IgnoreCase);
                            Match PostMessage = Regex.Match(s, @"GET /\?send=(.+?)&r=.+? HTTP", RegexOptions.IgnoreCase);
                            Match search = Regex.Match(s, @"GET /\?q=(.+?) HTTP", RegexOptions.IgnoreCase);
                            if (search.Success)
                            {
                                StringBuilder sb = new StringBuilder(@"<form action=""/"" method=""get""><input name=""q"" type=""text"" /><input type=""submit"" value=""Search"" /></form>");
                                for (int line = 0; line < _list.Count; line++)
                                {
                                    string s4 = _list[line];
                                    Match m = Regex.Match(s4, HttpUtility.UrlDecode(search.Groups[1].Value,Encoding.Default), RegexOptions.IgnoreCase);
                                    if (m.Success)
                                        sb.Append(string.Format("<br /><a href=?line={0}>{1}</a>", line, HttpUtility.HtmlEncode(s4)));
                                }
                                Trace2("search");
                                Send(sb.ToString());
                            }
                            else if (getlog.Success)
                            {
                                StringBuilder sb = new StringBuilder();
                                int i2 = int.Parse(getlog.Groups[1].Value);
                                for (int i = i2; i < Math.Min(_list.Count, i2 + 100); i++)
                                {
                                    sb.AppendLine(HttpUtility.HtmlEncode(_list[i]) + "<br />");
                                }
                                Send( sb.ToString());
                                Trace2("getlog");
                            }
                            else if (Regex.Match(s, @"GET /(Default.htm)? HTTP", RegexOptions.IgnoreCase).Success)
                            {
                                string s3 = File.ReadAllText("Default.htm");
                                _Socket.Send(string.Format(Res.httpsend, s3.Length, s3));
                                Trace2("Send1");
                            }
                            else if (PostMessage.Success) //send msg
                            {
                                Send(Trace2(OnMessage(HttpUtility.UrlDecode(PostMessage.Groups[1].Value))));
                            }
                            else if (gethistory.Success)
                            {

                                int i = int.Parse(gethistory.Groups[1].Value);
                                int oldi = i;
                                if (i == 0) i = Math.Max(0, _list.Count - 100);
                                StringBuilder sb = new StringBuilder();
                                for (; i < _list.Count; i++)
                                {
                                    sb.AppendLine(_list[i]);
                                }
                                //if (sb.Length != 0) Debugger.Break();
                                Send( String.Format("{0:D5}", i) + sb.ToString());
                                Trace2("Send");
                            }
                            else
                            {
                                Trace2("Send not found");
                                _Socket.Send(Res.httpnotfound);
                            }
                        }
                    }
                    catch (IOException) { }
                    catch (SocketException) { }
                    Trace2("disc");
                }
                private void Send(string text)
                {
                    _Socket.Send(Encoding.Default.GetBytes(string.Format(Res.httpsend, text.Length, text)));
                }
                private string OnMessage(string msg)
                {
                    Match m;
                    Trace2("OnMessage");                    
                    string ip = (((IPEndPoint)_Socket.RemoteEndPoint).Address).ToString();
                    HttpUser _User = _Dictionary.TryGetValue(ip);
                    
                    string s2 = GlobalCommands(msg,_User);
                    if (s2 != null) return s2;
                    if (_User == null)
                    {
                        m = Regex.Match(msg, Res.registerMatch);
                        if (m.Success)
                        {
                            string nick = "(web)" + m.Groups[1].Value;
                            if (FindUserbyNick(nick) == null)
                            {
                                _Dictionary.Add(ip, new HttpUser { nick = nick });
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
                        if (_User._Banned > DateTime.Now) return "banned";

                        if (Regex.IsMatch(msg, Res.unregisterMatch))
                        {
                            _Dictionary.Remove(ip);
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
                        
                        {   //message send
                            msg = _User.nick + ":" + msg;
                            foreach (Program.Icq icq in _Accounts)
                                foreach (Program.User user in icq._Users)                                
                                    icq.SendMessage(user.uin, msg);                                
                            AddMessage(msg);
                            return "";
                        }
                    }
                    
                    throw new Exception("wtf");
                }
            }
            public IcqChat _ChatBox;

            
            
            public static T Trace2<T>(T t)
            {
                //Trace.WriteLine("webchat:"+t);
                return t;
            }
        }
        public static SerializableDictionary<string, HttpUser> _Dictionary { get { return _Database._Dictionary; } }
        public static T Trace2<T>(T t)
        {
            Trace.WriteLine("Chatbox: " + t);
            return t;
        }
        public static string GlobalCommands(string msg, HttpUser _user)
        {
            Match ping = Regex.Match(msg, @"^.?ping(.*)");
            if (ping.Success)            
                return Trace2("pong " + ping.Groups[1].Value);            
            if (Regex.Match(msg, "^.?help$").Success)            
                return Res.help;
            Match m = Regex.Match(msg, @"^/ban (.+?) (\d*\.?\d+)$");
            if (m.Success)
            {
                HttpUser _HttpUser  = FindUserbyNick(m.Groups[1].Value);
                if (_HttpUser != null)
                {
                    _HttpUser._Banned = DateTime.Now + TimeSpan.FromMinutes(float.Parse(m.Groups[2].Value.Replace(".", ",")));
                    return "success";
                }
                else                
                    return "Failed";                
            }
            if (Regex.Match(msg, "^.?whois$").Success)
            {
                StringBuilder sb = new StringBuilder("\r\n");
                foreach (Icq ac in _Accounts)
                    foreach (User user in ac._Users)
                        sb.AppendLine("(" + user.uin + ")" + user.nick + ":" + user._Info);
                foreach (HttpUser user in _Dictionary.Values)
                    sb.AppendLine(user.nick + ":" + user._Info);
                    return Trace2(sb.ToString());
            }
            if (Regex.Match(msg, @"^.?register$").Success)            
                return Trace2(Res.register);            
            return null;
        }
        
    }
    
}
