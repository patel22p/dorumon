using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;

namespace GamedevOrg
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Spammer3.Setup();
            new Program();
        }
        const string host = "www.gamedev.org";
        public Program()
        {
            Populate();
        }
 
        public static void Test()
        {
            Spammer3.Setup();
            Clipboard.SetText("\""+String.Join("\",\"", File.ReadAllLines(file).ToArray())+"\"");
            
        }
        public static int i { get { if (File.Exists("i.txt"))return int.Parse(File.ReadAllText("i.txt")); return 500; } set { File.WriteAllText("i.txt", value.ToString()); } }
        const string file = "list.txt";
        private static void Populate()
        {
            List<string> list = new List<string>();
            if (File.Exists(file)) list = File.ReadAllLines(file).ToList();
            for (; i < 2000; i += 7)
                try
                {
                    using (TcpClient _TcpClient = new TcpClient(host, 80))
                    {
                        Socket _Socket = _TcpClient.Client;


                        Byte[] _bytes = File.ReadAllBytes("1 Sended get.html");
                        Helper.Replace(ref _bytes, "_i_", i.ToString(), 1);
                        _Socket.Send(_bytes);
                        string re = Http.ReadHttp(_Socket).ToStr().Save();
                        MatchCollection mm = Regex.Matches(re, @"; return false;"" title=""([\w _\-\.]+)""");
                        if (mm.Count.Trace() == 0) Debugger.Break();
                        foreach (Match m in mm)
                        {
                            list.Add(m.Groups[1].Value);
                        }
                        File.WriteAllLines(file, list.ToArray());

                    }
                }
                catch (SocketException) { }
                catch (IOException) { }
        }
    }
}
