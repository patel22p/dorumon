using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;


public class Vkontakte :Base
{

    public void Reconnect()
    {
        if (nw != null) nw.Close();
        nw = new NetworkStream(new TcpClient("vkontakte.ru", 80).Client);
    }
    NetworkStream nw;
    int ap_id = 1932732; 
    public Vkontakte()
    {
        vkontakte = this;
    }
    string password = "";
    string login = "";
    void Start() { }
    IEnumerator ie;
    public void Start(string login, string password)
    {
        
        this.password = WWW.EscapeURL(password);
        this.login = WWW.EscapeURL(login);
        ie = Connect().GetEnumerator();
        ie.MoveNext();
    }

    string secret { get { return PlayerPrefs.GetString("secret"); } set { PlayerPrefs.SetString("secret",value); } }
    string mid { get { return PlayerPrefs.GetString("mid"); } set { PlayerPrefs.SetString("mid", value); } }
    string sid { get { return PlayerPrefs.GetString("sid"); } set { PlayerPrefs.SetString("sid", value); } }
    internal int userid; //{ get { return PlayerPrefs.GetInt("userid"); } set { PlayerPrefs.SetInt("userid", value); } }
    private IEnumerable Connect()
    {
        if (userid == 0)
        {
            print("vkontakte started" + login + password);
            Write1("http://vkontakte.ru/login.php?app=1932732&layout=popup&type=browser");            
            yield return 1;

            string apphash = data;
            print("success1");
            apphash = Regex.Match(apphash, "var app_hash = '(.*?)';").Groups[1].Value;
            string s2 = H.Replace("act=login&app=1932732&app_hash=(apphash)&vk=&captcha_sid=&captcha_key=&email=(email)&pass=(pass)&expire=0&permanent=1"
                , "(apphash)", apphash, "(email)", login, "(pass)", password);                        
            Write1("http://login.vk.com", H.ToBytes(s2));
            yield return 1;

            string r1 = data;
            print("success2");
            string passkey = Regex.Match(r1, @"name='s' value='(.*?)'").Groups[1].Value;
            string apphash2 = Regex.Match(r1, "name=\"app_hash\" value=\"(.*?)\"").Groups[1].Value;
            string s4 = H.Replace("s=(passkey)&act=auth_result&m=4&permanent=&expire=1&app=1932732&app_hash=(apphash)"
                , "(apphash)", apphash2, "(passkey)", passkey);
            Write1("http://vkontakte.ru/login.php", H.ToBytes(s4));
            yield return 1;

            string result = data;
            print("success3");
            Match match = Regex.Match(result, "\"mid\":(.*?),\"sid\":\"(.*?)\",\"secret\":\"(.*?)\"");
            print(match.Success.ToString());
            mid = match.Groups[1].Value;
            sid = match.Groups[2].Value;
            secret = match.Groups[3].Value;            
            userid = int.Parse(GetGlobalVariable(1280));            
        }
        yield return 1;     
    }
    string data;
    WWW www;
    void Write1(string url,byte[] wwwform)
    {
        www = new WWW(url, wwwform);                        
    }
    void Update()
    {
        if (www != null && www.isDone)
        {
            data = H.ToStr(www.bytes);
            www = null;
            ie.MoveNext();
        }        
    }
    void Write1(string url)
    {        
        www = new WWW(url);                
    }
    public User GetUserInfo(int userid)
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getProfiles"},
                        new string[]{"fields","nickname,first_name,last_name,photo"},
                        new string[]{"v", "3.0"},
                        new string[]{"uids",userid.ToString()}
                    });

        WWW www = new WWW(sendfunc);
        while (!www.isDone) System.Threading.Thread.Sleep(100);

        string res = H.ToStr(www.bytes);
        User user = new User();
        user.id = userid;
        user.first_name = Regex.Match(res, "<first_name>(.*?)</first_name>").Groups[1].Value;
        user.last_name = Regex.Match(res, "<last_name>(.*?)</last_name>").Groups[1].Value;
        user.nick = Regex.Match(res, "<nickname>(.*?)</nickname>").Groups[1].Value;
        user.avatar = Regex.Match(res, "<photo>(.*?)</photo>").Groups[1].Value;
        return user;
    }        


    public string GetGlobalVariable(int key)
    {

        string sendfunc = H.Replace(S.s5, "(url)", SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getVariable"},
                        new string[]{"v", "3.0"},                        
                        new string[]{"key",key.ToString()} ,
                        new string[]{"test_mode","1"} ,
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

    public string GetVariable(int key)
    {
        string sendfunc = H.Replace(S.s5, "(url)", SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getVariable"},
                        new string[]{"v", "3.0"},
                        new string[]{"user_id", userid.ToString()},                        
                        new string[]{"key",(1024 +key).ToString()} ,
                        new string[]{"test_mode","1"} ,
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

        string md5 = mid.ToString();
        string url = "http://api.vkontakte.ru/api.php?";
        foreach (KeyValuePair<string, string> key in list)
            md5 += key.Key + "=" + key.Value;
        md5 += secret;
        string sig = H.getMd5Hash(md5);
        list.Add("sid", sid);
        list.Add("sig", sig);
        foreach (KeyValuePair<string, string> key in list)
            url += key.Key + "=" + key.Value + "&";
        url = url.TrimEnd(new char[] { '&' });
        return url;
    }


    

}
