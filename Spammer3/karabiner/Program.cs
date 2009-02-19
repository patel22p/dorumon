using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.IO;

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
        public Program()
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
            catch (Exception) { thlen = 0; ints = new int[5]; "deafult".Trace(); }
            new Thread(Save).StartBackground();
            current = words;
            for (; thlen < ints.Length; thlen++)
            {
                NwMethod(thlen, "Kar");                
            }
            

        }
        public void Save()
        {
            while (true)
            {
                Thread.Sleep(3);
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

        public int[] ints;
        private void NwMethod(int c, string sb)
        {
            if (c == 0)
            {
                while (i > 50) Thread.Sleep(1);
                new Thread(Send).Start(sb);                
                Thread.Sleep(1);
                return;
            }
            
            for (; ints[c]<current.Length;ints[c]++)            
            {
                NwMethod(c-1, sb + current[ints[c]]);
            }
            ints[c] = 0;
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