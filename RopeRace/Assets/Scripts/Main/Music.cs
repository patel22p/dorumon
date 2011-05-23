using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using doru;
using System.IO;


public class Music : bs
{
    internal const string webserver = "http://physxwars.ru/serv/";
    public void Start()
    {
        audio.volume = .5f;
    }
    TimerA timer = new TimerA();
    public void Play(string folder)
    {
        var w = new WWW(webserver + "/music.php?folder=" + folder + "&rand=" + Random.Range(0, 999));
        timer.AddMethod(() => w.isDone, delegate
        {
            if (w.error == null)
            {
                string[] files = w.text.Split("\r\n");
                foreach (var f in files)
                    if (!f.EndsWith(".ogg")) { Debug.Log("error music file " + f); return; }
                Rand(files, "");
            }
            else Debug.Log(w.error);
        });
    }
    void Update()
    {
        timer.Update();
    }
    private void Rand(string[] files, string old)
    {
        Debug.Log("Next" + files.Count());
        if (enabled)
        {
            string r = files.Where(a => (a != old || files.Length == 1)).Random();
            string f = webserver + r;
            WWW w = new WWW(f);
            timer.AddMethod(() => (w.isDone && w.audioClip != null && w.audioClip.length > 0 && w.audioClip.isReadyToPlay), delegate
            {
                if (w.error == null)
                {
                    audio.clip = w.audioClip;
                    audio.Play();
                    timer.AddMethod(() => !audio.isPlaying, delegate
                    {
                        Rand(files, r);
                    });
                }
                else Debug.Log(w.error);
            });
        }
    }
}