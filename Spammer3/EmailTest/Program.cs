using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using doru;

namespace EmailTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Spammer3.Setup();
            //TcpRedirect _TcpRedirect = new TcpRedirect();
            //_TcpRedirect._LocalPort = 25;
            //_TcpRedirect._RemoteIpDelegate = delegate{ new TcpClient ("localhost",

            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
            Console.Title = "Mail";
            Directory.SetCurrentDirectory(Path.GetFullPath("../../"));
            new Program();
        }

        public Program()
        {
            Thread _Thread = new Thread(Pop);
            _Thread.IsBackground = true;
            _Thread.Start();
            TcpListener _TcpListener = new TcpListener(IPAddress.Any, 25);
            _TcpListener.Start();
            while (true)
            {
                TcpClient _TcpClient = _TcpListener.AcceptTcpClient();
                Socket _Socket = _TcpClient.Client;
                Thread _Thread1 = new Thread(StartClient);
                _Thread1.Start(_Socket);
            }
        }

        private static void StartClient(object o)
        {
            Trace.WriteLine("<<<<Disconnected>>>>");
            Socket _Socket = (Socket)o;
            _Socket.@Send(@"220 a183.ip8.netikka.fi ESMTP
");
            try
            {
                while (_Socket.Connected)
                {
                    byte[] _buffer = _Socket.Receive();
                    string s = ASCIIEncoding.ASCII.GetString(_buffer);
                    string[] ss = s.Trim('\n', '\r').Split(' ');
                    Trace.WriteLine("<<<<<<<<<<received>>>>>>>>");
                    Trace.WriteLine(s);
                    switch (ss[0])
                    {
                        case "HELO":
                            _Socket.Send(@"250 Hello.
");
                            break;
                        case "EHLO":
                            _Socket.Send(@"250-hmailserver
250-SIZE
250 AUTH LOGIN
");
                            break;
                        case "MAIL": goto case "RCPT";
                        case "RCPT":
                            _Socket.Send(@"250 OK
");
                            break;
                        case "DATA":
                            _Socket.Send(@"354 OK, send.
");
                            break;
                        default:
                            Match _Match = Regex.Match(s, @"\bhttps?://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]", RegexOptions.IgnoreCase);
                            if (_Match.Success)
                            {
                                Trace.WriteLine("<<<<<<<<<<<<<<Downloading>>>>>>>>>>>>>" + s);
                                WebClient _WebClient = new WebClient();
                                string data = _WebClient.DownloadString(_Match.Value);
                                Trace.WriteLine(data);
                                return;
                            }
                            break;
                    }
                }
            }
            catch (SocketException) { }
            Trace.WriteLine("<<<<Disconnected>>>>");
        }
        public void Pop()
        {
            TcpListener _TcpListener = new TcpListener(IPAddress.Any, 110);
            _TcpListener.Start();
            Socket _Socket = _TcpListener.AcceptSocket();
            byte[] _buffer = new byte[9999];
            try
            {
                while (true)
                {
                    int c = _Socket.Receive(_buffer);
                    if (c == 0) break;
                    Trace.WriteLine("Pop Received:" + ASCIIEncoding.ASCII.GetString(_buffer, 0, c));
                }
            }
            catch (SocketException) { }
        }
    }
}
