///vBulletin® Version 3.7.4
using System;
using System.Collections.Generic;
using System.Linq;
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
            Spammer3.Setup();
            //Encoding.Default = Encoding.UTF8;
            new Program();
        }
        List<string> list = new List<string>();
        public Program()
        {
            new Thread(Populate).StartBackground("populate");
            new Thread(Spamm).StartBackground("spamm");
            Thread.Sleep(-1);
        }
        ListA posted = new ListA("posted.txt");
        intA i = new intA("i.txt");
        private void Populate()
        {
            while (true)
            {
                Socket _Socket = Connect();
                for (; ; i.i++)
                {
                    Trace.WriteLine("i:" + i);
                    //while (list.Count > 500) Thread.Sleep(100);
                    lock ("spamm")
                    {
                        try
                        {
                            string s2 = String.Format(Res._Get, i);
                            //Debugger.Break();
                            _Socket.Send(s2);
                            string s = Http.ReadHttp(_Socket).ToStr().Save("populate");
                            MatchCollection ms = Regex.Matches(s, @"u=\d+"">([\[\]{}() <>\- .^!a-zA-Z\dа-яА-Я_@$]+?)</a>", RegexOptions.IgnoreCase);
                            int i2 = 0;
                            foreach (Match m in ms)
                            {
                                
                                string s3 = HttpUtility.HtmlDecode(m.Groups[1].Value);
                                if (!list.Contains(s3) && !posted.Contains(s3))
                                {
                                    i2++;
                                    list.Add(s3);
                                    posted.Add(s3);
                                }                                
                            }
                            posted.Flush();
                            Trace.WriteLine("populated:" + i2);
                        }
                        catch (IOException) { _Socket = Connect(); i.i--; }
                    }
                }
            }
        }

        private static Socket Connect()
        {
            Socket _Socket = new TcpClient("forum.codenet.ru", 80).Client;
            return _Socket;
        }
        public string _message { get { return File.ReadAllText("../message.txt", Encoding.Default); } }
        public void Spamm()
        {
            Socket _Socket = Connect();

            while (true)
            {
                if (list.Count > 300)
                {
                    lock ("spamm")
                    {

                        while (true)
                        {
                            try
                            {                                
                                string usrs = list.Join(";");
                                _Socket.Send(Res._get2);
                                string s=Http.ReadHttp(_Socket).ToStr();
                                string cookie = Regex.Match(s, "bbsessionhash=(.+?);").Groups[1].Value;
                                string token = Regex.Match(s, "name=\"securitytoken\" value=\"(.+?)\"").Groups[1].Value;
                                if (token == "" || cookie == "") Debugger.Break();
                                string s2 = Http.Length(String.Format(Res._post, Enc(usrs), cookie, token,Enc(_message),Enc("привет"))).Save("spammsend");
                                _Socket.Send(s2);
                                s = Http.ReadHttp(_Socket).ToStr().Save("spammrecv");
                                Match m;
                                if ((m = Regex.Match(s, @"Вы не можете отправить сообщение (.+?),")).Success ||
                                    (m = Regex.Match(s, @"Следующие пользователи не найдены: <ol><li>(.+?)</li>")).Success||
                                    (m = Regex.Match(s, @"<li>(.+?) превысил")).Success)
                                    list.Remove(m.Groups[1].Value.Trace("removed:"));
                                else break;
                                //string test=HttpUtility.HtmlDecode("Max aka &quot;amv&quot;");
                                //Debugger.Break();
                            }
                            catch (IOException) { _Socket = Connect(); }
                        }
                        list.Clear();
                    }
                }
                Thread.Sleep(100);
            }
            
        }
        public string Enc(string s)
        {
            return HttpUtility.UrlEncode(s, Encoding.Default);
        }
    }
}
