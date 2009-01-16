using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using doru;
using System.Net;

namespace ProxyPinger
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {            
            Console.Title = "Pinger";
            Directory.SetCurrentDirectory(Path.GetFullPath("../../../"));
            const int c = 10;
            ThreadPool.SetMaxThreads(c, c);
            ThreadPool.SetMinThreads(c, c);
            new Program();
        }

        private static void NewMethod1()
        {
            //4,1,0,80,0,0,0,1,77,79,90,0,105,103,111,114,108,101,118,111,99,104,107,105,110,46,105,103,46,102,117,110,112,105,99,46,111,114,103,0,
            TcpRedirect _TcpRedirect = new TcpRedirect();
            _TcpRedirect._LocalPort = 9051;
            _TcpRedirect._RemoteIpDelegate += delegate { return new TcpClient("localhost", 9050).Client; };
            _TcpRedirect.Start();
            Thread.Sleep(-1);            
        }        
        private static void NewMethod()
        {
            TcpListener _TcpListener = new TcpListener(IPAddress.Any, 9051);
            _TcpListener.Start();
            Socket _Socket = _TcpListener.AcceptSocket();
            byte[] _bytes = _Socket.Receive();            
            foreach (byte b in _bytes) Trace.Write(b + ",");
            Debugger.Break();
        }
        public class Ip
        {
            public override string ToString()
            {
                return ip + ":" + port;
            }
            public string ip;
            public int port;
        }

        public static GetProxies _GetProxies;
        const string _filename = "proxy.txt";
        public Program()
        {
            if (File.Exists(_filename))
            {
                string s = File.ReadAllText(_filename);
                foreach (Match _Match in Regex.Matches(s, @"(.+)\:(.+)"))
                {
                    _List.Add(new Ip { ip = _Match.Groups[1].Value, port = int.Parse(_Match.Groups[2].Value) });
                }
            }
            new Thread(Start).Start();
            _GetProxies = new GetProxies();            
            new CheckProxies().StartAsync();
            _GetProxies.StartClipboardCheck();

        }
        public void Start()
        {
            while (true)
            {
                Thread.Sleep(500);
                lock ("ping")
                    if (count != _List.Count)
                    {
                        count = _List.Count;
                        using (FileStream _FileStream = new FileStream(_filename, FileMode.Create, FileAccess.Write))
                            foreach (Ip _ip in _List)
                            {
                                _FileStream.Write(_ip.ip + ":" + _ip.port + "\r\n");
                            }
                    }

            }
        }
        int count;
        public class GetProxies
        {
            public Stack<Ip> _Ips = new Stack<Ip>();
            public void StartAsync()
            {                
                new Thread(StartClipboardCheck).Start();
            }
            string oldclip;
            public void StartClipboardCheck()
            {
                while (true)
                {
                    string s = Clipboard.GetText();                    
                    if (s != oldclip && s.Length > 0)
                    {                        
                        oldclip = s;
                        MatchCollection _Matches = Regex.Matches(s, @"\b((?:[0-9]{1,3}\.){3}[0-9]{1,3})\b[\s:]+(\d+)");
                        Trace.WriteLine("<<<<<<ClipBoard Received>>>>>>>>" + _Matches.Count + " Matches");                        
                        foreach (Match _Match in _Matches)
                        {
                            Ip _Ip = new Ip();
                            _Ip.ip = _Match.Groups[1].Value;
                            _Ip.port = int.Parse(_Match.Groups[2].Value);
                            if((from ip in _Ips where ip.ip == _Ip.ip && ip.port == _Ip.port select ip).FirstOrDefault()==null &&
                                (from ip in _List where ip.ip == _Ip.ip && ip.port == _Ip.port select ip).FirstOrDefault() == null)
                                _Ips.Push(_Ip);
                        }
                    }
                    Thread.Sleep(100);
                }
            }

        }
        public static List<Ip> _List = new List<Ip>();
        public class CheckProxies
        {
            public void StartAsync()
            {
                new Thread(Start).Start();
            }
            public void Start()
            {
                while (true)
                {
                    if (_GetProxies._Ips.Count > 0)
                    {
                        Ip _Ip = _GetProxies._Ips.Pop();
                        ThreadPool.QueueUserWorkItem(ConnectHttp, _Ip);
                    }
                    Thread.Sleep(10);
                }
            }
            static string _localip = "85.157.182.183";
            private static void ConnectSocks4(object o)
            {
                Ip _Ip = (Ip)o;
                try
                {                    
                    TcpClient _TcpClient = new TcpClient(_Ip.ip, _Ip.port);
                    Socket _Socket = _TcpClient.Client;
                    using (MemoryStream _MemoryStream = new MemoryStream())
                    {
                        BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                        _BinaryWriter.Write((byte)0x04);
                        _BinaryWriter.Write((byte)0x01);
                        _BinaryWriter.Write(new byte[]{0,80});
                        _BinaryWriter.Write((int)Dns.GetHostEntry("igorlevochkin.ig.funpic.org").AddressList[0].Address);
                        _BinaryWriter.Write((byte)0);                                                
                        _Socket.Send(_MemoryStream.ToArray());
                    }
                    //_Socket.Send(new byte[] { 4, 1, 0, 80, 0, 0, 0, 1, 77, 79, 90, 0 });
                    byte[] _bytes = _Socket.Receive();
                    Trace.WriteLine("SOCKS>>>>" + _Ip + ":" + _bytes[1].ToHex());
                    _TcpClient.Close();
                }
                catch (SocketException e) { Trace.WriteLine("SOCKS>>>>" + _Ip + ":" + e.Message); }
            }
            private static void ConnectSocks4a(object o)
            {
                Ip _Ip = (Ip)o;
                try
                {                    
                    TcpClient _TcpClient = new TcpClient(_Ip.ip, _Ip.port);
                    Socket _Socket = _TcpClient.Client;
                    using (MemoryStream _MemoryStream = new MemoryStream())
                    {
                        BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                        _BinaryWriter.Write((byte)0x04);
                        _BinaryWriter.Write((byte)0x01);
                        _BinaryWriter.Write(new byte[] { 0, 80 });
                        _BinaryWriter.Write(new byte[] { 0, 0, 0, 3 });
                        _BinaryWriter.Write("MAZ\0".ToBytes());
                        _BinaryWriter.Write("igorlevochkin.ig.funpic.org\0".ToBytes());
                        _Socket.Send(_MemoryStream.ToArray());
                    }
                    byte[] _bytes = _Socket.Receive();
                    if (_bytes[1] == 0x5a) PingSuccess(_Ip);
                    _TcpClient.Close();
                }
                catch (SocketException e) { Trace.WriteLine(_Ip + ":" + e.Message); }
                //Debugger.Break();            
            }
            private static void ConnectSocks5(object o)
            {
                Ip _Ip = (Ip)o;
                TcpClient _TcpClient = new TcpClient(_Ip.ip, _Ip.port);
                Socket _Socket = _TcpClient.Client;
                _Socket.Send(new byte[]{0x05,0x01,0x00});
                byte[] _bytes = _Socket.Receive();
                if (_bytes[0] == 0x05 && _bytes[1] == 0x01)
                    PingSuccess(_Ip);
            }

            private static void ConnectHttp(object o)
            {
                Ip _Ip = (Ip)o;
                Trace.WriteLine("Ping:" + _Ip);
                DateTime _DateTime = DateTime.Now;
                TcpClient _TcpClient=null;
                try
                {
                    _TcpClient = new TcpClient(_Ip.ip, _Ip.port);
                    string s = @"GET http://igorlevochkin.ig.funpic.org/proxy.php HTTP/1.0
Host: igorlevochkin.ig.funpic.org
Proxy-Connection: Keep-Alive

";
                    double ping = (DateTime.Now - _DateTime).TotalMilliseconds;
                    Socket _Socket = _TcpClient.Client;
                    _Socket.Send(ASCIIEncoding.ASCII.GetBytes(s));

                    byte[] _bytes = Http.ReadHttp(new NetworkStream(_Socket));
                    Trace.WriteLine("<<<<<<<<<<<<<<<<<<<<"+_Ip+">>>>>>>>>>>>>>>>>>>>>>>>>");                        
                    Trace.WriteLine(_bytes.ToStr());
                    string s2 = ASCIIEncoding.ASCII.GetString(_bytes);
                    if (!Regex.IsMatch(s2, _localip) && Regex.IsMatch(s2,"_success_")) 
                    {
                        PingSuccess(_Ip);
                    }                    
                }

                catch (SocketException e) { Trace.WriteLine(_Ip + ":" + e.Message); }
                catch (ExceptionA e) { Trace.WriteLine(_Ip + ":" + e.Message); }
                catch (IOException e) { Trace.WriteLine(_Ip+ ":" + e.Message); }
                if(_TcpClient!=null) _TcpClient.Close();
            }
            private static void PingSuccess(Ip _Ip)
            {
                lock ("ping")
                    _List.Add(_Ip);

                Trace.WriteLine("Pong" +_Ip);
            }
        }
    }
}
