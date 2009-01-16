using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Web;

namespace Flasher
{
    class Program
    {
        
        static void Main(string[] args)
        {                         
            Spammer3.Setup();
            new Program();
        }
        intA i = new intA("i.txt");
        ListA _ListA = new ListA("list.txt");
        ListA _ListA2 = new ListA("list2.txt");
        public Program()
        {
            Connect();
            //NewMethod1();
            Spamm();

        }

        private void GetToken()
        {            
            byte[] _bytes = File.ReadAllBytes("1 Sended.html");
            Helper.Replace(ref _bytes, "_page_", @"/forum/private.php?do=newpm", 1);
            _Socket.Send(_bytes.Save());
            string s = Http.ReadHttp(_Socket).ToStr().Save();
        }

        private void Spamm()
        {
            while (true)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string s in _ListA)
                    if (!_ListA2.Contains(s))
                    {
                        sb.Append(s + ";");
                    }
                byte[] _bytes = File.ReadAllBytes("1 Sended post.html");
                Helper.Replace(ref _bytes, "_users_", HttpUtility.UrlEncode(sb.ToString(0, sb.Length - 1), Encoding.Default), 1);
                string s2 = HttpUtility.UrlEncode(File.ReadAllText("../message.txt", Encoding.Default), Encoding.Default);
                Helper.Replace(ref _bytes, "_msg_", s2, 1);
                Http.Length(ref _bytes);
                _Socket.Send(_bytes.Save());
                string s3 = Http.ReadHttp(_Socket).ToStr().Save();
                Match m = Regex.Match(s3, "вить сообщение (.+?), поскольку он");
                if (m.Success)
                {
                    _ListA2.Add(HttpUtility.HtmlDecode(m.Groups[1].Value).Trace());
                    _ListA2.Flush();
                }
                else
                {
                    foreach (string s in _ListA)
                        if (!_ListA2.Contains(s)) _ListA2.Add(s);
                    _ListA2.Flush();
                    break;
                }                
            }
        }
        Socket _Socket;
        private void Populate()
        {
            Connect();
            for (; i < 551; i += 1)
            {

                byte[] _bytes = File.ReadAllBytes("1 Sended.html");
                Helper.Replace(ref _bytes, "_page_", "/forum/memberlist.php?&order=asc&sort=username&page=" + i, 1);
                _Socket.Send(_bytes.Save());
                string s = Http.ReadHttp(_Socket).ToStr().Save();
                MatchCollection ms = Regex.Matches(s, @"<a href=""member.php\?u=\d+"">(.+?)</a>");
                ms.Count.Trace();
                foreach (Match m in ms)
                {
                    string s2 = HttpUtility.HtmlDecode(m.Groups[1].Value);
                    if ((!_ListA.Contains(s2)).Trace()) _ListA.Add(s2);
                }
                "".Trace();
                _ListA.Flush();
            }
        }

        private void Connect()
        {
            TcpClient _TcpClient = new TcpClient("www.flasher.ru", 80);
            _Socket = _TcpClient.Client;
        }

        private static void Join()
        {
            string.Join(";", File.ReadAllLines("list.txt").ToArray()).Trace();
        }

    }
}
