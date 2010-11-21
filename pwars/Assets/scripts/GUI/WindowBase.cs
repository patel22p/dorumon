using System;
using UnityEngine;
interface IGUI { }
public class WindowBase : Base2,IGUI
{
    public void Action(string name, object param)
    {
        controller.SendMessage(name, param, SendMessageOptions.DontRequireReceiver);
    }
    public void Action(string name)
    {
        controller.SendMessage(name, SendMessageOptions.DontRequireReceiver);
    }
    public void ActionAll(string name)
    {
        foreach (Base a in GameObject.FindObjectsOfType(typeof(Base)))
            a.SendMessage(name, SendMessageOptions.DontRequireReceiver);
    }

    public MonoBehaviour controller;
    //new public bool enabled { get { return base.enabled; } set { if (enabled) Show(); else Hide(); } }
    public void Show(MonoBehaviour controller)
    {
        SendMessageUpwards("HideWindow", SendMessageOptions.DontRequireReceiver);        
        enabled = true;
        this.controller = controller;                
    }
    
    
    public void onOver()
    {
        _Loader.audio.PlayOneShot((AudioClip)Resources.Load("sounds/mouseover"));
    }
    public void onButtonClick()
    {
        _Loader.audio.PlayOneShot((AudioClip)Resources.Load("sounds/click"));
    }

    public void Hide()
    {
        enabled = false;
    }
    void HideWindow()
    {
        enabled = false;
    }
}