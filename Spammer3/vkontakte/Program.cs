using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace vkontakte
{
    class Program
    {
        public static string _message { get { return File.ReadAllText("../message.txt",Encoding.Default); } }

        static void Main(string[] args)
        {
            Spammer3.Setup();
            new Program();
        }

        public List<string> _addedGroupsList = new List<string>();
        public List<string> _addedUsersList = new List<string>();
        public Program()
        {            
            const string file = "List.txt";            
            List<string> list = new List<string>();
            if (File.Exists(file)) list = File.ReadAllLines(file).ToList();
            if (File.Exists("_addedGroupsList.txt")) _addedGroupsList = File.ReadAllLines("_addedGroupsList.txt").ToList();
            if (File.Exists("_addedUsersList.txt")) _addedUsersList = File.ReadAllLines("_addedUsersList.txt").ToList();

            
            try
            {
                foreach(string s in list)
                {
                    Match m = Regex.Match(s, @"(\d+) (\w+)");
                    string id = m.Groups[1].Value;
                    string hash = m.Groups[2].Value;                                        
                    try
                    {
                        if (!_addedUsersList.Contains(id))
                        {
                            _addedUsersList.Add(id);
                            File.WriteAllLines("_addedUsersList.txt", _addedUsersList.ToArray());
                            AddFriend(id, hash);
                        }
                        if (!_addedGroupsList.Contains(id))
                        {
                            AddtoGroup(id);
                            _addedGroupsList.Add(id);
                            File.WriteAllLines("_addedGroupsList.txt", _addedGroupsList.ToArray());
                        }                        
                    }
                    catch (SocketException e) { e.Message.Trace(); }
                    catch (IOException e) { e.Message.Trace(); }
                    
                }
            }
            catch (ExceptionB e) { e.Message.Trace(); }
            Trace.WriteLine("good bye");
        }

        
        private static void AddFriend(string id,string hash)
        {
            using (TcpClient _TcpClient = new TcpClient(host, 80))
            {
                Socket _Socket = _TcpClient.Client;
                NetworkStream _NetworkStream = new NetworkStream(_Socket);

                if (hash == "") throw new Exception("emtryline");
                byte[] _bytes = File.ReadAllBytes("1 Sended getlist.html");
                Helper.Replace(ref _bytes, "_id_".ToBytes(), (@"/friend.php?act=add&id=" + id + "&h=" + hash).ToBytes());
                Trace.WriteLine("AddFriend1");
                _Socket.Send(_bytes.Save());
                string s = Http.ReadHttp(_NetworkStream).ToStr().Save();
                RedirectCheck(ref s);
                Match m = Regex.Match(s, @"id=""first_name"" value=""(.+)"".*\r?\n?.*value=""(.+)"".*\r?\n.*id=""sex"" value=""(\w*)""", RegexOptions.Multiline);                                
                
                if (!m.Success)
                {
                    if (Regex.Match(s, "Вы попытались загрузить более одной однотипной страницы в секунду").Trace().Length != 0)
                    {
                        Thread.Sleep(30000);
                        return;
                    }
                    else
                    {
                        try
                        {
                            throw new Exception("Canot add:".Trace());
                        }
                        catch { return; }
                    }
                }
                Thread.Sleep(1000);
                AddFriend2(_Socket, id, hash, _NetworkStream, m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
                Thread.Sleep(3000);
            }
        //    Debugger.Break();
        }
        class ExceptionB : Exception { public ExceptionB(string a) : base(a) { } }
        
        private static void AddFriend2(Socket _Socket, string id, string hash, NetworkStream _NetworkStream,string firstname,string lastname,string sex)
        {
            string s32 = HttpUtility.UrlEncode(_message, Encoding.Default); 
            byte[] text = File.ReadAllBytes(@"1 Sended add friend.html");
            Helper.Replace(ref text, "_id_".ToBytes(), id.ToBytes(),2);            
            Helper.Replace(ref text, "_hash_".ToBytes(), hash.ToBytes(),2);
            Helper.Replace(ref text, "_first_".ToBytes(), firstname.ToBytes(),1);
            Helper.Replace(ref text, "_last_".ToBytes(), lastname.ToBytes(),1);
            Helper.Replace(ref text, "_sex_".ToBytes(), sex.ToBytes(),1);
            Helper.Replace(ref text, "_text_".ToBytes(), s32.ToBytes(),1);
            Http.Length(ref text);
            Trace.WriteLine("addFriend2");
            _Socket.Send(text.Save());
            string str = Http.ReadHttp(_NetworkStream).Save().ToStr();
            RedirectCheck(ref str);
            if (str.Contains(" не можете добавить более 40 друзей за "))            
                throw new ExceptionB("limit");

            if (Regex.Match(str, "Вы не можете добавить этого пользователя в друзья|уже была отправле|получила? уведомление и п").Value.Trace().Length == 0)            
            {
                try
                {
                    throw new Exception("addfriend" + Regex.Match(str, "Вы попытались загрузить более одной однотипной страницы в секунду. Вернитесь назад и повторите попытку".Trace()).Value);
                }
                catch { }
            }
        }
        
        private static void AddtoGroup(string id)
        {
            Trace.WriteLine("AddToGroup");
            using (TcpClient _TcpClient = new TcpClient(host, 80))
            {
                Socket _Socket = _TcpClient.Client;                
                byte[] text = File.ReadAllBytes(@"1 Sended add to group.html");
                Helper.Replace(ref text, "_id_".ToBytes(), id.ToString().ToBytes(), 1);
                Http.Length(ref text);
                _Socket.Send(text.Save());
                string str = Http.ReadHttp(_Socket).Save().ToStr();


                if (Regex.Match(str, "Пользователь уже в группе|Приглашение уже высылалось|риглашение выслан|ватель запретил п|ья могут приглашать этого по").Value.Trace() == "")
                    if (str.Contains("те пригласить только 40 человек")) throw new ExceptionB("limit");
                    else throw new ExceptionA("unknown");
            }
            Thread.Sleep(1000);
        }
        
        const string host = "vkontakte.ru";
        private static void Populate(string file)
        {
            List<string> list= new List<string>();
            TcpClient _TcpClient = new TcpClient(host, 80);
            Socket _Socket = _TcpClient.Client;
            NetworkStream _NetworkStream = new NetworkStream(_Socket);
            for (int i = 0; i < 499; i += 10)
            {
                byte[] _bytes = File.ReadAllBytes(@"1 Sended getlist.html");
                Helper.Replace(ref _bytes, "_id_".ToBytes(), (@"/search.php?&group=9618&o=1&st=" + i).ToBytes(), 1);
                _Socket.Send(_bytes);
                string s = Http.ReadHttp(_NetworkStream).Save().ToStr();
                MatchCollection ms = Regex.Matches(s, @"friend.php\?act=add&id=(\d+)&h=(\w+)");
                if (ms.Count == 0)
                {
                    //Debugger.Break();
                    Thread.Sleep(2000);
                    i--;
                    continue;
                }
                Trace.WriteLine("Found" + ms.Count);
                foreach (Match m in ms)
                {
                    string s2 = m.Groups[1].Value + " " + m.Groups[2].Value;
                    if (s2.Length == 0) Debugger.Break();
                    if (!list.Contains(s2))
                    {
                        list.Add(s2);
                        //Trace.Write("+");
                    }
                }
                File.WriteAllLines(file, list.ToArray());
                Thread.Sleep(1000);
            }
        }

        public static Random _Random = new Random();
        private static void RedirectCheck(ref string str)
        {
            using (TcpClient _TcpClient = new TcpClient(host, 80))
            {
                Socket _Socket = _TcpClient.Client;
                NetworkStream _NetworkStream = new NetworkStream(_Socket);
                Match _Match = Regex.Match(str.Substr("\r\n\r\n"), @"Location: (.*)", RegexOptions.IgnoreCase);
                if (_Match.Success)
                {
                    byte[] _bytes2 = File.ReadAllBytes("1 Sended getlist.html");
                    Helper.Replace(ref _bytes2, "_id_".ToBytes(), ("/" + _Match.Groups[1].Value.Trim()).ToBytes());
                    _Socket.Send(_bytes2);
                    Trace.WriteLine("Redirect");
                    str = Http.ReadHttp(_NetworkStream).Save().ToStr();
                }
            }
        }
    }
}
