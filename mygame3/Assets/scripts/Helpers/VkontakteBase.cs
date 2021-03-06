﻿using UnityEngine;
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

public partial class z0Vk
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
        public z0Vk.status st = new status();
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
public class WWW2
{
    static List<WWW2> ws = new List<WWW2>();
    public delegate void Action(WWW2 w);
    public event Action done;
    public WWW www;
    public WWW2(string s)        
    {
        www = new WWW(s);
        lock ("www")
            ws.Add(this);
    }
    public static void Update()
    {
        List<WWW2> remove = new List<WWW2>();
        lock ("www")
            foreach (WWW2 w in ws)
            {
                if (w.www.isDone)
                {
                    w.done(w);
                    w.www.Dispose();
                    remove.Add(w);
                }
            }
        foreach (WWW2 w in remove)
            ws.Remove(w);
    }
}
