using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;
using System.IO;


public class Music : Base
{
    string[] s;
    void Start()
    {        
        if (!isWebPlayer || !OptionsWindow.enableMusic)
            enabled = false;
        else
        {
            new WWW2("m.txt").done += delegate(WWW2 w)
            {
                s = H.ToStr(w.bytes).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (s.Length > 0)
                    Next();
            };
        }
    }

    private void Next()
    {
                
        WWW www = new WWW(s[UnityEngine.Random.Range(0, s.Length - 1)]);
        audio.clip = www.audioClip;
        sw = true;
    }
    bool sw;
    void Update()
    {
        audio.volume = OptionsWindow.enableMusic ? 0 : 1;
        if (!audio.isPlaying && audio.clip.isReadyToPlay)
        {
            if (sw)
            {
                sw = false;
                audio.Play();
            }
            else
                Next();
        }
        
    }

}