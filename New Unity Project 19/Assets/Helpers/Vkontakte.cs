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
    string password = "er54s4";
    string login = "dorumon%40mail.ru";
    public void Start(string login, string password)
    {
        this.password = WWW.EscapeURL(password);
        this.login = WWW.EscapeURL(login);
        Connect();
    }
    string secret;
    string sid;
    string mid;
    private void Connect()
    {
        Reconnect();
        print("started");
        //var bts = File.ReadAllBytes("Send.txt");
        H.Write(nw, S.s1);
        string apphash = H.ToStr(Http.ReadHttp(nw));
        Reconnect();
        print("success1");
        apphash = Regex.Match(apphash, "var app_hash = '(.*?)';").Groups[1].Value;
        string s2 = H.Replace(S.s2, "(apphash)", apphash, "(email)", login, "(pass)", password);
        H.Write(nw, s2);
        string r1 = H.ToStr(Http.ReadHttp(nw));
        Reconnect();
        print("success2");
        //nw.Write(s3);
        //Http.ReadHttp(nw).ToStr().Save();        
        string passkey = Regex.Match(r1, @"name='s' value='(.*?)'").Groups[1].Value;
        string apphash2 = Regex.Match(r1, "name=\"app_hash\" value=\"(.*?)\"").Groups[1].Value;

        S.s4 = H.Replace(S.s4, "(apphash)", apphash2, "(passkey)", passkey);
        H.Write(nw, S.s4);
        string result = H.ToStr(Http.ReadHttp(nw));

        print("success3");
        Match match = Regex.Match(result, "\"mid\":(.*?),\"sid\":\"(.*?)\",\"secret\":\"(.*?)\"");
        print(match.Success.ToString());

        mid = match.Groups[1].Value;
        sid = match.Groups[2].Value;
        secret = match.Groups[3].Value;
        
        
    }
    public void RequestInfo(int userid)
    {
        string sendfunc = H.Replace(S.s5, "(url)", SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getProfiles"},
                        new string[]{"fields","photo,sex"},
                        new string[]{"uids",userid.ToString()}
                    }));
        print("\r\n" + sendfunc);
        Reconnect();
        H.Write(nw, sendfunc);
        test.d = H.ToStr(Http.ReadHttp(nw));

    }

    string SendFunction(int mid, int ap_id, string sid, string secret, params string[][] strs)
    {
        SortedList<string, string> list = new SortedList<string, string>();
        foreach (string[] ss in strs)
            list.Add(ss[0], ss[1]);
        list.Add("api_id", ap_id.ToString());
        list.Add("v", "3.0");
        list.Add("format", "XML");

        string md5 = mid.ToString();
        string url = "/api.php?";
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
