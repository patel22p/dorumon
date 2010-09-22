using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;



public class VkontakteWindow : Base
{
    int id;
    Vk vk;
    MessageWindow chat;
    ScoreBoard scoreboard;
    void Awake()
    {
        if (!enabled) return;
        print("Vkontakte start");
        _Vkontakte = this;
        vk = this.GetComponent<Vk>();
    }

    void Start()
    {


        scoreboard = this.GetComponent<ScoreBoard>();
        chat = this.GetComponent<MessageWindow>();
        id = UnityEngine.Random.Range(0, int.MaxValue);


        rect = new Rect(Screen.width - 200, 0, 0, 0);
        //Application.ExternalCall("GetUrl");
    }

    //public void GetUrl(String s)
    //{
    //    login = s;
    //    _vk.Start(login);
    //} 

    internal Rect rect;

    public Texture2D test;
    void OnGUI()
    {
        GUILayout.Label(test);
        try
        {
            rect = GUILayout.Window(id, rect, Window, lc.vkontakte .ToString(), GUILayout.Width(200), GUILayout.Height(300));
        }
        catch (Exception e) { print(e); }
    }


    public void onVkConnected()
    {
        printC(lc.vkconnected);
        Application.LoadLevel(Level.z2menu.ToString());
    }

    private void OnServerInitialized()
    {
        //if (_vk._Status == Vk.Status.connected)
        //{
        //    _vk.SetStatus("");            
        //}
    }
    private void OnConnectedToServer()
    {
        //if (_vk._Status == Vk.Status.connected)
        //{
        //    _vk.SetStatus("");            
        //}
    }
    public Dictionary<int, Vk.user> friends { get { return _vk.friends; } }
    public Dictionary<string, int> chatusers = new Dictionary<string, int>();
    public string chatuserstext;
    void Update()
    {
        if (vk._Status == Vk.Status.connected)
        {
            if (_TimerA.TimeElapsed(5000))
                _vk.GetChatMessages(0, true);
            if (_TimerA.TimeElapsed(6000))
                _vk.GetMessages();
            if (_TimerA.TimeElapsed(9000))
                _vk.SendChatMsg("");
            //if (_TimerA.TimeElapsed(10000))
            //    _vk.GetNews();

            chatuserstext = lc.usersonline .ToString() + _Vkontakte.chatusers.Count + "\r\n";
            foreach (string user in _Vkontakte.chatusers.Keys)
                chatuserstext += user + "\r\n";

            List<string> remove = new List<string>();
            foreach (KeyValuePair<string, int> a in chatusers)
                if (_vk.unixtime - a.Value > 15000)
                    remove.Add(a.Key);
            foreach (string s in remove)
                chatusers.Remove(s);

            foreach (Vk.response resp in _vk.GetResponses())
            {
                foreach (Vk.message_info msg in resp.messages)
                {
                    if (!chatusers.ContainsKey(msg.user_name))
                        chatusers.Add(msg.user_name, msg.time);
                    else
                        chatusers[msg.user_name] = msg.time;

                    msg.message = WWW.UnEscapeURL(msg.message);
                    print(msg.user_name + ":" + msg.message);
                    if (msg.message != "")
                        chat.Write(msg.message);
                }
                foreach (Vk.status st in resp.statuses)
                {
                    if (friends.ContainsKey(st.uid))
                    {
                        if (friends[st.uid].st.timestamp < st.timestamp)
                        {
                            friends[st.uid].st = st;
                            friends[st.uid].online = true;
                        }
                    }
                    else
                        Debug.Log("status user not exists" + st.uid);
                }


                foreach (Vk.message msg in resp.personal)
                {
                    msg.body = WWW.UnEscapeURL(msg.body);
                    if (friends.ContainsKey(msg.uid))
                    {
                        Vk.user user = friends[msg.uid];
                        MessageWindow w = GetWindow(user);
                        w.Write(user.nick + ":" + msg.body);
                    }
                    else Debug.Log("user not exists" + msg.uid);
                }
            }
        }
    }

    Vector2 scrollPosition;
    string login { get { return PlayerPrefs.GetString("login"); } set { PlayerPrefs.SetString("login", value); } }
    public GUIStyle linkstyle;

