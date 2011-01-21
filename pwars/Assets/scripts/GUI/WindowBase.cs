using System;
using UnityEngine;


public class WindowBase : Base2
{
    
    public void Action(string name, params object[] param)
    {
        controller.SendMessage("Action", name, SendMessageOptions.DontRequireReceiver);
        //controller.Action(name, param);
    }
    public void Action(string name)
    {
        controller.SendMessage("Action", name, SendMessageOptions.DontRequireReceiver);
    }
    //public void ActionAll(string name)
    //{
    //    foreach (Base a in GameObject.FindObjectsOfType(typeof(Base)))
    //        a.SendMessage(name, SendMessageOptions.DontRequireReceiver);
    //}    
    //new public bool enabled { get { return base.enabled; } set { if (enabled) Show(); else Hide(); } }

    public MonoBehaviour controller;


    public void Toggle(MonoBehaviour obj)
    {
        
        if (!enabled)        
            Show(obj);
        else
            Hide();

    }
    public void Show(MonoBehaviour controller)
    {
        lockCursor = false;
        SendMessageUpwards("HideWindow", SendMessageOptions.DontRequireReceiver);        
        enabled = true;
        this.controller = controller;                
    }
    public void ShowDontHide(MonoBehaviour controller)
    {        
        enabled = true;
        this.controller = controller;
    }

    public void ShowOnTop(MonoBehaviour controller)
    {        
        enabled = true;
        this.controller = controller;
    }

    [FindAsset("mouseover")]
    public AudioClip mouseOver;
    [FindAsset("click")]
    public AudioClip mouseclick;
    public void onOver()
    {
        _Loader.audio.PlayOneShot(mouseOver);
    }
    public void onButtonClick()
    {
        _Loader.audio.PlayOneShot(mouseclick);
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