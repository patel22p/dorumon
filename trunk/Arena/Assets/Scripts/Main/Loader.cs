using System;
using UnityEngine;
using System.Linq;
using doru;

public class Loader:bs
{
    string debugtext;
    [FindTransform("debug")]
    public GUIText debugGui; 
    public int exceptions;
    public int errors;
    public string lastLog = "";
    public TimerA timer = new TimerA();
    public override void Awake()
    {
        if (Check()) return;
        base.Awake();
        Application.RegisterLogCallbackThreaded(log);
        Camera.main.GetComponent<GUILayer>().enabled = true;
    }

    public void log(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
            errors++;
        if (type == LogType.Exception)
            exceptions++;
        lastLog = condition;
    }
    public void WriteVar(object s)
    {
        debugvars += s + "\r\n";
    }
    string debugvars;
    public void WriteDebug(object s)
    {
        Debug.Log(s);
        debugtext += s + "\r\n";
    }
    void Update()
    {
        timer.Update();
        string t = debugtext + "\r\nErrors:" + errors + " Exceptions:" + exceptions + "\r\nLastLog:" + lastLog + "\r\n" + debugvars;        
        debugGui.text = t;
        debugvars = "";
    }
}