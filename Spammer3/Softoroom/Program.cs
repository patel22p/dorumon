using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace Softoroom
{
    class Program
    {
        static void Main(string[] args)
        {
            
            
            new Program();
        }
        List<string> list = new List<string>();
        List<string> sendedList = new List<string>();
        public Program()
        {            
            Spammer3.Setup();            
            if (File.Exists("list.txt")) list = File.ReadAllLines("list.txt").ToList();
            if (File.Exists("sendedList.txt")) sendedList = File.ReadAllLines("sendedList.txt").ToList();            
            Connect();
            while (true)
            {
                Populate();
                Spamm();
            }
        }
        Socket _Socket;
        private Socket Spamm()
        {

            foreach (string user in (from u in list where !sendedList.Contains(u) select u))
            {
                try
                {
                    //if (!sendedList.Contains(user))
                    {
                        "Sending".Trace();
                        sendedList.Add(user);
                        byte[] _bytes = File.ReadAllBytes("1 Sended Post.html");
                        Helper.Replace(ref _bytes, "_id_", user, 1);
                        Helper.Replace(ref _bytes, "_message_", _message, 1);
                        Http.Length(ref _bytes);
                        _Socket.Send(_bytes.Save());
                        string s = Http.ReadHttp(_Socket).ToStr().Save();
                        File.WriteAllLines("sendedList.txt", sendedList.ToArray());
                    }
                    System.Threading.Thread.Sleep(2);
                }
                catch (IOException e)
                {
                    e.Message.Trace();
                    _Socket.Close();
                    Connect();
                }
            }
            return _Socket;
        }

        public string _message { get { return File.ReadAllText("../message.txt", Encoding.Default); } }
         
        private Socket Populate()
        {            
            intA i = new intA("i.txt");
            int max = i.i + 1000;
            for (; i.i < max; i.i += 50)
            {
                try
                {
                    byte[] _bytes = File.ReadAllBytes("1 Sended.html"); ;
                    Helper.Replace(ref _bytes, "_page_", i.i.ToString(), 1);
                    Http.Length(ref _bytes);
                    _Socket.Send(_bytes.Save());
                    string s = Http.ReadHttp(_Socket).ToStr().Save();
                    MatchCollection ms = Regex.Matches(s, @"(?:(?:user\d+.html)|(?:showuser=\d+))"">(.+?)<");
                    ms.Count.Trace();
                    foreach (Match m in ms)
                    {
                        if ((!list.Contains(m.Groups[1].Value)).Trace()) list.Add(m.Groups[1].Value);
                    }
                    "".Trace();
                }
                catch (IOException e)
                {
                    e.Message.Trace();
                    _Socket.Close();
                    Connect();
                    i.i -= 50;
                }
                File.WriteAllLines("list.txt", list.ToArray());
            }
            //_Socket.Receive().ToStr().Save();
            return _Socket;
        }

        private void Connect()
        {
            if (_Socket != null) _Socket.Close();
            TcpClient _TcpClient = new TcpClient("softoroom.net", 80);
            _Socket = _TcpClient.Client;
            
        }
    }
}
