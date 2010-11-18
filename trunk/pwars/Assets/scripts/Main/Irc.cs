using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
public class Irc : MonoBehaviour
{
    public string[] users;
    public string msg;
    string host = "irc.quakenet.org";
    internal int port = 6667;
    internal bool success;
    Socket _Socket;
    NetworkStream _NetworkStream;
    StreamWriter sw;
    StreamReader sr;
    Thread _thread;
    public string ircNick;//{ get { return nick; } }
    string user = "physxwarsuser1";
    string user2 = "localhost";
    string about = "about";
    string channel = "PhysxWars";
    void Start()
    {
        _thread = new Thread(StartThread);
        _thread.IsBackground = true;
        _thread.Start();
    }
    
    
    void OnApplicationQuit()
    {
        if (_Socket != null)
            _Socket.Close();
        if (_thread != null)
            _thread.Abort();

    }
    public void SendIrcMessage(string s)
    {
        OnMessage(ircNick + ": " + s);
        sw.WriteLine("PRIVMSG #" + channel + " :" + s);
    }

    void StartThread()
    {
        try
        {
            Connect();
            Write(string.Format("NICK {0}", ircNick));
            Write("USER " + user + " " + user2 + " server :" + about);
            while (true)
            {
                string s = log(sr.ReadLine());
                if (s == null || s == "") throw new Exception("string is null");
                if (Regex.Match(s, @":.+? 005").Success && !success)
                {
                    Write("JOIN #" + channel);
                    OnMessage("Connected");
                    success = true;
                }
                Match m;
                if ((m = Regex.Match(s, @"PING \:(\w+)", RegexOptions.IgnoreCase)).Success)
                    Write(("PONG :" + m.Groups[1]));
                if ((m = Regex.Match(s, @"\:.+? 353 .+? = #(.+?) \:(.+)")).Success)
                {
                    users = m.Groups[2].Value.Trim().Split(' ');
                    success = true;
                }
                if ((m = Regex.Match(s, @"\:(.+?)!.*? PRIVMSG .*? \:(.*)")).Success)
                    OnMessage(m.Groups[1].Value + ": " + m.Groups[2].Value);
                if ((m = Regex.Match(s, @"\:(.+?)!.*? PART")).Success)
                {
                    users = users.Where(a => !a.Contains(m.Groups[1].Value)).ToArray();
                    OnMessage(m.Groups[1].Value + " leaved");
                }
                if ((m = Regex.Match(s, @"\:(.+?)!.*? JOIN")).Success)
                {
                    users = users.Union(new[] { m.Groups[1].Value }).ToArray();
                    OnMessage(m.Groups[1].Value + " joined");
                }                
            }
        }
        catch (Exception e)
        {
            print(e);
            OnMessage("Disconnected:" + e);
        }
    }
    void OnMessage(string m)
    {
        msg += m+"\r\n";
    }

    void Write(string s)
    {
        sw.WriteLine(log(s));
    }
    private void Connect()
    {
        OnMessage("Connecting");
        _Socket = new TcpClient(host, port).Client;
        _NetworkStream = new NetworkStream(_Socket);
        sw = new StreamWriter(_NetworkStream, Encoding.GetEncoding(1251));
        sr = new StreamReader(_NetworkStream, Encoding.GetEncoding(1251));
        
        sw.AutoFlush = true;
    }
    StringBuilder sb = new StringBuilder();
    public T log<T>(T t)
    {
        sb.Append(t);
        return t;
    }


}
