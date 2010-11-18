using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;
using System.IO;


public class Music : Base
{
    string[] s = new string[0];
    bool enableMusic=true;
    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        
    }
    public void Start(string file)
    {
        if (!enableMusic) return;
        new WWW2(hosting+ file).done += delegate(WWW2 w)
        {
            
            s = H.ToStr(w.www.bytes).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);            
            if (s.Length > 0)
                Next();
        };
    }
    private void Next()
    {
        if (s.Length == 0) return;
        int i = UnityEngine.Random.Range(0, s.Length);
        
        string ss = s[i];
        //print("Трек: " + ss.Substring(0, ss.Length - 4));
        www = new WWW(hosting + ss);        
        audio.clip = www.audioClip;
        
        UnityEngine.Object.DontDestroyOnLoad(audio.clip);
        sw = true;
    }
    WWW www;
    bool sw;
    void Update()
    {
        audio.volume = _SettingsWindow.MusicVolume;
        if (audio.clip != null)
        {
            //if (audio.volume < .2)
            //    audio.volume += Time.deltaTime / 10;

            if (!audio.isPlaying && audio.clip.isReadyToPlay)
            {
                if (sw)
                {
                    sw = false;
                    audio.Play();
                    //print("play");
                }
                else
                    Next();
            }

        }
        
            

        
        
    }

}