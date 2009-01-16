using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
namespace Irc
{
    class Program
    {
        static void Main(string[] args)
        {
            Spammer3.Setup();
            Program _Program = new Program();
            _Program.Start();
        }
        public static Random _Random = new Random();
        public Program()
        {
            if (File.Exists(host + "nicks.txt")) _nicklist = File.ReadAllLines(host + "nicks.txt").ToList();
            if (File.Exists(host + "nicksused.txt")) _nicksused = File.ReadAllLines(host + "nicksused.txt").ToList();
        }
        NetworkStream _NetworkStream;
        private void Client()
        {
            NetworkStream _NetworkStream = Connect();
            new Thread(delegate()
            {
                while (true)
                {
                    _NetworkStream.WriteLine(Console.ReadLine());
                }
            }).Start();
            while (true)
                _NetworkStream.ReadLine().Trace();
        }
        public void Start()
        {            
            string text = File.ReadAllText("IRC_servers.ini");
            MatchCollection _MatchCollection = Regex.Matches(text, @"SERVER:([\w\.-]+):(\d+)");
            List<string> serversused = new List<string>();
            if (File.Exists("serversused.txt"))  serversused= File.ReadAllLines("serversused.txt").ToList();
            foreach (Match _Match in _MatchCollection)
            {
                if (!serversused.Contains(_Match.Groups[1].Value))
                {
                    //Client();
                    serversused.Add(_Match.Groups[1].Value);
                    File.WriteAllLines("serversused.txt", serversused.ToArray());
                    try
                    {
                        host = _Match.Groups[1].Value;
                        port = int.Parse(_Match.Groups[2].Value);
                        _NetworkStream = Connect();
                        PopulateChannels();
                        Trace.WriteLine("<<<<<<<<<<<<<<<<<<<Channels Populated>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        Thread.Sleep(1000);
                        PopulateNicks();
                        Trace.WriteLine("<<<<<<<<<<<<<<<<<<<Nicks Populated>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        Thread.Sleep(1000);
                        SendPm();
                        Trace.WriteLine("<<<<<<<<<<<<<<<<<<<Sending Sended>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    }
                    catch (IOException e) { e.Trace(); }
                    catch (ExceptionB e) { e.Message.Trace(); }
                    catch (SocketException e) { (host + e.Message).Trace(); }
                    Thread.Sleep(1000);
                    if(_Socket!=null) _Socket.Close();
                }
            }            

        }
        
        public string[] _Tags = File.ReadAllLines("../tags.txt");
        string host = "irc.ablenet.org";
        public int port = 6667;
        Socket _Socket;
        private NetworkStream Connect()
        {            
            //Socket _Socket = Proxy.Socks5Connect("localhost", 9050, "irc.tambov.ru", 7770);
            _Socket = new TcpClient(host, port).Client;            
            _NetworkStream = new NetworkStream(_Socket);
            _NetworkStream.ReadTimeout = 10000;
            _NetworkStream.WriteLine(string.Format("NICK {0}", _Tags.Random())).Trace();
            _NetworkStream.WriteLine("USER " + _Tags.Random() + " " + _Tags.Random() + " server :" + _Tags.Random()).Trace();
            bool success = false;
            Thread _Thread = new Thread(delegate()
                {
                    try
                    {
                        while (true)
                        {
                            string s = _NetworkStream.ReadLine().Trace();
                            Match _Match = Regex.Match(s, @"PING \:(\w+)", RegexOptions.IgnoreCase);

                            if (_Match.Success)
                            {
                                _NetworkStream.WriteLine(("PONG :" + _Match.Groups[1]).Trace());
                            }
                            if (Regex.Match(s, @":.+? 005").Success) success = true;
                        }
                    }
                    catch (IOException) { }
                });
            _Thread.Start();
            _Thread.Join(5000);
            _Thread.Abort();
            if (!success) throw new ExceptionB("cannot connect to: " +host);
            _NetworkStream.WriteLine("codepage cp1251");
            Trace.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<Connected>>>>>>>>>>>>>>>>>>>>>>>>");
            return new NetworkStream(_Socket);
        }
        List<string> _nicksused = new List<string>();
        private void SendPm()
        {            
            
            foreach (string _nick in _nicklist)
            {
                if (!_nicksused.Contains(_nick))
                {                                        
                    _NetworkStream.WriteLine(("PRIVMSG " + _nick + " "+message).Trace());
                    _nicksused.Add(_nick);
                    File.WriteAllLines(host + "nicksused.txt", _nicksused.ToArray());
                    Thread.Sleep(3000);
                    while(_Socket.Available > 0)
                    {
                        string s = _NetworkStream.ReadLine().Trace();
                        if (Regex.Match(s, @"\:.+? 439").Success)
                        {
                            Connect();
                        }
                    }
                }
            }            
        }
        public const string message = @"http://code.google.com/p/counterstrike/ Silverlight multiplayer game join us!";
        List<String> _nicklist = new List<string>();
        private void PopulateNicks()
        {            
            if (File.Exists(host + "_PostedChannels.txt")) _PostedChannels = File.ReadAllLines(host + "_PostedChannels.txt").ToList();
            string s = File.ReadAllText(host + "channel.txt", Encoding.Default);

            List<Asd> sortlist = new List<Asd>();

            foreach (Match m in Regex.Matches(s, @"(.+?) (\d+)"))
            {
                sortlist.Add(new Asd { a = int.Parse(m.Groups[2].Value), b = m.Groups[1].Value });
            }
            List<string> channels = (from a in sortlist orderby a.a descending select a.b).Take(5).ToList();
            string.Join(",", channels.ToArray()).Trace();

            foreach (string s1 in channels)
            {
                if (!_PostedChannels.Contains(s1))
                {
                    Thread.Sleep(7000);
                    _NetworkStream.WriteLine(("JOIN #" + s1).Trace());
                    _NetworkStream.WriteLine("PART #" + s1 + " : "+message);
                    while (true)
                    {
                        string s4 = _NetworkStream.ReadLine().Trace();
                        Match _Match = Regex.Match(s4, @"\:.+? 353 .+? = #(.+?) \:(.+)");
                        if (_Match.Success)
                        {
                            _PostedChannels.Add(_Match.Groups[1].Value);
                            foreach (string s2 in _Match.Groups[2].Value.Trim().Split(' '))
                            {
                                string s3 = s2.Trim('+', '%', '@','&');
                                if (!_nicklist.Contains(s3) && s3 != "ChanServ")
                                {
                                    _nicklist.Add(s3);
                                    Trace.WriteLine("Added: " + s3);
                                }

                            }
                            File.WriteAllLines(host + "nicks.txt", _nicklist.ToArray());
                            break;
                        }
                    }
                    File.WriteAllLines(host + "_PostedChannels.txt", _PostedChannels.ToArray());
                }
            }
            
        }
        public List<string> _PostedChannels = new List<string>();

        class Asd
        {
            public int a;
            public string b;
        }
        private void PopulateChannels()
        {            
            _NetworkStream.WriteLine("list");
            List<string> list = new List<string>();
            if (File.Exists(host + "channel.txt")) list = File.ReadAllLines(host + "channel.txt").ToList();

            while (true)
            {
                string s = _NetworkStream.ReadLine().Trace();
                {
                    Match _Match = Regex.Match(s, @"\:.+? 322 .+? #(.+?) (\d+?)");
                    if (_Match.Success)
                    {
                        if (!list.Contains(s)) list.Add((_Match.Groups[1].Value + " " + _Match.Groups[2].Value).Trace());
                    }
                }
                if (Regex.Match(s, @"\:.+? 323").Success)
                    break;
            }

            File.WriteAllLines(host + "channel.txt", list.ToArray());            
        }


    }
}