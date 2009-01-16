using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;

namespace Anonib
{
    class Program
    {
        static void Main(string[] args)
        {
            Spammer3.Setup();
            _Tags = File.ReadAllLines("../tags.txt");
            Program _Program = new Program();
        }
        public Program()
        {
            List<string> list = null;

            Socket _Socket = new TcpClient(host, 80).Client;
            _Socket.Send(File.ReadAllBytes("1 Sended.html"));
            NetworkStream _NetworkStream = new NetworkStream(_Socket);
            byte[] _bytes = Http.ReadHttp(_NetworkStream);
            string received = _bytes.ToStr().Save();
            _Socket.Close();
            MatchCollection _MatchCollection = Regex.Matches(received, @"<script>var b='(\w+)';document\.write", RegexOptions.IgnoreCase);
            list = new List<string>();

            foreach (Match m in _MatchCollection)
            {
                string page = m.Groups[1].Value;
                if (!list.Contains(page)) list.Add(page);
            }
            File.WriteAllLines("list.txt", list.ToArray());

            ("listcount;" + list.Count).Trace();
            for (int i = 0; i < list.Count; i++)
            {
                string page = list[0];
                list.RemoveAt(0);
                list.Add(page);
                File.WriteAllLines("list.txt", list.ToArray());
                try
                {
                    Send2(page);
                    //Thread.Sleep(60000 * 5);
                }
                catch (ExceptionA e) { Trace.WriteLine(e.Message); }
            }            
        }
        public static Random _Random = new Random();
        public string host = "anonib.com";
        public void Send2(string page)
        {
            page.Trace();
            using (Socket _Socket = GetSocket())
            {
                NetworkStream _NetworkStream = new NetworkStream(_Socket);
                string send = File.ReadAllText("2 Sended.html");
                Helper.Replace(ref send, "_id_", page, 2);

                send = Spammer3.ReplaceRandoms(send, _Tags);

                byte[] _bytes = send.ToBytes();
                _bytes = AddFile(_bytes);
                Http.Length(ref _bytes);

                _Socket.Send(_bytes);
                string _received = Http.ReadHttp(_NetworkStream).ToStr().Save();
                _Socket.Close();
                Trace.WriteLine("Result:"+Regex.Match(_received, @"No board info at thread.php|ur browser sent a request that this server could not u|unable to connect,dbi,user5|ry, but your ip has been banned and can not view th|ou don't have permission to access|The verification code you entered is incorrect|Due to massive abuse, proxies aren't allowed to post|index.php\?t=|Unfortunately, this board is set up to not have images|but Thread Lock has been enabled on this board").Value);
            }
        }
        public Socket GetSocket()
        {
            return new TcpClient(host, 80).Client;
            //return Proxy.Socks5Connect("localhost", 9050, host, 80);
        }
        private static byte[] AddFile(byte[] _bytes)
        {
            using (FileStream _FileStream = File.Open("1.png", FileMode.Append))
                _FileStream.WriteByte(1);
            Helper.Replace(ref _bytes,"_file_".ToBytes(), File.ReadAllBytes("1.png"),1);
            return _bytes;
        }
        public static string[] _Tags;
        
    }
}
