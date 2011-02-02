using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using doru;
using System.IO;


public class Music : bs
{
    public void Play(string folder)
    {
        var w = new WWW(bs.webserver + "/music.php?folder=" + folder + "&rand=" + Random.Range(0, 999));
        _TimerA.AddMethod(() => w.isDone, delegate
        {
            if (w.error == null)
            {
                string[] files = w.text.Split("\r\n");
                Rand(files);
            }
            else Debug.Log(w.error);
        });
    }
    private void Rand(string[] files)
    {
        string f = bs.webserver + files.Random();
        Debug.Log(f);
        WWW w = new WWW(f);
        _TimerA.AddMethod(() => (w.isDone && w.audioClip != null && w.audioClip.isReadyToPlay), delegate
        {
            if (w.error == null)
            {                
                audio.clip = w.audioClip;
                audio.Play();                
                _TimerA.AddMethod(() => !audio.isPlaying, delegate
                {
                    Rand(files);
                });
            }
            else Debug.Log(w.error);
        });
    }
}