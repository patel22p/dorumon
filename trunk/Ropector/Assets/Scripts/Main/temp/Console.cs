//using System;
//using UnityEngine;
//using gui = UnityEngine.GUILayout;
//public class Console : bs
//{
//    public override void Awake()
//    {
//        enabled = false;
//    }
//    string txt = "";
//    public void OnGUI()
//    {
//        if (gui.Button("Send"))
//            Application.ExternalEval(txt);
//        txt = gui.TextArea(txt, gui.Width(Screen.width), gui.Height(Screen.height));        
//    }
//}