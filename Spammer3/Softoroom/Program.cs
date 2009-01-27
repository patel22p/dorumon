using System;
using System.Collections.Generic;
using System.Linq;
using doru;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Softoroom
{
    class Program
    {
        static void Main(string[] args)
        {
            Spammer3.Setup();
            new Program();
        }
        public static List<string> _list = new List<string>();
        public static ListA _sendedList;
        public static intA _i = new intA("i.txt");
        public Program()
        {
            _sendedList = new ListA("sendedList.txt");
            _sendedList.Clear();
            _list.Add("darkGhost");
            //for (int i = 0; i < 5; i++)
            //    new Client().StartSpammAsync();
            //for (int i = 0; i < 2; i++)
            //{
            //    new Client().StartPopulateAsync();
            //}
            new Client().StartSpamm();

            "end".Trace();            
        }
        public class Client
        {
            Socket _Socket;
            public void StartSpammAsync()
            {
                new Thread(StartSpamm).StartBackground("spamm");
            }
            public void StartPopulateAsync()
            {
                new Thread(StartPopulate).StartBackground("populate");
            }
            public Client()
            {
                Connect();
            }
            public void StartSpamm()
            {
                while (true)
                {

                    string user = GetUser();
                    if (user != null)
                    {
                        try
                        {
                            ("Sending to " + user).Trace();                            
                            byte[] _bytes = File.ReadAllBytes("1 Sended Post.html");
                            Helper.Replace(ref _bytes, "_id_", user, 1);
                            Helper.Replace(ref _bytes, "_b22_", Helper.Randomstr(22),1);
                            Helper.Replace(ref _bytes, "_message_", _message, 1);
                            Http.Length(ref _bytes);
                            _Socket.Send(_bytes);
                            string s = Http.ReadHttp(_Socket).ToStr().Save("spamm");
                            _sendedList.Flush();
                            Thread.Sleep(2);
                        }
                        catch (IOException e)
                        {
                            e.Message.Trace();
                            _Socket.Close();
                            Connect();
                        }
                    }
                    Thread.Sleep(50);
                }
            }

            private static string GetUser()
            {
                lock ("spamm")
                {

                    string user = (from u in _list where !_sendedList.Contains(u) select u).FirstOrDefault();
                    if (user!=null) _sendedList.Add(user);
                    return user;
                }
            }

            public string _message { get { return File.ReadAllText("../message.txt", Encoding.Default); } }

            private void StartPopulate()
            {
                for (; ; _i.i += 50) //164250
                {
                    try
                    {
                        byte[] _bytes = File.ReadAllBytes("1 Sended.html"); ;
                        Helper.Replace(ref _bytes, "_page_", _i.i.ToString(), 1);
                        Http.Length(ref _bytes);
                        _Socket.Send(_bytes);
                        string s = Http.ReadHttp(_Socket).ToStr().Save("populate");
                        MatchCollection ms = Regex.Matches(s, @"(?:(?:user\d+.html)|(?:showuser=\d+))"">(.+?)<");
                        int c=0;
                        foreach (Match m in ms)
                        {
                            string s2 = m.Groups[1].Value;
                            if ((!_list.Contains(s2) && !_sendedList.Contains(s2)))
                            {
                                _list.Add(s2);
                                c++;
                            }
                        }
                        ("Populated " + c).Trace();
                    }
                    catch (IOException)
                    {
                        //e.Message.Trace();
                        "Populate IoException".Trace();
                        _Socket.Close();
                        Connect();
                        _i.i -= 50;
                    }
                }
            }

            private void Connect()
            {
                if (_Socket != null) _Socket.Close();
                TcpClient _TcpClient = new TcpClient("softoroom.net", 80);
                _Socket = _TcpClient.Client;
            }
        }
    }
}
