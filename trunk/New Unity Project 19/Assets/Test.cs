using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class User
{
    public int id;
    public string first_name;
    public string last_name;
    public string nick;
    public string avatar;
}

public class Test : Base
{
    internal string d = "mpme";

    void Awake()
    {
        Application.RegisterLogCallback(LogCallback1);
        test = this;
    }

    void Start()
    {

        Vkontakte _Vkontakte = new Vkontakte();
        _Vkontakte.Start("dorumon@mail.ru", "er54s4");
        User user = _Vkontakte.GetUserInfo(_Vkontakte.userid);
        print(user.avatar);

    }    
    void LogCallback1(string condition, string stackTrace, LogType type)
    {
        console += "\r\n" + type + stackTrace + condition;
    }

    Vector2 scrollPosition;
    void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.TextField(console);
        GUILayout.EndScrollView();
    }
    // Update is called once per frame
    void Update()
    {

    }
    internal string console = @"8";
}