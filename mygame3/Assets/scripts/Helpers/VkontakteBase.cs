using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Threading;
using System;

public static class S
{
    //    public static string s4 = @"POST /login.php HTTP/1.0
    //Host: vkontakte.ru
    //Connection: Keep-Alive
    //User-Agent: Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.127 Safari/533.4
    //Referer: http://login.vk.com/
    //Content-Length: 138
    //Cache-Control: max-age=0
    //Origin: http://login.vk.com
    //Content-Type: application/x-www-form-urlencoded
    //Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
    //Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4
    //Accept-Charset: windows-1251,utf-8;q=0.7,*;q=0.3
    //Cookie: remixchk=5; remixsid=nonenone
    //
    //s=(passkey)&act=auth_result&m=4&permanent=&expire=1&app=1932732&app_hash=(apphash)";


    //    public static string s2 = @"POST / HTTP/1.0
    //Host: login.vk.com
    //Connection: Keep-Alive
    //User-Agent: Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.127 Safari/533.4
    //Referer: http://vkontakte.ru/login.php?app=1932732&layout=popup&type=browser
    //Content-Length: 129
    //Cache-Control: max-age=0
    //Origin: http://vkontakte.ru
    //Content-Type: application/x-www-form-urlencoded
    //Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
    //Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4
    //Accept-Charset: windows-1251,utf-8;q=0.7,*;q=0.3
    //
    //act=login&app=1932732&app_hash=(apphash)&vk=&captcha_sid=&captcha_key=&email=(email)&pass=(pass)&expire=0&permanent=1&addMember=1&app_settings_2=1&app_settings_4=1&app_settings_8=1&app_settings_16=1&app_settings_32=1&app_settings_64=1&app_settings_128=1&app_settings_256=1&app_settings_512=1&app_settings_1024=1&app_settings_2048=1&app_settings_8192=1";


    //    public static string s1 = @"GET /login.php?app=1932732&layout=popup&type=browser HTTP/1.1
    //Host: vkontakte.ru
    //Connection: keep-alive
    //User-Agent: Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.127 Safari/533.4
    //Cache-Control: max-age=0
    //Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
    //Accept-Encoding: gzip,deflate,sdch
    //Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4
    //Accept-Charset: windows-1251,utf-8;q=0.7,*;q=0.3
    //Cookie: remixchk=5; remixsid=nonenone

    //";

    public static string s5 = @"GET (url) HTTP/1.1
Host: api.vkontakte.ru
Connection: keep-alive
User-Agent: Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.127 Safari/533.4
Cache-Control: max-age=0
Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
Accept-Encoding: gzip,deflate,sdch
Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4
Accept-Charset: windows-1251,utf-8;q=0.7,*;q=0.3
Cookie: remixchk=5; remixsid=nonenone

";
}

public partial class Vk
{ 
    
    public class response
    {                
        public List<status> statuses = new List<status>();
        public List<user> users = new List<user>();
        public List<message_info> messages = new List<message_info>();
        public List<message> personal = new List<message>();
        
    }
    public class status
    {
        public int timestamp;
        public int uid;
        public string text;
    }
    public class message_info
    {
        public int user_id;
        public int time;
        public string user_name;
        public string message;

    }
    public class message
    {
        public int date;
        public int mid;
        public int uid;
        public int read_state;
        public string title;
        public string body;
    }
    public class user
    { 
        [XmlIgnoreAttribute]
        public Texture2D texture;
        //public override int GetHashCode()
        //{
        //    return nwid.GetHashCode();
        //}
        
        //public override bool Equals(object obj)
        //{
        //    return ((user)obj).nwid == this.nwid;
        //}
        public float loaded;
        public string first_name = "";
        public string last_name = "";
        public string nickname = "";
        public Vk.status st = new status();
        public string nick { get { return nickname == "" ? first_name + " " + last_name : nickname; } set { nickname = value; } }
        public string photo ="";
        public int uid;
        public NetworkPlayer nwid;        
        public int totalzombiekills;
        public int totalzombiedeaths;        
        public int totalkills;
        public int totaldeaths;

        public int deaths;
        public bool online;
        public bool installed;
        public int frags;
        public int ping;
        public int fps;        
        public Team team;
    }
    
    
}
public class WWW2 : WWW
{
    static List<WWW2> ws = new List<WWW2>();
    public delegate void Action(WWW2 w);
    public event Action done;
    public WWW2(string s)
        : base(s)
    {
        lock ("www")
            ws.Add(this);
    }
    public static void Update()
    {
        List<WWW2> remove = new List<WWW2>();
        lock ("www")
            foreach (WWW2 w in ws)
            {
                if (w.isDone)
                {
                    w.done(w);
                    w.Dispose();
                    remove.Add(w);
                }
            }
        foreach (WWW2 w in remove)
            ws.Remove(w);
    }
}
