using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;



public class VkontakteWindow : Base
{
    int id;
    Vkontakte vk;
    void Start()
    {
        id = Random.Range(0, int.MaxValue);
        _Vkontakte = this;
        vk = this.GetComponent<Vkontakte>();
        rect = new Rect (Screen.width - 200, 0,0,0);
    }
    internal Rect rect;


    void OnGUI()
    {

        rect = GUILayout.Window(id, rect, Window, "vkontakte", GUILayout.Width(200), GUILayout.Height(300));        
        
    }
    string login { get { return PlayerPrefs.GetString("vklogin"); } set { PlayerPrefs.SetString("vklogin", value); } }
    string password { get { return PlayerPrefs.GetString("password"); } set { PlayerPrefs.SetString("password", value); } }
    
    public void onConnected()
    {
        new WWW2(_User.photo).done += delegate(WWW2 www)
        {
            _User.texture = www.texture;
        };

    }

    void Update()
    {
        WWW2.Update();
        //if (loginbtn && vk._Status == Vkontakte.Status.disconnected)
        //{
        //    vk.Start(login, password);            
        //}

        if (vk._Status == Vkontakte.Status.connected && _TimerA.TimeElapsed(2503))
        {
            _vk.GetMessages();
        }
        foreach (Vkontakte.response resp in _vk.GetResponses())
        {
            foreach (Vkontakte.user user in resp.users)
            {                
                LoadAvatar(user);
                users.Add(user.uid, user);
            }
            foreach (Vkontakte.message_info msg in resp.messages)
            {
                if (msg.message.StartsWith(_User.uid.ToString()))
                    if (users.ContainsKey(msg.uid))
                    {
                        GetWindow(users[msg.uid]).Write(msg.message); ;
                    }
                print(msg.user_name + ":" + msg.message);
            }
        }
    }
    Dictionary<int, Vkontakte.user> users = new Dictionary<int, Vkontakte.user>();
    private void LoadAvatar(Vkontakte.user user)
    {
        WWW2 w = new WWW2(user.photo);
        w.done += delegate(WWW2 w2)
        {
            user.texture = w2.texture;
        };
    }
    bool loginbtn;
    public Vkontakte.user _User { get { return _vk._User; } }
    Vector2 scrollPosition;
    void Window(int id)
    {

        if (vk._Status == Vkontakte.Status.disconnected)
        {
            GUILayout.Label("Email:");
            login = GUILayout.TextArea(login);
            GUILayout.Label("Password:"); 
            password = GUILayout.PasswordField(password, '*');
            loginbtn = GUILayout.Button("Login");

            if (GUILayout.Button("gmail.ru"))
                vk.Start("http://vkontakte.ru/api/login_success.html#session=%7B%22expire%22%3A%220%22%2C%22mid%22%3A%2295853480%22%2C%22secret%22%3A%225b6c1208fe%22%2C%22sid%22%3A%22d8c3fed16ab9665e41062ac50e381ba646ab88eecaeec3c1629b2f48e1%22%7D");
            if (GUILayout.Button("dorumonstr@gmail.com"))
                vk.Start("http://vkontakte.ru/api/login_success.html#session=%7B%22expire%22%3A%220%22%2C%22mid%22%3A%229684567%22%2C%22secret%22%3A%22bbf034c0a0%22%2C%22sid%22%3A%224b116f646fd75b3c357532c6302ded441469b682d0df94af62665ef4e5%22%7D");
            if (GUILayout.Button("dorumon@mail.ru"))
                vk.Start("http://vkontakte.ru/api/login_success.html#session=%7B%22expire%22%3A%220%22%2C%22mid%22%3A%2257109080%22%2C%22secret%22%3A%224454a797a5%22%2C%22sid%22%3A%2292d99c49f8d3b1bb45270e2775f475bd4d68960015cb38eb62c4e9bde7%22%7D");
                

        }
        if (_vk._Status == Vkontakte.Status.connected && _User != null)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label("Name:" + _User.nick);
            GUILayout.Box(_User.texture);
            foreach (Vkontakte.user user in users.Values)
            {
                GUILayout.Label("Name: " + user.nick);
                if (GUILayout.Button(user.texture, GUILayout.Height(40)) && !windows.ContainsKey(user.uid))
                {
                    GetWindow(user);
                }
            }
            GUILayout.EndScrollView();
        }
        GUI.DragWindow();
    }
    private MessageWindow GetWindow(Vkontakte.user user)
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
    public Vkontakte.response resp;
    
}

//public class User : Vkontakte.user
//{

//}