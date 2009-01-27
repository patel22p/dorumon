using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using doru;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace BrainWash
{
    public class Program
    {

        static void Main(string[] args)
        {
            Spammer3.Setup();
            new Program();
        }
        static string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        public Program()
        {
            MatchCollection ms = Regex.Matches(File.ReadAllText("Ips.txt"), "/(.+?);(.+?);(.+)\r");
            foreach (Match m in ms)
            {
                Client _BrainWash = new Client { host = m.Groups[1].Value, port = int.Parse(m.Groups[2].Value), page = m.Groups[3].Value };
                _BrainWash.StartAsync();
            }
        }
        public class Client
        {
            public string host;
            public int port;
            public string page;
            NetworkStream _NetworkStream;
            public void StartAsync()
            {
                new Thread(Start).Start();
            }
            private void Start()
            {
                Connect();
            CrFile:
                string file = path+"/"+(name + DateTime.Now.ToString().Replace(":", ".") + ".mp3").Trace();
                using (FileStream _FileStream = new FileStream(file, FileMode.Create, FileAccess.Write))
                {
                    while (true)
                    {
                        try
                        {
                            _FileStream.Write(_NetworkStream.Read(blocksize.Value));
                            _FileStream.Flush();
                            int len = _NetworkStream.ReadB() * 16;
                            if (len != 0)                            
                                _NetworkStream.Read(len).ToStr().Trace(); //Regex.Match(metadataHeader, "(StreamTitle=')(.*)(';StreamUrl)").Groups[2].Value.Trim();                           
                            if (_FileStream.Length > 3000 * 1024) goto CrFile;
                        }
                        catch (IOException) { "reconnecting".Trace(); Connect(); }
                    }
                }
                throw new Exception("exit");
            }
            public string name;
            public int? blocksize;
            private Socket Connect()
            {
                while (true)
                    try
                    {
                        Socket _Socket = new TcpClient(host, port).Client;
                        string s = Res.get.Replace("_host_", host + ":" + port).Replace("_page_", page);
                        _Socket.Send(s);
                        _NetworkStream = new NetworkStream(_Socket);
                        _NetworkStream.ReadTimeout = 20000;
                        string s2 = _NetworkStream.Cut("\r\n\r\n").ToStr();
                        name = Regex.Match(s2, "icy-name:(.+)\r").Groups[1].Value;
                        blocksize = int.Parse(Regex.Match(s2, @"icy-metaint:(.+)\b").Groups[1].Value);
                        return _Socket;
                    }
                    catch { }
            }
        }
    }
}
