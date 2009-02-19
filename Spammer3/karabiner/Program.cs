using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Collections;

namespace karabiner
{


    class Program
    {

        static void Main(string[] args)
        {
            Logging._RedirectOutPut = false;
            Logging.Setup();
            new Program();
        }

        const string words = "1234567890QWERTYUIOPZXCVBNMqwertyuiopasdfghjklzxcvbnm";

        string numbers = "0123456789";
        string current;
        string _ok = "HTTP/1.1 200 OK\r\n";
        ListA _ListA = new ListA("success.txt");
        int thlen;
        public int[] ints;

        public Program()
        {
            Load();
            new Thread(Save).StartBackground();
            current = words;
            for (; thlen < ints.Length; thlen++)
            {
                Begin(thlen);
            }
        }

        
        List<string> _Begin = new List<string> { "KarS" ,"KarRena","KarRf251",""};
        List<string> _End = new List<string> { ".jpg", ".JPG", ".html", ".htm" };
        private void Begin(int c)
        {                        
            for (; ints[c] < _Begin.Count; ints[c]++)
            {
                if (c == 0)                
                    Send(_Begin[ints[c]]);
                else
                    BruteForce(c - 1, _Begin[ints[c]]);
            }
            ints[c] = 0;
        }
        private void End(int c, string sb)
        {
            for (; ints[c] < _End.Count; ints[c]++)
                Send(sb + _End[ints[c]]);
            ints[c] = 0;
        }
        public void Send(string sb)
        {
            while (i > 200) Thread.Sleep(2);
            new Thread(Send).Start(sb);
        }
        private void BruteForce(int c, string sb)
        {
            if (c == 0)
            {

                End(c, sb);
                Thread.Sleep(2);
                return;
            }

            for (; ints[c] < current.Length; ints[c]++)
            {
                BruteForce(c - 1, sb + current[ints[c]]);
            }
            ints[c] = 0;
        }
        private void Load()
        {
            try
            {
                string[] ss = File.ReadAllText("save.txt").Split(":");
                thlen = int.Parse(ss[0]);
                ss = ss[1].Split(",");
                ints = new int[ss.Length];
                for (int i = 0; i < ss.Length; i++)
                    ints[i] = int.Parse(ss[i]);

            }
            catch (Exception) { thlen = 0; ints = new int[3 + 2]; "deafult".Trace(); }
            
        }
        public void Save()
        {
            while (true)
            {
                Thread.Sleep(10);
                string s = thlen + ":" + ints.Join(",");
                File.WriteAllText("save.txt", s);
            }
        }
        int i;
        public void Send(object o)
        {
            i++;
            string s = (string)o;
            try
            {

                Socket _Socket = new TcpClient("192.84.196.226", 8080).Client;
                using (NetworkStream _NetworkStream = new NetworkStream(_Socket))
                {
                    _NetworkStream.Write(string.Format(Res.proxy, s));
                    string s2 = _NetworkStream.Cut("\r\n").ToStr();
                    s.Trace(s2.Trim());
                    if (s2 == _ok)
                    {
                        if (!_ListA.Contains(s)) _ListA.Add(s);
                        _ListA.Flush();
                        "success".Trace();
                    }                
                _Socket.Close();
                }
            }
            catch (IOException) { "err".Trace(); }
            catch (SocketException) { "connerr".Trace(); }
            i--;
        }



    }
}
//class Patok
//{
//    public Patok()
//    {
//        Connect();
//    }
//    string get;
//    public void StartAsync(string s)
//    {
//        this.get = s;
//        _Free.Remove(this);
//        new Thread(Start).Start();
//    }
//    Socket _Socket;
//    NetworkStream _NetworkStream;
//    public void Connect()
//    {
//        _Socket = new TcpClient("www014.upp.so-net.ne.jp", 80).Client;
//        _NetworkStream = new NetworkStream(_Socket);
//    }
//    public void Start()
//    {
//        while (true)
//            try
//            {
//                _NetworkStream.Write(String.Format(Res.get, get));
//                string s = _NetworkStream.Cut("\r\n").ToStr();
//                s.Trace(get);
//                _Free.Add(this);                        
//                break;
//            }
//            catch (IOException) { }

//    }
//}