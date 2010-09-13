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
    public delegate void Action();
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
    void Awake()
    {
        if (!enabled) return;
        print("Vk start" + enabled);
        _vk = this;
    }
    
    void Start()
    {
        
        print("vk start");
        thread = new Thread(StartThreads);
        thread.IsBackground = true;
        thread.Name = "VK";
        thread.Start();
        

    }
    
    List<Action> actions = new List<Action>();
    void StartThreads()
    {
        while (true)
        {
            if (actions.Count > 0)
            {
                List<Action> actions1 = actions;
                actions = new List<Action>();
                foreach (Action a in actions1)
                    try
                    {
                        a();
                    }
                    catch (Exception e) { print(a.Method.Name + "() " + e); }
            }
            Thread.Sleep(500);
        }
    }

    
    public void Start(string url)
    {        
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

                    userid = int.Parse(GetGlobalVariable(1280));
                    Thread.Sleep(500);
                    localuser = GetUserInfo(userid);
                    time = ToDate(int.Parse(GetGlobalVariable(0)));
                    LastStatusTime = unixtime;
                    _TimerA.AddMethod(delegate()
                    {
                        new WWW2(localuser.photo).done += delegate(WWW2 www)
                        {
                            printC("loaded texture");
                            localuser.texture = www.texture;
                            DontDestroyOnLoad(localuser.texture);
                        };
                    });
                    int i = 500;
                    
                    GetAppUsers(false);
                    Thread.Sleep(i);
                    GetFriends(false);
                    Thread.Sleep(i);
                    GetChatMessages(10, false);                    
                    Thread.Sleep(i);
                    GetOwnStats(false);
                    _Status = Status.connected;
                    _TimerA.AddMethod(_Vkontakte.onVkConnected);
                }
                catch (System.Exception e) { _Status = Status.disconnected; printC("Login vkontakte failed, make sure you give the application all the rights\r\n" + e); }
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
    public static DateTime ToDate(int time)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(time).AddHours(4);
    }
    private void GetUserData()
    {
                                
        printC("success");        
        

    }

    private void GetOwnStats(bool async)
    {
        newThread(async,delegate()
        {
            string[] ss = GetLocalVariable((int)Keys.playerstats).Split(',');
            if (ss.Length != 4)
                print("empty stats" + ss.Length);
            else
            {
                localuser.totalkills = int.Parse(ss[0]);
                localuser.totaldeaths = int.Parse(ss[1]);
                localuser.totalzombiekills = int.Parse(ss[2]);
                localuser.totalzombiedeaths = int.Parse(ss[3]);
                print("stats set" + localuser.totalkills);
            }
        });
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
    void newThread(Action a) { newThread(true, a); }
    void newThread(bool async, Action a)
    {
        if (async)
            actions.Add(a);
        else
            a();
    }
    
    Thread thread;
    void OnApplicationQuit()
    {
        if(thread!=null)
            thread.Abort();
    }

    void SetGlobalVariable(int key, string value)
    {

        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                        new string[][]
                    { 
                        new string[]{"method","putVariable"},                                            
                        new string[]{"key",key.ToString()},
                        new string[]{"value",value},                        
                        new string[]{"test_mode","2"} 
                    });
        Write(sendfunc);
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
                Write(sendfunc);
                
            });
    }

    public SortedList<float, user> highscoresZombie = new SortedList<float, user>();
    public SortedList<float, user> highscores = new SortedList<float, user>();
    public void KillsTop(bool zombie)
    {        
        newThread(delegate()
        {
            try
            {
                 int key = zombie ? 100 : 150;
                string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                            new string[][]
                    { 
                        new string[]{"method","getVariables"},                                                                    
                        new string[]{"test_mode","2"},
                        new string[]{"count","32"},
                        new string[]{"key",key.ToString()},
                    });

                string res = Write(sendfunc);
                int min = localuser.totalzombiekills;
                int id = -1;
                SortedList<float, user> scores =(zombie ? highscoresZombie : highscores);
                scores.Clear();
                bool found = false;
                for (int i = 0; i < 32; i++)
                {
                    Match m = Regex.Matches(res, "<value>(.*?)</value>")[i];
                    string[] ss = m.Groups[1].Value.Split(',');
                    user u = new user();
                    u.uid = int.Parse(ss[0]);
                    u.nick = ss[1];
                    u.photo = ss[2];

                    u.totalkills = int.Parse(ss[3]);
                    u.totaldeaths = int.Parse(ss[4]);


                    scores.Add((float)u.totalkills + UnityEngine.Random.value, u);
                    if (!found)
                    {
                        if (u.totalkills < min)
                        {
                            min = u.totalkills;
                            id = i;
                        }
                        if (u.uid == localuser.uid)
                        {
                            found = true;
                            id = i;
                        }
                    }
                }
                Thread.Sleep(500);
                if (id != -1)
                    SetGlobalVariable(key + id, tostring(localuser.uid, localuser.nick, localuser.photo,
                        zombie ? localuser.totalzombiekills : localuser.totalkills,
                        zombie ? localuser.totalzombiedeaths : localuser.totaldeaths));
                else
                    print("rank is to low");
                print("killstop success" + highscoresZombie.Values.Count);
            }
            catch (Exception e) { print(e); }
        });
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
        if (res.Contains("Too many requests per second")) throw new Exception("Vkontakte Warining: Too Many requests");
        if (res.Contains("<error>")) throw new Exception("Vkontakte Error " + res);
        return res;
    }
    public Dictionary<int, user> friends = new Dictionary<int, user>();

    private void GetAppUsers(bool async)
    {
        newThread(async,delegate()
        {
            string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                        new string[][]
                    { 
                        new string[]{"method","friends.getAppUsers"},                                                
                    });
            string res = Write(sendfunc);
             MatchCollection mm = Regex.Matches(res, "<uid>(.*?)</uid>");
            foreach (Match m in mm)
                appusers.Add(int.Parse(m.Groups[1].Value));                
            print("App users " + mm.Count);
        });
    }
    public List<int> appusers = new List<int>();
    
    public void GetFriends(bool async)
    {
        newThread(async,delegate()
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
            response r = (response)respxml.Deserialize(new StringReader(res));
            print("get friends " + r.users.Count);
            friends.Clear();
            friends.Add(localuser.uid, localuser);
            foreach (user user in r.users)
            {
                LoadAvatar(user);
                friends.Add(user.uid, user);
                if (appusers.Contains(user.uid)) user.installed = true;
                user.st.text = user.online ? "online" : "offline";
            }
        });
    }

    
    private void LoadAvatar(user user)
    {
        _TimerA.AddMethod(delegate()
        {
            WWW2 w = new WWW2(user.photo);
            w.done += delegate(WWW2 w2)
            {
                user.texture = w2.texture;
                DontDestroyOnLoad(user.texture);
            };
        });
    }

    public void GetNews()
    {

        newThread(delegate()
        {
            string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                        new string[][]
                    { 
                        new string[]{"timestamp",LastStatusTime.ToString()},                                                
                        new string[]{"method","activity.getNews"},                                                
                    });

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
        });
        LastStatusTime = unixtime;
    }
    public int unixtime
    {
        get
        {
            TimeSpan ts = (time - new DateTime(1970, 1, 1, 0, 0, 0));
            return (int)ts.TotalSeconds;
        }
    }
    
    int LastStatusTime;
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
        newThread(delegate()
        {
            try
            {
                print("send msg");
                string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                            new string[][]
                    {                         
                        new string[]{"method","messages.send"}, 
                        new string[]{"uid",uid.ToString()},
                        new string[]{"message", WWW.EscapeURL(message)},                        
                    });

                Write(sendfunc);

            }
            catch { printC("error message not sended to " + uid); }
        });
    }



    public void SendChatMsg(string message)
    {
        newThread(delegate()
        {
            try
            {
                print("send chat message");
                string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                            new string[][]
                    {                         
                        new string[]{"method","sendMessage"}, 
                        new string[]{"message", WWW.EscapeURL(message)},
                        new string[]{"test_mode","2"} ,                    
                    });

                Write(sendfunc);
            }
            catch (Exception e) { printC("message not sended" + e); }
        });
    }
    List<response> responses = new List<response>();

    public void GetMessages()
    {
        newThread(delegate()
        {
            string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                        new string[][]
                    { 
                        new string[]{"method","messages.get"},
                        new string[]{"filters","1"},
                    });

            string res = Write(sendfunc);

            res = Regex.Match(res, "<count>.*?</count>(.*)</response>", RegexOptions.Singleline).Groups[1].Value;
            res = "<response><personal>" + res + "</personal></response>";
            response r = (response)respxml.Deserialize(new StringReader(res));

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
        });
    }

    public void GetChatMessages(int get,bool async)
    {

        newThread(async, delegate()
        {
        string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                    new string[][]
                    { 
                        new string[]{"method","getMessages"}       ,
                        new string[]{"messages_to_get",get.ToString()},
                        new string[]{"test_mode","2"} ,                        
                    });
        
                    string res = Write(sendfunc);
                    
                    res = Regex.Match(res, "<response list=\"true\">(.*)</response>", RegexOptions.Singleline).Groups[1].Value;
                    res = "<response><messages>" + res + "</messages></response>";
                    response r = (response)respxml.Deserialize(new StringReader(res));
                    Add(r);
            });
    }
    public void SetStatus(string text)
    {
        newThread(delegate()
        {
            string sendfunc = SendFunction(int.Parse(mid), ap_id, sid, secret,
                        new string[][]
                    { 
                        new string[]{"method","activity.set"},                        
                        new string[]{"text", text}                        
                    });

            string resp = Write(sendfunc);
            print("set Status" + resp);
        });
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
