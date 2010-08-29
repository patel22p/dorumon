using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Test : Base
{
    public string d = "mpme";
    public string console="";
    void Awake()
    {
        test = this;
    }
    
    void Start() {
        Application.RegisterLogCallback(LogCallback1);
        Vkontakte _Vkontakte = new Vkontakte();
        
        
	}

    
    

    void LogCallback1(string condition, string stackTrace, LogType type) 
    {
        console += "\r\n"+type + stackTrace + condition;
    }
    
    void OnGUI()
    {
        GUILayout.TextField(console);
        
    }
    // Update is called once per frame
    void Update()
    {

    }
}
