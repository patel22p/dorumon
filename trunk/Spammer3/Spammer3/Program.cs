using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Net.Sockets;
using doru;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;
using System.Xml.Schema;

namespace Spammer3
{
    public class Program
    {

        public static XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Database),new XmlAttributeOverrides(),new Type[]{ typeof(PatternReplacement),typeof(FileArrayReplacement)},new XmlRootAttribute(),"test");

        static void Main(string[] args)
        {
            //var sadasdsaa = _XmlSerializer.Deserialize(File.Open("asdas.xml", FileMode.Open));
            Database _Database = new Database();
            Task _Task = new Task();
            _Task._Replacements.Add(new Replacement());
            _Task._Replacements.Add(new PatternReplacement());
            _Database._Tasks.Add(_Task);
            
            _XmlSerializer.Serialize(File.Open("test.xml", FileMode.Create),_Database);

            if (args.Length != 0) path = String.Join(" ", args);
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                
            }                        

            Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
            new Program();
        }
        public static string path = Path.GetFullPath(@"../../../test/test.xml");
        static string title;
        public static string Title
        {
            set { if (title != value) Console.Title = title = value; }
            get { return title; }
        }
        public class Database
        {
            public List<Task> _Tasks = new List<Task>();
            public string _Host;
            public int _Loop=1;
            public string _RandomFile = "../tags.txt";
        }
        public class Replacement
        {
            public virtual string a { get; set; }
            public virtual string b { get; set; }
            public virtual string Replace(string text)
            {
                return text.Replace(a, b);
            }
        }
        public class PatternReplacement : Replacement
        {
            public override string Replace(string text)
            {
                Match _Match = Regex.Match(_FileLine, b, RegexOptions.IgnoreCase);
                if (!_Match.Success) throw new ExceptionA("PatternReplacement Error");
                return text.Replace(a, _Match.Groups[1].Value);
            }
        }
        public class FileArrayReplacement : Replacement
        {
            public FileArrayReplacement()
            {
                b = "../tags.txt";
            }
            List<string> _list;
            [XmlAttribute]
            public int count = 1;
            public override string Replace(string text)
            {
                StringBuilder _StringBuilder = new StringBuilder();
                _list = File.ReadAllLines(b).ToList();
                for (int i = 0; i < count; i++)
                {
                    string s = _list[0];
                    _list.Remove(s);
                    _list.Add(s);
                    _StringBuilder.Append(s + " ");
                    File.WriteAllLines(b, _list.ToArray());
                }
                return _StringBuilder.ToString(0, _StringBuilder.Length - 1);
            }
        }

        public class Task
        {
            public string _Upload;
            public enum ProxyType { Tor, None, Socks4, Socks5, Web };
            public ProxyType _ProxyType;
            public int _Sleep = 5000;
            public int _Loop = 1;
            public string _TimeSpan;
            public string _OutputFile;
            public string _Format = "{0}";
            public string _Pattern;
            public string _InputFile;
            public string _SendFile;
            public List<Replacement> _Replacements = new List<Replacement>();
        }


        public static Random _Random = new Random();
        public class Trace
        {
            public static void WriteLine(string o)
            {
                System.Diagnostics.Trace.WriteLine(o);
            }
            public static void WriteLine(object o)
            {
                System.Diagnostics.Trace.WriteLine(o);
            }
        }
        public Program()
        {
            new Thread(StartTor).StartBackground();
            
            _XmlSerializer.UnknownElement += new XmlElementEventHandler(delegate(object o, XmlElementEventArgs e) { throw new ExceptionA(e.LineNumber.ToString()); });
            using (FileStream _FileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                _Database = (Database)_XmlSerializer.Deserialize(_FileStream);
            }
            StartSpamming();
        }




        Database _Database;
        public const string _Tor = @"C:\Program Files\Vidalia Bundle\Tor\tor.exe";
        public static string _FileLine;
        public static int _DatabaseLoop;
        public static int _TaskLoop;
        public List<string> _RandomTags;
        public void StartSpamming()
        {
            _RandomTags = File.ReadAllLines(_Database._RandomFile).ToList();
            for (_DatabaseLoop = 0; _DatabaseLoop < _Database._Loop; _DatabaseLoop++)
            {
                foreach (Task _Task in _Database._Tasks)
                {
                    if (File.Exists(_Task._OutputFile))
                    {
                        if ((DateTime.Now - new FileInfo(_Task._OutputFile).LastWriteTime) < TimeSpan.Parse(_Task._TimeSpan ?? "0:0:0")) continue;
                        File.Delete(_Task._OutputFile);
                    }

                    string _SendFile = File.ReadAllText(_Task._SendFile);
                    List<string> _InputFile = _Task._InputFile == null ? new List<string> { null } : File.ReadAllLines(_Task._InputFile).ToList();


                    for (int c = 0; c < _InputFile.Count; c++)
                    {
                        UpdateInputFile(_Task, _InputFile);
                        for (_TaskLoop = 0; _TaskLoop < _Task._Loop; _TaskLoop++)
                        {
                            string text = (string)_SendFile.Clone();
                            byte[] _bytes = ReplaceText(_Task, text);
                            File.WriteAllBytes("LastSended.html", _bytes);
                            SendText(_Task, _bytes);
                            Thread.Sleep(_Task._Sleep);
                        }
                    }
                }
            }
        }

        private void SendText(Task _Task, byte[] _bytes)
        {
            Socket _Socket;
            while (true)
            {
                try
                {
                    _Socket = Connect(_Task);
                    _Socket.Send(_bytes);
                    ReadResponse(_Task, _Socket);
                    _Socket.Close();
                    break;
                }
                catch (ExceptionA e) { Trace.WriteLine(e.Message); }
                catch (IOException e) { Trace.WriteLine(e.Message); }
            }
        }

        private byte[] ReplaceText(Task _Task,  string text)
        {
            foreach (Replacement _Replacement in _Task._Replacements)
            {
                text = _Replacement.Replace(text);
            }
            text = ReplaceRandoms(text);

            text = text.Replace("_host_", _Database._Host);

            text = text.Replace("_Task_", _TaskLoop.ToString());
            text = text.Replace("_Database_", _DatabaseLoop.ToString());
            
            byte[] _bytes = ASCIIEncoding.ASCII.GetBytes(text);

            if (_Task._Upload != null)
            {
                using (FileStream _FileStream = new FileStream(_Task._Upload, FileMode.Append, FileAccess.Write))
                {
                    _FileStream.Write(_Random.NextBytes(10));
                }
                _bytes = _bytes.Replace("_file_", File.ReadAllBytes(_Task._Upload));
            }
            _bytes = _bytes.Replace("_length_", (_bytes.Length - 4 - _bytes.IndexOf2("\r\n\r\n")).ToString());
            return _bytes;
        }

        private static void UpdateInputFile(Task _Task, List<string> _InputFile)
        {

            if (_Task._InputFile != null)
            {
                _FileLine = _InputFile[0];
                _InputFile.Remove(_FileLine);
                _InputFile.Add(_FileLine);
                File.WriteAllLines(_Task._InputFile, _InputFile.ToArray());
                Trace.WriteLine("Sended:" + _FileLine);
            }
        }

        

        private string ReplaceRandoms(string text)
        {
            text = Regex.Replace(text,@"_randomtext(\d+)_",delegate(Match m)
            {
                StringBuilder _StringBuilder = new StringBuilder();
                for (int i2 = 0; i2 < int.Parse(m.Groups[1].Value); i2++)
                {
                    _StringBuilder.Append(_RandomTags[_Random.Next(_RandomTags.Count)] + " ");
                }
                return _StringBuilder.ToString(0, _StringBuilder.Length - 1);
            });
            
            foreach (Match m in Regex.Matches(text, @"_randomcode(\d+)_"))
            {
                text = text.Replace(m.Value, _Random.RandomString(int.Parse(m.Groups[1].Value)));
            }
            return text;
        }



        private Socket Connect(Task _Task)
        {
            switch (_Task._ProxyType)
            {
                case Task.ProxyType.None:
                    return new TcpClient(_Database._Host, 80).Client;
                case Task.ProxyType.Tor:
                    return Proxy.Socks5Connect("localhost", 9050, _Database._Host, 80);
                default: throw new Exception();
            }

        }

        private void ReadResponse(Task _Task, Socket _Socket)
        {
            NetworkStream _NetworkStream = new NetworkStream(_Socket);
            _NetworkStream.ReadTimeout = 30000;
            string text = ASCIIEncoding.ASCII.GetString(Http.ReadHttp(_NetworkStream));
            text.Save();
            List<string> _List;
            if (File.Exists(_Task._OutputFile))
                _List = File.ReadAllLines(_Task._OutputFile).ToList();
            else
                _List = new List<string>();
            MatchCollection _Matches = Regex.Matches(text, _Task._Pattern, RegexOptions.IgnoreCase);
            Console.WriteLine("\nMatches:" + _Matches.Count + "  " + Title);
            foreach (Match _Match in _Matches)
            {
                if (_Match.Success)
                {
                    string s = string.Format(_Task._Format, _Match.Groups.Cast<object>().ToArray());

                    Console.Write(s);
                    if (!_List.Contains(s)) _List.Add(s);
                }
            }
            if (_List.Count > 0 && _Task._OutputFile != null) File.WriteAllLines(_Task._OutputFile, _List.ToArray());
        }


        public static void UrlList()
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
            string[] topics = new string[] { "furi" };
            List<string> _List = new List<string>();
            foreach (string t in topics)
            {
                _List.Add("/" + t + "/");
                for (int i = 1; i <= 14; i++)
                {
                    _List.Add("/" + t + "/" + i + ".html");
                }
            }
            File.WriteAllLines("urllist.txt", _List.ToArray());
        }
        public static void StartTor()
        {
            if ((from p in Process.GetProcesses() where p.ProcessName == "tor" select p).FirstOrDefault() == null)
                Process.Start(_Tor);
            Socket _Socket = new TcpClient("localhost", 9051).Client;
            _Socket.Send("AUTHENTICATE \"er54s4\"\r\n");
            while (true)
            {
                _Socket.Send("getinfo stream-status\r\n");
                string s= _Socket.ReceiveText();
                StringBuilder _StringBuilder = new StringBuilder();
                foreach (Match m in Regex.Matches(s, @"\d\d?\d? (\w+ \d\d?\d?) [\d.]+:\d\d?\d?"))
                {
                    _StringBuilder.Append(m.Groups[1].Value+",");
                }
                Title = _StringBuilder.ToString();
                Thread.Sleep(100);
            }
        }
    }
}





//Database _Database = new Database();
//Task _Task = new Task();
//_Task._Replacements.Add(new Replacement());
//_Database._Tasks.Add(_Task);
//_XmlSerializer.Serialize(File.Create("asd.xml"), _Database);
//        private static void NewMethod()
//        {
//            Socket _Socket = Proxy.Socks5Connect("127.0.0.1", 9050, "www.google.ru", 80);
//            Trace.WriteLine("connected");
//            _Socket.Send(@"GET http://www.google.ru/ HTTP/1.0
//Host: www.google.ru
//Proxy-Connection: Keep-Alive
//
//");
//            NetworkStream _NetworkStream = new NetworkStream(_Socket);
//            byte[] _bytes = Http.ReadHttp(_NetworkStream);
//            Trace.WriteLine(_bytes.ToStr());
//        }