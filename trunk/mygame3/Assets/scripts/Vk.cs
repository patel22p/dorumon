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


public partial class Vk : Base
{
    internal enum Status { disconnected, connecting, connected }
    internal enum Keys { playerstats = 1300 }
    internal Status _Status;
    public void Reconnect()
    {
        if (nw != null) nw.Close();
        nw = new NetworkStream(new TcpClient("vkontakte.ru", 80).Client);
    }
    
    NetworkStream nw;
    int ap_id = 1935303;

    void Start() { _vk = this; }

    public void Start(string url)
    {
        print("vk start");
        _Status = Status.connecting;
        newThread(delegate()
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
                catch (System.Exception e) { _Status = Status.disconnected; print(e); }
            });
    }
    string secret; //{ get { return PlayerPrefs.GetString("secret"); } set { PlayerPrefs.SetString("secret", value); } }
    string mid; //{ get { return PlayerPrefs.GetString("mid"); } set { PlayerPrefs.SetString("mid", value); } }
    string sid; //{ get { return PlayerPrefs.GetString("sid"); } set { PlayerPrefs.SetString("sid", value); } }
    int userid;//{ get { return PlayerPrefs.GetInt("userid"); } set { PlayerPrefs.SetInt("userid", value); } }

    public DateTime time;
    void Update()
    {
        if (_Status == Status.connected)
            time = time.AddSeconds(Time.deltaTime);

    }
    private void GetUserData()
    {
        int i = 500;
        userid = int.Parse(GetGlobalVariable(1280));
        Thread.Sleep(i);
        localuser = GetUserInfo(userid);
        Thread.Sleep(i);
        time = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(int.Parse(GetGlobalVariable(0)));
        SetUnixTime();
        Thread.Sleep(i);
        GetFriends();
        Thread.Sleep(i);
        GetChatMessages(10);
        Thread.Sleep(i);
        GetScoreBoard();
        Thread.Sleep(i);
        GetAppUsers();
        Thread.Sleep(i);
        GetStats();
        Thread.Sleep(i);
        SetStatus("In PhysxWars GameLabby " + time.ToShortTimeString());        
        _Status = Status.connected;
        printC("success");
        _TimerA.AddMethod(_Vkontakte.onVkConnected);        
        friends.Add(localuser.uid, localuser);
        new WWW2(localuser.photo).done += delegate(WWW2 www)
        {
            print("loaded texture");
            localuser.texture = www.texture;
            DontDestroyOnLoad(localuser.texture);
        };

    }

    private void GetStats()
    {
        string[] ss = GetLocalVariable((int)Keys.playerstats).Split(',');
        if (ss.Length != 4)
            print("empty stats" + ss.Length);
        else
        {
            localuser.totalkills = int.Parse(ss[0]);
            localuser.totaldeaths = int.Parse(ss[1]);
            localuser.totalzombiekills= int.Parse(ss[2]);
            localuser.totalzombiedeaths= int.Parse(ss[3]);
            print("stats set" + localuser.totalkills);
        }
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

        string res = Write(sendfunc);
        
        user user = new user();
        user.uid = userid;
        user.first_name = Regex.Match(res, "<first_name>(.*?)</first_name>").Groups[1].Value;
        user.last_name = Regex.Match(res, "<last_name>(.*?)</last_name>").Groups[1].Value;
        user.nick = Regex.Match(res, "<nickname>(.*?)</nickname>").Groups[1].Value;
        user.photo = Regex.Match(res, "<photo>(.*?)</photo>").Groups[1].Value;
        return user;
    }
    public string GetLocalVariable(int key)
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getVariable"},                        
                        new string[]{"user_id", userid.ToString()},                        
                        new string[]{"key",key.ToString()} ,
                        new string[]{"test_mode","2"} ,
                    });

        string res = Write(sendfunc);
        
        return Regex.Match(res, "<response>(.*?)</response>").Groups[1].Value;
    }
    List<Thread> threads = new List<Thread>();
    void newThread(ThreadStart  a)
    {
        Thread t = new Thread(a);
        t.IsBackground = true;
        threads.Add(t);
        t.Start();
    }
    void OnApplicationQuit()
    {
        foreach (Thread t in threads)
            if (t.IsAlive)
                t.Abort();
    }

    public void SetLocalVariable(int key, string value)
    {

        newThread(delegate()
            {
                string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                            new string[][]
                    { 
                        new string[]{"method","putVariable"},                                            
                        new string[]{"key",key.ToString()},
                        new string[]{"value",value},
                        new string[]{"user_id", userid.ToString()},                        
                        new string[]{"test_mode","2"} 
                    });
                string res = Write(sendfunc);
                
            });
    }
    List<score_info> scores = new List<score_info>();
    void GetScoreBoard()
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getHighScores"},                                                
                    });

        string res = Write(sendfunc);
        res = Regex.Match(res, "<response list=\"true\">(.*)</response>", RegexOptions.Singleline).Groups[1].Value.Trim();
        res = "<response><scores>" + res + "</scores></response>";
        response r = (response)xml.Deserialize(new StringReader(res));
        scores = r.scores;
        foreach (score_info s in scores)
        {
            if (friends.ContainsKey(s.user_id))
                friends[s.user_id].stats = s.score;
        }
        print("get scores" + r.scores.Count);
        

    }
    void GetGlobalVariables(int key, int count)
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getVariables"},                                                                    
                        new string[]{"test_mode","2"},
                        new string[]{"count","32"},
                        new string[]{"key","100"},
                    });

        string res = Write(sendfunc);

    }

    internal void SendWallMsg(int userid, string text)
    {
        newThread(delegate()
        {
            string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                        new string[][]
                    { 
                        new string[]{"method","wall.post"},                                            
                        new string[]{"message",text},                                                                    
                        new string[]{"owner_id", userid.ToString()},                        
                        
                    });
            string res = Write(sendfunc);
            print("sendwall "+res);
        });
    }    
    private string GetGlobalVariable(int key)
    {

        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getVariable"},                                            
                        new string[]{"key",key.ToString()},
                        new string[]{"test_mode","2"} 
                    });
        string res = Write(sendfunc);
        print("get global variable"+key);
        return Regex.Match(res, "<response>(.*?)</response>").Groups[1].Value;
    }

    private string Write(string sendfunc)
    {
        Reconnect();
        H.Write(nw, sendfunc);
        string res = H.ToStr(Http.ReadHttp(nw));
        if (res.Contains("<error>")) throw new Exception("Vkontakte Error " + res);
        return res;
    }
    public Dictionary<int, user> friends = new Dictionary<int, user>();

    //

    private void GetAppUsers()
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","friends.getAppUsers"},                                                
                    });
        string res = Write(sendfunc);
        res = Regex.Match(res, "<response list=\"true\">(.*)</response>", RegexOptions.Singleline).Groups[1].Value.Trim();
        res = "<response><appusers>" + res + "</appusers></response>";
        response r = (response)xml.Deserialize(new StringReader(res));
        print("appusers" + r.appusers.Count);
        foreach (int id in r.appusers)
        {
            if (friends.ContainsKey(id))
                friends[id].installed = true;
        }
    }

    
    private void GetFriends()
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","friends.get"},                        
                        new string[]{"fields","nickname,first_name,last_name,photo,online,uid"}                           
                    });
        
        string res = Write(sendfunc);        

        res = Regex.Match(res, "<response list=\"true\">(.*)</response>", RegexOptions.Singleline).Groups[1].Value.Trim();
        res = "<response><users>" + res + "</users></response>";
        response r = (response)xml.Deserialize(new StringReader(res));
        print("get friends " + r.users.Count);

        foreach (user user in r.users)
        {
            LoadAvatar(user);
            friends.Add(user.uid, user);
            user.st.text = user.online ? "online" : "offline";
        }        
    }

    
    private void LoadAvatar(user user)
    {
        WWW2 w = new WWW2(user.photo);
        w.done += delegate(WWW2 w2)
        {
            user.texture = w2.texture;
            DontDestroyOnLoad(user.texture);
        };
    }

    public void GetNews()
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"timestamp",unixTime.ToString()},                                                
                        new string[]{"method","activity.getNews"},                                                
                    });

        newThread(delegate()
        {
            try
            {
                string res = Write(sendfunc);
                print("status");
                response resp = new response();
                foreach (Match m in Regex.Matches(res, "<uid>(.*?)</uid>.*?<timestamp>(.*?)</timestamp>.*?<text>(.*?)</text>", RegexOptions.Singleline))
                {
                    status st = new status();
                    st.uid = int.Parse(m.Groups[1].Value);
                    st.timestamp = int.Parse(m.Groups[2].Value);
                    st.text = m.Groups[3].Value;
                    resp.statuses.Add(st);
                    print("set status" + st.text);
                }
                Add(resp);
            }
            catch (System.Exception e) { print(e); }
        });
        SetUnixTime();
    }

    private void SetUnixTime()
    {
        TimeSpan ts = (_vk.time - new DateTime(1970, 1, 1, 0, 0, 0));
        unixTime = (int)ts.TotalSeconds;
    }
    int unixTime;
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

    

    public void SendMsg(int uid, string message)
    {
        print("send msg");
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    {                         
                        new string[]{"method","messages.send"}, 
                        new string[]{"uid",uid.ToString()},
                        new string[]{"message", WWW.EscapeURL(message)},                        
                    });
        newThread(delegate()
        {
            try
            {
                string res = Write(sendfunc);
                
            }
            catch { printC("error message not sended to " + uid); }
        });
    }



    public void SendChatMsg(string message)
    {
        print("send chat message");
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    {                         
                        new string[]{"method","sendMessage"}, 
                        new string[]{"message", WWW.EscapeURL(message)},
                        new string[]{"test_mode","2"} ,                    
                    });
        newThread(delegate()
        {
            try
            {
                string res = Write(sendfunc);                
            }
            catch (Exception e) { printC("message not sended" + e); }
        });
    }
    List<response> responses = new List<response>();

    public void GetMessages()
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","messages.get"},
                        new string[]{"filters","1"},
                    });
        newThread(delegate()
        {
            try
            {
                string res = Write(sendfunc);
                
                res = Regex.Match(res, "<count>.*?</count>(.*)</response>", RegexOptions.Singleline).Groups[1].Value;
                res = "<response><personal>" + res + "</personal></response>";
                response r = (response)xml.Deserialize(new StringReader(res));

                Add(r);
                if (r.personal.Count > 0)
                {
                    string mids = "";
                    foreach (message m in r.personal)
                        mids += m.mid + ",";
                    sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                        new string[][]
                    { 
                        new string[]{"method","messages.markAsRead"},
                        new string[]{"mids",mids.TrimEnd(',')},
                    });

                    print(Write(sendfunc));
                }
            }
            catch (System.Exception e) { print(e); }
        });
    }

    public void GetChatMessages(int get)
    {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getMessages"}       ,
                        new string[]{"messages_to_get",get.ToString()},
                        new string[]{"test_mode","2"} ,                        
                    });
        newThread(delegate()
            {
                try
                {
                    string res = Write(sendfunc);
                    
                    res = Regex.Match(res, "<response list=\"true\">(.*)</response>", RegexOptions.Singleline).Groups[1].Value;
                    res = "<response><messages>" + res + "</messages></response>";
                    response r = (response)xml.Deserialize(new StringReader(res));
                    Add(r);
                }
                catch { }
            });
    }
    public void SetStatus(string text)
    {
        
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","activity.set"},                        
                        new string[]{"text", text}                        
                    });

        string resp = Write(sendfunc);
        print("set Status" + resp);
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
        return H.Replace(S.s5, "(url)", url);
    }




}
