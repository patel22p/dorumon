using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;

public class Test : MonoBehaviour
{
    string d = "mpme";
    // Use this for initialization
    string console="";
    void Start() {
        Application.RegisterLogCallback(LogCallback1);
        NewMethod();
        //WebClient asd = new WebClient();
        //d =asd.DownloadString("http://gooogle.com");
	}

    private void NewMethod()
    {
        
        TcpClient tcp = new TcpClient("yiff.ru", 80);
        NetworkStream nw = new NetworkStream(tcp.Client);
        H.Write(nw, H.ToBytes(s));
        byte[] bts = Http.ReadHttp(nw);
        d = H.ToStr(bts);
    }
    
    string s = @"GET / HTTP/1.1
User-Agent: Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1
Host: www.yiff.ru
Accept: text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1
Accept-Language: ru,en;q=0.9,ru-RU;q=0.8
Accept-Charset: iso-8859-1, utf-8, utf-16, *;q=0.1
Accept-Encoding: deflate, gzip, x-gzip, identity, *;q=0
Referer: http://www.yiff.ru/forum.yiff
Connection: Keep-Alive, TE
TE: deflate, gzip, chunked, identity, trailers

";

    void LogCallback1(string condition, string stackTrace, LogType type)
    {
        console += type + condition;
    }
    
    void OnGUI()
    {
        GUILayout.TextField(console);
        GUILayout.TextField(d);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
