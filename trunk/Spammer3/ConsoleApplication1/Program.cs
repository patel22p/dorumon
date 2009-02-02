//vBulletin® Version 3.7.4
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using doru;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Web;
namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            
            new Program();
        }
        List<string> list = new List<string>();
        public Program()
        {
            new Thread(Populate).StartBackground("populate");
            new Thread(Spamm).StartBackground("spamm");
        }
        intA i = new intA("i.txt");
        private void Populate()
        {
            while (true)
            {
                Socket _Socket = Connect();
                for (; ; i.i++)
                    lock ("spamm")
                    {
                        try
                        {
                            string s2 = String.Format(Res._Get, i);
                            //Debugger.Break();
                            _Socket.Send(s2);
                            string s = Http.ReadHttp(_Socket).ToStr().Save("populate");
                            MatchCollection ms = Regex.Matches(s, @"u=\d+"">(.+?)</a>", RegexOptions.IgnoreCase);
                            foreach (Match m in ms)
                            {
                                int i2=0;
                                string s3 = m.Groups[1].Value + ";";
                                if (!list.Contains(s3))
                                {
                                    i2++;
                                    list.Add(s3);
                                    Trace.Write(s3);
                                }
                                Trace.WriteLine("populated:" + i);
                            }
                        }
                        catch (IOException) { _Socket = Connect(); i.i--; }

                    }
            }
        }

        private static Socket Connect()
        {
            Socket _Socket = new TcpClient("forum.codenet.ru", 80).Client;
            return _Socket;
        }

        public void Spamm()
        {
            Socket _Socket = Connect();

            while (true)
            {
                if (list.Count > 100)
                {
                    lock ("spamm")
                    {

                        while (true)
                        {
                            try
                            {
                                string usrs = HttpUtility.UrlEncode(list.Join(";"));
                                _Socket.Send(String.Format(Res._post, usrs));
                                string s = Http.ReadHttp(_Socket).ToStr().Save("spamm");
                                Match m;
                                if ((m = Regex.Match(s, @"Вы не можете отправить сообщение (.+?),")).Success)
                                    list.Remove(m.Groups[1].Value);
                                else break;
                            }
                            catch (IOException) { _Socket = Connect(); }
                        }
                        list.Clear();
                    }
                }
                Thread.Sleep(100);
            }
            
        }
    }
}