    void Window(int id)
    {
        if (_vk._Status == Vk.Status.connected)
            GUILayout.Label(_vk.time.ToShortDateString() + " " + _vk.time.ToShortTimeString());


        if (vk._Status == Vk.Status.disconnected)
        {

            
            GUILayout.Label(lc.url.ToString(), GUILayout.ExpandWidth(false));
            login = GUILayout.TextArea(login);            
            if (GUILayout.Button(lc.login.ToString()))
                vk.Start(login);            
            if (GUILayout.Button(lc.auth .ToString()))
                OpenUrl("http://vkontakte.ru/login.php?app=1935303&layout=popup&type=browser&settings=15615");
            GUILayout.Label(lc.authhelp .ToString());

            if (GUILayout.Button(lc.loginasguest .ToString()) || skip || !build)
                Application.LoadLevel(Level.z2menu.ToString());
            if (!build)
            {
                if (GUILayout.Button("gmail.ru"))
                    vk.Start("http://vkontakte.ru/api/login_success.html#session=%7B%22expire%22%3A%220%22%2C%22mid%22%3A%2295853480%22%2C%22secret%22%3A%225b6c1208fe%22%2C%22sid%22%3A%22d8c3fed16ab9665e41062ac50e381ba646ab88eecaeec3c1629b2f48e1%22%7D");
                if (GUILayout.Button("dorumonstr@gmail.com"))
                    vk.Start("http://vkontakte.ru/api/login_success.html#session=%7B%22expire%22%3A%220%22%2C%22mid%22%3A%229684567%22%2C%22secret%22%3A%22bbf034c0a0%22%2C%22sid%22%3A%224b116f646fd75b3c357532c6302ded441469b682d0df94af62665ef4e5%22%7D");
                if (GUILayout.Button("dorumon@mail.ru"))
                    vk.Start("http://vkontakte.ru/api/login_success.html#session=%7B%22expire%22%3A%220%22%2C%22mid%22%3A%2257109080%22%2C%22secret%22%3A%224454a797a5%22%2C%22sid%22%3A%2292d99c49f8d3b1bb45270e2775f475bd4d68960015cb38eb62c4e9bde7%22%7D");
            }
        }


        if (_vk._Status == Vk.Status.connected && localuser != null)
        {
            GUILayout.Label(lc.usersonline.ToString() + chatusers.Count);
            if (_Level == Level.z2menu && GUILayout.Button(lc.logout .ToString()))
            {
                Application.LoadLevel(Level.z1login.ToString());
                _vk._Status = Vk.Status.disconnected;
            }

            if (GUILayout.Button(lc.chat.ToString()))
                chat.enabled = true;
            if (GUILayout.Button(lc.scoreboard.ToString()))
            {
                scoreboard.enabled = true;
                _vk.KillsTop(true);
                _vk.KillsTop(false);
            }
            if (GUILayout.Button(lc.refresh.ToString()))
                _vk.GetFriends(true);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label(lc.name + localuser.nick);
            GUILayout.Label(lc.status+ localuser.st.text);
            GUILayout.Box(localuser.texture);
            foreach (Vk.user user in friends.Values)
                if (user.uid != localuser.uid && user.online)
                {
                    GUILayout.Label(lc.name + user.nick);
                    GUILayout.Label(lc.status + user.st.text);
                    if (user.installed) GUILayout.Label(lc.gameInstalled.ToString());
                    if (!user.installed && GUILayout.Button(lc.addfr.ToString()))
                    {
                        user.installed = true;
                        _vk.SendWallMsg(user.uid, localuser.nick + lc.playgame);
                    }
                    if (GUILayout.Button(user.texture, GUILayout.Height(40)) && !windows.ContainsKey(user.uid))
                        GetWindow(user);
                }
            GUILayout.EndScrollView();
        }
        GUI.DragWindow();
    }

    private static void OpenUrl(string url)
    {
        if (isWebPlayer)
            Application.ExternalCall("window.open", url);
        else
            Application.OpenURL(url);
    }
    private MessageWindow GetWindow(Vk.user user)
    {
        MessageWindow msgw;
        if (windows.ContainsKey(user.uid))
            msgw = windows[user.uid];
        else
        {
            msgw = gameObject.AddComponent<MessageWindow>();
            msgw.user = user;
            windows.Add(user.uid, msgw);
        }
        return msgw;
    }
    public Dictionary<int, MessageWindow> windows = new Dictionary<int, MessageWindow>();
    public Vk.response resp;

}

//public class User : Vkontakte.user
//{

//}