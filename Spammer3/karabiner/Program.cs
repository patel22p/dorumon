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
            current = numbers;
            for (; thlen < ints.Length; thlen++)
            {
                Begin(thlen);
            }
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
            catch (Exception) { thlen = 0; ints = new int[2 + 2]; "deafult".Trace(); }
            new Thread(Save).StartBackground();
        }
        List<string> _Begin = new List<string> { "Kars" };
        List<string> _End = new List<string> { ".jpg", ".JPG", ".html", ".htm" };
        private void Begin(int c)
        {                        
            for (; ints[c] < _Begin.Count; ints[c]++)
            {
                if (c == 0)                
                    OnString(_Begin[ints[c]]);
                else
                BruteForce(c - 1, _Begin[ints[c]]);
            }
            ints[c] = 0;
        }
        private void End(int c, string sb)
        {
            for (; ints[c] < _End.Count; ints[c]++)
                OnString(sb + _End[ints[c]]);
        }
        public void OnString(string sb)
        {
            sb.Trace();
        }
        private void BruteForce(int c, string sb)
        {
            if (c == 0)
            {
                //while (i > 50) Thread.Sleep(1);
                //new Thread(Send).Start(sb);
                End(c, sb);
                Thread.Sleep(1);
                return;
            }

            for (; ints[c] < current.Length; ints[c]++)
            {
                BruteForce(c - 1, sb + current[ints[c]]);
            }
            ints[c] = 0;
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

                Socket _Socket = new TcpClient("www014.upp.so-net.ne.jp", 80).Client;
                NetworkStream _NetworkStream = new NetworkStream(_Socket);
                _NetworkStream.Write(string.Format(Res.get, s));
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