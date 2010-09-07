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
    }

    void Start()
    {

        print("Vkontakte start");
        scoreboard = this.GetComponent<ScoreBoard>();
        chat = this.GetComponent<MessageWindow>();
        id = UnityEngine.Random.Range(0, int.MaxValue);
        _Vkontakte = this;
        vk = this.GetComponent<Vk>();
        rect = new Rect(Screen.width - 200, 0, 0, 0);


    }
    internal Rect rect;


    void OnGUI()
    {
        try
        {
            rect = GUILayout.Window(id, rect, Window, "Login", GUILayout.Width(200), GUILayout.Height(300));
        }
        catch (Exception e) { print(e); }
    }


    public void onVkConnected()
    {

        Application.LoadLevel(Level.z2menu.ToString());
    }
    public bool enabletimer;
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
    void Update()
    {
        if (vk._Status == Vk.Status.connected)
        {
            if (_TimerA.TimeElapsed(5000))
                _vk.GetChatMessages(0,true);
            if (_TimerA.TimeElapsed(6000))
                _vk.GetMessages();
            if (_TimerA.TimeElapsed(10000))
            {

                _vk.GetNews();
            }

            foreach (Vk.response resp in _vk.GetResponses())
            {

                foreach (Vk.message_info msg in resp.messages)
                {
                    msg.message = WWW.UnEscapeURL(msg.message);
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
                        printC("(" + user + ")" + msg.body);
                    }
                    else Debug.Log("user not exists" + msg.uid);
                }
            }
        }
    }

    Vector2 scrollPosition;
    string login { get { return PlayerPrefs.GetString("login"); } set { PlayerPrefs.SetString("login", value); } }
    public GUIStyle linkstyle;
    int begintime = 18;
    void Window(int id)
    {
        GUILayout.Label(_vk.time.ToShortDateString() + " " + _vk.time.ToShortTimeString());

        if (_vk.time.Hour < begintime && _vk.time.Hour > begintime + 2 && enabletimer)
        {


            int timeleft = begintime - _vk.time.Hour;
            if (timeleft < 0) timeleft += 24;
            GUILayout.Label("бета тест проводится с 18:00 до 20:00, осталось " + timeleft + " часов");
            
        }
        else
            if (vk._Status == Vk.Status.disconnected)
            {                
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Url:", GUILayout.ExpandWidth(false));
                login = GUILayout.TextArea(login);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Login"))
                    vk.Start(login);
                if (GUILayout.Button("В Контакте Авторизация"))
                    OpenUrl("http://vkontakte.ru/login.php?app=1935303&layout=popup&type=browser&settings=15615");
                GUILayout.Label("1.нажмите авторизация \r\n2. скопируйте адресс полученной login success страницы в поле url \r\n3. нажмите логин");

                if (GUILayout.Button("login as guest"))
                    Application.LoadLevel(Level.z2menu.ToString());
                if (GUILayout.Button("gmail.ru"))
                    vk.Start("http://vkontakte.ru/api/login_success.html#session=%7B%22expire%22%3A%220%22%2C%22mid%22%3A%2295853480%22%2C%22secret%22%3A%225b6c1208fe%22%2C%22sid%22%3A%22d8c3fed16ab9665e41062ac50e381ba646ab88eecaeec3c1629b2f48e1%22%7D");
                if (GUILayout.Button("dorumonstr@gmail.com"))
                    vk.Start("http://vkontakte.ru/api/login_success.html#session=%7B%22expire%22%3A%220%22%2C%22mid%22%3A%229684567%22%2C%22secret%22%3A%22bbf034c0a0%22%2C%22sid%22%3A%224b116f646fd75b3c357532c6302ded441469b682d0df94af62665ef4e5%22%7D");
                if (GUILayout.Button("dorumon@mail.ru"))
                    vk.Start("http://vkontakte.ru/api/login_success.html#session=%7B%22expire%22%3A%220%22%2C%22mid%22%3A%2257109080%22%2C%22secret%22%3A%224454a797a5%22%2C%22sid%22%3A%2292d99c49f8d3b1bb45270e2775f475bd4d68960015cb38eb62c4e9bde7%22%7D");
            }
        
        
        if (_vk._Status == Vk.Status.connected && localuser != null)
        {                        
            if (_Level == Level.z2menu && GUILayout.Button("Log Out"))
            {
                Application.LoadLevel(Level.z1login.ToString());
                _vk._Status = Vk.Status.disconnected;
            }

            if (GUILayout.Button("Chat"))
                chat.enabled = true;
            if (GUILayout.Button("Score Board"))
            {
                scoreboard.enabled = true;
                _vk.KillsTop(true);
                _vk.KillsTop(false);
            }
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            if (GUILayout.Button("refresh friend list"))
                _vk.GetFriends(true);

            GUILayout.Label("Name:" + localuser.nick);
            GUILayout.Label("Status:" + localuser.st.text);
            GUILayout.Box(localuser.texture);
            foreach (Vk.user user in friends.Values)
                if (user.uid != localuser.uid && user.online)
                {
                    GUILayout.Label("Name: " + user.nick);
                    GUILayout.Label("Status:" + user.st.text);
                    if (!user.installed && GUILayout.Button("Пригласить друга в игру"))
                    {
                        user.installed = true;
                        _vk.SendWallMsg(user.uid, user.nick + " приглашает вас сыграть с ним в шутер physxwars");
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
        if (iswebplayer)
            Application.ExternalCall("OpenURL", url);
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