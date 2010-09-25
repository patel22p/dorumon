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

    }
    public void Start(string file)
    {
        new WWW2(hosting+ file).done += delegate(WWW2 w)
        {
            s = H.ToStr(w.bytes).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);            
            if (s.Length > 0)
                Next();
        };
    }
    private void Next()
    {
        int i = UnityEngine.Random.Range(0, s.Length);
        print(i);
        string ss = s[i];
        printC(lc.music + ss.Substring(0, ss.Length - 4));
        www = new WWW(hosting + ss);        
        audio.clip = www.audioClip;
        
        UnityEngine.Object.DontDestroyOnLoad(audio.clip);
        sw = true;
    }
    WWW www;
    bool sw;
    void Update()
    {        
        if (OptionsWindow.enableMusic && audio.clip != null)
        {
            if (audio.volume < 1)
                audio.volume += Time.deltaTime / 10;

            if (!audio.isPlaying && audio.clip.isReadyToPlay)
            {
                if (sw)
                {
                    sw = false;
                    audio.Play();
                    print("play");
                }
                else
                    Next();
            }

        }
        else
            audio.volume = 0;

        
        
    }

}