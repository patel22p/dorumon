using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;



public class z0VkontakteWindow : WindowBase
{
    
    z0Vk vk;
    
    ScoreBoard scoreboard;

    protected override void Awake()
    {
        if (!enabled) return;
        print("Vkontakte start");
        _Vkontakte = this;
        vk = this.GetComponent<z0Vk>();
        base.Awake();
    }

    void Start()
    {
        scoreboard = this.GetComponent<ScoreBoard>();
        
        id = UnityEngine.Random.Range(0, int.MaxValue);


        size = new Vector2(100, 300);
        //Application.ExternalCall("GetUrl");
    }

    //public void GetUrl(String s)
    //{
    //    login = s;
    //    _vk.Start(login);
    //}         
    


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
    public Dictionary<int, z0Vk.user> friends { get { return _vk.friends; } }
    
    
    void Update()
    {
        if (vk._Status == z0Vk.Status.connected)
        {
            if (_TimerA.TimeElapsed(5000))
                _vk.GetChatMessages(0, true);
            if (_TimerA.TimeElapsed(6000))
                _vk.GetMessages();            
            //if (_TimerA.TimeElapsed(10000))
            //    _vk.GetNews();
            foreach (z0Vk.response resp in _vk.GetResponses())
            {
                foreach (z0Vk.message_info msg in resp.messages)
                {
                    msg.message = WWW.UnEscapeURL(msg.message);                    
                    printC(msg.user_name + ":" + msg.message);
                }
                foreach (z0Vk.status st in resp.statuses)
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


                foreach (z0Vk.message msg in resp.personal)
                {
                    msg.body = WWW.UnEscapeURL(msg.body);
                    if (friends.ContainsKey(msg.uid))
                    {
                        z0Vk.user user = friends[msg.uid];
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

    protected override void Window(int id)
    {
        if (_vk._Status == z0Vk.Status.connected)
            GUILayout.Label(_vk.time.ToShortDateString() + " " + _vk.time.ToShortTimeString());


        if (vk._Status == z0Vk.Status.disconnected)
        {

            
            GUILayout.Label(lc.url.ToString(), GUILayout.ExpandWidth(false));
            login = GUILayout.TextArea(login);
            if (GUILayout.Button(lc.login.ToString()))
                if (login == "")
                    printC(lc.authhelp);
                else
                    vk.Start(login);            
            if (GUILayout.Button(lc.auth .ToString()))
                OpenUrl("http://vkontakte.ru/login.php?app=1935303&layout=popup&type=browser&settings=15615");            

            if (GUILayout.Button(lc.loginasguest .ToString()) || skip)
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


        if (_vk._Status == z0Vk.Status.connected && localuser != null)
        {
            
            if (_Level == Level.z2menu && GUILayout.Button(lc.logout .ToString()))
            {
                Application.LoadLevel(Level.z1login.ToString());
                _vk._Status = z0Vk.Status.disconnected;
            }            
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
            foreach (z0Vk.user user in friends.Values)
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
    private MessageWindow GetWindow(z0Vk.user user)
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
    public z0Vk.response resp;

}

//public class User : Vkontakte.user
//{

//}