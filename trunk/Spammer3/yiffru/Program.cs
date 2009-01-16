using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net.Sockets;

namespace yiffru
{
    class Program
    {
        public static Random _Random = new Random();
        public static string[] _Tags;
        static void Main(string[] args)
        {
            Spammer3.Setup();
            _Tags = File.ReadAllLines("../tags.txt");
            Program _Program = new Program();            
        }
        const string listfile = "list1.txt";
        public Program()
        {
            TcpClient _TcpClient = new TcpClient("yiff.ru", 80);
            Socket _Socket = _TcpClient.Client;
            
            List<string> list = new List<string>();
            
            if (File.Exists(listfile)) list = File.ReadAllLines(listfile).ToList();
            //list.Sort(delegate { return _Random.Next(-1, 1); });
            Shuffle(list);
            while (true)
            foreach (string i in list)
            {
                byte[] _bytes = File.ReadAllBytes("1 Sended get.html");
                Helper.Replace(ref _bytes, "_page_", @"/forum.yiff?tid=" + i, 1);
                _Socket.Send(_bytes);
                Trace.WriteLine("Send:");
                string s = Http.ReadHttp(_Socket).ToStr().Save();
                Match _Match = Regex.Match(s, @"name=""checkID"" value=""(\d+?)"".*\r?\n.*name=""checkValue"" value=""(\d+?)"">");
                if (!_Match.Success)
                {
                    "Scip".Trace();
                    continue;                    
                }

                SendPost(_Socket, i, _Match);
                //Spammer3.ReplaceRandoms(s, null);                
            }
        }
        public static void Shuffle<T>(List<T> listToShuffle)
        {
            Random randomClass = new Random();

            for (int k = listToShuffle.Count - 1; k > 1; --k)
            {
                int randIndx = randomClass.Next(0, k);
                T temp = listToShuffle[k];
                listToShuffle[k] = listToShuffle[randIndx]; // move random num to
                listToShuffle[randIndx] = temp;
            }

        }
        private static void SendPost(Socket _Socket, string i, Match _Match)
        {
            byte[] _bytes2 = File.ReadAllBytes("1 Sended post topic.html");
            Helper.Replace(ref _bytes2, "_id_", i, 1);
            Helper.Replace(ref _bytes2, "_checkid_", _Match.Groups[1].Value, 1);
            Helper.Replace(ref _bytes2, "_checkvalue_", _Match.Groups[2].Value, 1);
            Helper.Replace(ref _bytes2, "_rad_", new Random().Next(0,99999).ToString(), 1);
            Http.Length(ref _bytes2);
            _Socket.Send(_bytes2);
            Trace.WriteLine("Post:");
            Http.ReadHttp(_Socket).ToStr().Save();
        }

        private static void PopulateTopics(Socket _Socket, List<string> list)
        {
            for (int i = 0; i < 500; i += 25)
            {
                byte[] _bytes = File.ReadAllBytes("1 Sended get.html");
                Helper.Replace(ref _bytes, "_id_", i.ToString(), 1);
                _Socket.Send(_bytes);
                string s = Http.ReadHttp(_Socket).ToStr().Save();
                MatchCollection mm = Regex.Matches(s, @"forum.yiff\?tid=(\d+)");
                if (mm.Count.Trace() == 0) Debugger.Break();
                foreach (Match m in mm)
                {
                    if (!list.Contains(m.Groups[1].Value))
                    {
                        list.Add(m.Groups[1].Value);
                        Trace.Write("+");
                    }
                }
            }
            File.WriteAllLines(listfile, list.ToArray());
        }

        

        private static void Priv()
        {
            List<string> list = null;
            if (!File.Exists("users.txt"))
            {
                string file = File.ReadAllText("1.html", Encoding.Default);
                list = new List<string>();
                foreach (Match _Match in Regex.Matches(file, @"\?a=userinfo&id=(\d+)"">(.+?)<"))
                {
                    list.Add(_Match.Groups[1].Value + "," + _Match.Groups[2].Value);
                }
                File.WriteAllLines("users.txt", list.ToArray());
            }
            else
            {
                list = File.ReadAllLines("users.txt").ToList();
            }

            TcpClient _TcpClient = new TcpClient(host, 80);
            Socket _Socket = _TcpClient.Client;
            NetworkStream _NetworkStream = new NetworkStream(_Socket);
            for (int i = 0; i < list.Count; i++)
            {
                string s = list[0];
                list.RemoveAt(0);
                list.Add(s);
                string[] ss = s.Split(',');
                File.WriteAllLines("list.txt", list.ToArray());
                byte[] _bytes = File.ReadAllBytes("2 Sended.html");
                Helper.Replace(ref _bytes, "_id_", ss[0], 1);
                Helper.Replace(ref _bytes, "_name_", ss[1], 1);
                Http.Length(ref _bytes);
                _Socket.Send(_bytes);
                string s2 = Http.ReadHttp(_NetworkStream).ToStr().Save();
            }
        }
        const string host = "yiff.ru";
    }
}
