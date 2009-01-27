using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Sockets;
using System.IO;
using doru;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading;
using System.Diagnostics;

namespace Vingrad.ru
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.Default = Encoding.UTF8;
            Spammer3.Setup();
            //string s = _message;
            new Program();
        }        
        public Program()
        {
            _sendedList = new ListA("postedlist.txt");
            for (int i = 0; i < 5; i++)
                new Client().StartSpammAsync();

            for (int i = 0; i < 1; i++)
                new Client().StartPopulateAsync();

            while (true)
            {
                Thread.Sleep(5000);
                _sendedList.Flush();
                Trace.WriteLine("flushed");
            }
        }
        public static ListA _sendedList;
        public static List<string> _list = new List<string>() { "dorumon" };            
        public static intA i = new intA("i.txt");
        private static string GetUser()
        {
            lock ("spamm")
            {
                string user = null;
                try
                {
                    user = (from u in _list where !_sendedList.Contains(u) select u).FirstOrDefault();
                }
                catch { }
                if (user != null)
                {
                    _sendedList.Add(user);
                    _list.Remove(user);
                }
                return user;
            }
        }
        public class Client
        {            
            Socket _Socket;
            public Client()
            {
                Connect();
            }
            public void StartSpammAsync()
            {
                new Thread(StartSpamm).StartBackground("spamm");
            }
            public void StartPopulateAsync()
            {
                new Thread(StartPopulate).StartBackground("populate");
            }
            private void StartSpamm()
            {                
                while (true)
                {
                    string user = GetUser();
                    try
                    {                        
                        if (user != null)
                        {                            
                            ("sending to "+user).Trace();                            
                            byte[] _bytes = File.ReadAllBytes("1 Sended Post.html");
                            Helper.Replace(ref _bytes, "_title_", HttpUtility.UrlEncode("привет", Encoding.Default), 1);
                            Helper.Replace(ref _bytes, "_name_", user, 1);
                            Helper.Replace(ref _bytes, "_message_", HttpUtility.UrlEncode(_message, Encoding.Default), 1);                            
                            Http.Length(ref _bytes);
                            _Socket.Send(_bytes);
                            string s2 = Http.ReadHttp(_Socket).ToStr().Save("spamm");                            
                            ("succes to " + user).Trace();
                        }
                    }
                    catch (IOException) { ("failed to " + user).Trace(); Connect(); }
                    Thread.Sleep(50);
                }                
            }
            private void StartPopulate()
            {
                for (; ; )
                {
                    if (_list.Count < 10)
                    {
                        i.i += 50;
                        try
                        {
                            byte[] _bytes = File.ReadAllBytes("1 Sended.html");
                            Helper.Replace(ref _bytes, "_page_", String.Format(@"/forum/act-Members/name_box/all/max_results-50/filter-3/sort_order-desc/sort_key-posts/{0}.html", i.i), 1);
                            _Socket.Send(_bytes);
                            string s = Http.ReadHttp(_Socket).ToStr().Save("populate");

                            MatchCollection ms = Regex.Matches(s, @"<a href=""/users/.+?"">(.+?)</a>");
                            int c = 0;
                            foreach (Match m in ms)
                            {

                                string s2 = HttpUtility.HtmlDecode(m.Groups[1].Value);
                                if (!_list.Contains(s2) && !_sendedList.Contains(s2))
                                {
                                    c++;
                                    _list.Add(s2);
                                }
                            }
                            ("populated " + c).Trace();
                        }
                        catch (IOException) { Connect(); }
                    }
                    Thread.Sleep(50);
                }
            }

            private void Connect()
            {
                if (_Socket != null) _Socket.Close();
                TcpClient _TcpClient = new TcpClient("forum.vingrad.ru", 80);
                _Socket = _TcpClient.Client;
            }
        }
        public static string _message { get { return File.ReadAllText("../message.txt",Encoding.Default2); } }
    }
}
