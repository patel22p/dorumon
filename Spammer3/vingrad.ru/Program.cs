using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using doru;
using System.Text.RegularExpressions;
using System.Web;

namespace Vingrad.ru
{
    class Program
    {
        static void Main(string[] args)
        {
            Spammer3.Setup();
            new Program();
        }
        //string passhash = "033e31d41981c4f7dd5ce89df0b6ecff";
        //string sessid = "758a32ddcfe106024b91e579899ceb81";
        public Program()
        {
            Connect();
            while (true)
            {
                Populate();
                Spamm();
            }
        }
        intA i = new intA("i.txt");
        private void Spamm()
        {
            TcpClient _TcpClient = new TcpClient("vingrad.ru", 80);
            _Socket = _TcpClient.Client;
            foreach (string s in _ListA)
            {
                try
                {
                    if (!_listPosted.Contains(s))
                    {
                        "sending".Trace();
                        _listPosted.Add(s);
                        byte[] _bytes = File.ReadAllBytes("1 Sended Post.html");
                        Helper.Replace(ref _bytes, "_title_", HttpUtility.UrlEncode("привет", Encoding.UTF8), 1);
                        Helper.Replace(ref _bytes, "_name_", s, 1);
                        Helper.Replace(ref _bytes, "_message_", HttpUtility.UrlEncode(_message,Encoding.UTF8), 1);
                        Http.Length(ref _bytes);
                        _Socket.Send(_bytes.Save());
                        string s2 = Http.ReadHttp(_Socket).ToStr().Save();
                        _listPosted.Flush();
                    }
                }
                catch (IOException e) { e.Message.Trace(); Connect(); }
            }
        }
        Socket _Socket;
        ListA _listPosted = new ListA("postedlist.txt");
        ListA _ListA = new ListA("list.txt");            
        private void Populate()
        {
            Connect();
            int max = i.i + 1000;
            for (; i.i < max; i.i += 50)
            {
                try
                {
                    byte[] _bytes = File.ReadAllBytes("1 Sended.html");
                    Helper.Replace(ref _bytes, "_page_", String.Format(@"/forum/act-Members/name_box/all/max_results-50/filter-3/sort_order-desc/sort_key-posts/{0}.html", i.i), 1);
                    _Socket.Send(_bytes);
                    string s = Http.ReadHttp(_Socket).ToStr().Save();

                    MatchCollection ms = Regex.Matches(s, @"<a href=""/users/.+?"">(.+?)</a>");

                    foreach (Match m in ms)
                    {
                        _ListA.Add(HttpUtility.HtmlDecode(m.Groups[1].Value)).Trace();
                    }
                    "".Trace();
                    _ListA.Flush();
                }
                catch (IOException) { Connect(); }
            }            
        }

        private void Connect()
        {
            if (_Socket != null) _Socket.Close();
            TcpClient _TcpClient = new TcpClient("forum.vingrad.ru", 80);
            _Socket = _TcpClient.Client;
        }



        public static string _message { get { return File.ReadAllText("../message.txt",Encoding.Default); } }
    }

}
