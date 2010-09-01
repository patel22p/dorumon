using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.Threading;


public class Vkontakte : Base
{
    internal enum Status { disconnected, connecting, connected }
    internal Status _Status;
    public void Reconnect()
    {
        if (nw != null) nw.Close();
        nw = new NetworkStream(new TcpClient("vkontakte.ru", 80).Client);
    }
    public user _User;
    NetworkStream nw;
    int ap_id = 1935303;
    //string password = "";
    //string login = "";
    void Start() { _vk = this; }
    //public void Start(string login, string password)
    //{
    //    _Status = Status.connecting;
    //    this.password = WWW.EscapeURL(password);
    //    this.login = WWW.EscapeURL(login);
    //    new Thread(Connect).Start();
    //}
    public void Start(string url)
    {
        _Status = Status.connecting;
        new Thread(delegate()
            {
                try
                {
                    //http://vkontakte.ru/api/login_success.html#session={"expire":"0","mid":"9684567","secret":"903de5bc49","sid":"ff3e1b4b90275dde244bd99653c441b1073da741537f5f6b625261900e"}
                    url = WWW.UnEscapeURL(url);                    
                    mid = Regex.Match(url, @"""mid"":""?(\d*)").Groups[1].Value;
                    sid = Regex.Match(url, @"""sid"":""(\w*)").Groups[1].Value;  
                    secret = Regex.Match(url, @"""secret"":""(\w*)").Groups[1].Value;                    
                    print(mid + "," + secret + "," + sid);
                    GetUserData(); 
                }
                catch { _Status = Status.disconnected; }
            }).Start();
    }
    string secret; //{ get { return PlayerPrefs.GetString("secret"); } set { PlayerPrefs.SetString("secret", value); } }
    string mid; //{ get { return PlayerPrefs.GetString("mid"); } set { PlayerPrefs.SetString("mid", value); } }
    string sid; //{ get { return PlayerPrefs.GetString("sid"); } set { PlayerPrefs.SetString("sid", value); } }
    int userid;//{ get { return PlayerPrefs.GetInt("userid"); } set { PlayerPrefs.SetInt("userid", value); } }
    //private void Connect()
    //{
    //    try
    //    {
    //        if (userid == 0)
    //        {
    //            print("vkontakte started" + login + password);
    //            string apphash = Write(S.s1);
    //            print("success1");
    //            apphash = Regex.Match(apphash, "var app_hash = '(.*?)';").Groups[1].Value;
    //            string s2 = H.Replace(S.s2, "(apphash)", apphash, "(email)", login, "(pass)", password);
    //            string r1 = Write(s2);
    //            print("success2");
    //            string passkey = Regex.Match(r1, @"name='s' value='(.*?)'").Groups[1].Value;
    //            string apphash2 = Regex.Match(r1, "name=\"app_hash\" value=\"(.*?)\"").Groups[1].Value;
    //            string s4 = H.Replace(S.s4, "(apphash)", apphash2, "(passkey)", passkey);
    //            string result = Write(s4);
    //            print("success3");
    //            print(result);
    //            Match match = Regex.Match(result, "\"mid\":(.*?),\"sid\":\"(.*?)\",\"secret\":\"(.*?)\"");
    //            print(match.Success.ToString());
    //            mid = match.Groups[1].Value;
    //            sid = match.Groups[2].Value;
    //            secret = match.Groups[3].Value;
    //        }
    //        GetUserData();
    //    }
    //    catch
    //    {
    //        _Status = Status.disconnected;
    //        print("Could not connect");
    //    }
        
    //}

    private void GetUserData()
    {
        userid = int.Parse(GetGlobalVariable(1280));
        _User = GetUserInfo(userid);
        GetFriends();
        _Status = Status.connected;
        print("success");
        _TimerA.AddMethod(_Vkontakte.onConnected);
    }



    private user GetUserInfo(int userid)
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getProfiles"},
                        new string[]{"fields","nickname,first_name,last_name,photo"},                        
                        new string[]{"uids",userid.ToString()}
                    });
        
        string res = Write(H.Replace(S.s5, "(url)", sendfunc));
        print(res);
        user user = new user();
        user.uid = userid;
        user.first_name = Regex.Match(res, "<first_name>(.*?)</first_name>").Groups[1].Value;
        user.last_name = Regex.Match(res, "<last_name>(.*?)</last_name>").Groups[1].Value;
        user.nick = Regex.Match(res, "<nickname>(.*?)</nickname>").Groups[1].Value;
        user.photo = Regex.Match(res, "<photo>(.*?)</photo>").Groups[1].Value;
        return user;
    }


    private string GetGlobalVariable(int key)
    {

        string sendfunc = H.Replace(S.s5, "(url)", SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getVariable"},                                            
                        new string[]{"key",key.ToString()},
                        new string[]{"test_mode","2"} 
                    }));
        string res = Write(sendfunc);
        print(res);
        return Regex.Match(res, "<response>(.*?)</response>").Groups[1].Value;
    }

    private string Write(string sendfunc)
    {
        Reconnect();
        H.Write(nw, sendfunc);
        string res = H.ToStr(Http.ReadHttp(nw));
        return res;
    }
    private void GetFriends()
    {
        string sendfunc = H.Replace(S.s5, "(url)", SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","friends.get"},                        
                        new string[]{"fields","nickname,first_name,last_name,photo,online,uid"}                           
                    }));

        string res = Write(sendfunc);
        print(res);

        res = Regex.Match(res, "<response list=\"true\">(.*)</response>", RegexOptions.Singleline).Groups[1].Value.Trim();
        res = "<response><users>" + res + "</users></response>";
        print(res);
        Add((response)xml.Deserialize(new StringReader(res)));
    }
    void Add(response resp)
    {
        
        lock ("vk")
            responses.Add(resp);
    }
    public List<response> GetResponses()
    {
        lock ("vk")
        {
            if (responses.Count == 0) return responses;
            List<response> ret = responses;
            responses = new List<response>();
            return ret;
        }
    }

    XmlSerializer xml = new XmlSerializer(typeof(response), new Type[] { typeof(user), typeof(message_info) });
   
    public class response
    {
        public List<user> users = new List<user>();
        public List<message_info> messages = new List<message_info>();
    }

    public class message_info
    {
        public int uid;
        public int time;
        public string user_name;
        public string message;

    }

    public class user
    {
        [XmlIgnoreAttribute]
        public Texture2D texture;        
        public string first_name;
        public string last_name;
        public string nickname;
        public string nick { get { return nickname == "" ? first_name + " " + last_name : nickname; } set { nickname = value; } }
        public string photo;
        public int uid;
        public bool online;
    }

    public void SendMsg(string message)
    {        

        string sendfunc = H.Replace(S.s5, "(url)", SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    {                         
                        new string[]{"method","sendMessage"}, 
                        new string[]{"message", WWW.EscapeURL(message)}    ,
                        new string[]{"test_mode","2"} ,                    
                    }));
        new Thread(delegate()
        {
            string res = Write(sendfunc);
            print(res);
        }).Start();
    }
    List<response> responses = new List<response>();

    

    public void GetMessages()
    {
        string sendfunc = H.Replace(S.s5, "(url)", SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getMessages"}       ,
                        new string[]{"test_mode","2"} ,                        
                    }));
        new Thread(delegate()
            {
                try
                {
                    string res = Write(sendfunc);                    
                    res = Regex.Match(res, "<response list=\"true\">(.*)</response>", RegexOptions.Singleline).Groups[1].Value;
                    res = "<response><messages>" + res + "</messages></response>";
                    Add((response)xml.Deserialize(new StringReader(res)));
                }
                catch { }
            }).Start();
    }
    public void SetStatus(string text)
    {
        string sendfunc = H.Replace(S.s5, "(url)", SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","activity.set"},                        
                        new string[]{"text", text}                        
                    }));

        string resp = Write(sendfunc);
        print(resp);
    }
    

    public string GetVariable(int key)
    {
        string sendfunc = H.Replace(S.s5, "(url)", SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getVariable"},                        
                        new string[]{"user_id", userid.ToString()},                        
                        new string[]{"key",(1024 +key).ToString()} ,
                        new string[]{"test_mode","2"} ,
                    }));

        string res = Write(sendfunc);
        print(res);
        return Regex.Match(res, "<response>(.*?)</response>").Groups[1].Value;
    }
    string SendFunction(int mid, int ap_id, string sid, string secret, params string[][] strs)
    {
        SortedList<string, string> list = new SortedList<string, string>();
        foreach (string[] ss in strs)
            list.Add(ss[0], ss[1]);
        list.Add("api_id", ap_id.ToString());
        list.Add("format", "XML");
        list.Add("v", "3.0");
        string md5 = mid.ToString();
        string url = "http://api.vkontakte.ru/api.php?";
        foreach (KeyValuePair<string, string> key in list)
            md5 += key.Key + "=" + key.Value;
        md5 += secret;
        string sig = H.getMd5Hash(md5);
        list.Add("sid", sid);
        list.Add("sig", sig);
        foreach (KeyValuePair<string, string> key in list)
            url += key.Key + "=" + WWW.EscapeURL(key.Value) + "&";
        url = url.TrimEnd(new char[] { '&' });
        return url;
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
        ws.Add(this);
    }
    public static void Update()
    {
        List<WWW2> remove = new List<WWW2>();
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
