using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class GameWindow : bs {
    List<string> systemMessage = new List<string>();
    public GUIText fpstext;
    public GUIText chatInput;
    public GUIText chatOutput;
    public GUIText CenterText;
    [FindTransform]
    public GUITexture gunTexture;
    [FindTransform]
    public GUIText gunPatrons;
    [FindTransform]
    public GUIText teamscore;
    [FindTransform]
    public GUIText lifeupgrate;
    [FindTransform]
    public GUIText speedupgrate;
    [FindTransform]
    public GUIText nitroupgrate;
    [FindTransform]
    public GUITexture antigrav;
    [FindTransform("light")]
    public GUITexture spotLight;
    [FindTransform("clock")]
    public GUITexture clock;
    
    public GUIText Score;
    [FindTransform]
    public GUITexture energy;
    [FindTransform]
    public GUITexture energyoff;
    [FindTransform]
    public GUITexture life;
    [FindTransform]
    public GUITexture lifeoff;
    [FindTransform]
    public GUIText stagetime;
    public GUIText time;
    public GUIText systemMessages;
    public GUIText level;
    public GUIText zombiesLeft;
    public GUIText frags;    
    public void AppendSystemMessage(string s)
    {
        systemMessage.Add(s);
        systemMessages.text = string.Join("\r\n", systemMessage.TakeLast(5).ToArray());
    }
    
    public void SetWidth(GUITexture t, int value)
    {
        var p = t.pixelInset;
        if (p.width != value)
        {
            p.width = value;
            t.pixelInset = p;
        }
    }
    void Update()
    {
        if (_localPlayer == null) return;
        fpstext.text = "Fps: " + _Game.fps + " Ping:" + _Game.ping + " Errors:" + _Console.exceptionCount;
        var ts = System.TimeSpan.FromMinutes(_Game.timeleft);
        this.time.text = ts.Hours + ":" + ts.Minutes + ":" + ts.Seconds;
        if (_Game.mapSettings.Team)
            this.teamscore.text = _Game.BlueFrags+":"+_Game.RedFrags;
        this.nitroupgrate.text = "Power Upgrate: " + _localPlayer.EnergyUpgrate;
        this.speedupgrate.text = "Speed Upgrate: " + _localPlayer.SpeedUpgrate;
        this.lifeupgrate.text = "Life Upgrate: " + _localPlayer.LifeUpgrate;
        this.spotLight.enabled = _localPlayer.haveLight;
        this.clock.enabled = _localPlayer.haveTimeBomb;
        this.antigrav.enabled = _localPlayer.haveAntiGravitation;        
        if (_localPlayer != null)
        {                        
            SetWidth(life, (int)Mathf.Min(_localPlayer.Life, _localPlayer.MaxLife));
            SetWidth(lifeoff, (int)_localPlayer.MaxLife);
            SetWidth(energy, (int)Mathf.Min((int)_localPlayer.Energy, energyoff.pixelInset.width));

            this.gunPatrons.text = _localPlayer.gun.Text + ":" + (int)_localPlayer.gun.patronsLeft;
            if (_Game.mapSettings.zombi)
            {
                this.zombiesLeft.text = "Zombies : " + _Game.AliveZombies.Count().ToString();
                this.level.text = "Level: " + _Game.stage.ToString();
                this.stagetime.text = "Stage Time: " + (int)_Game.stageTime;
            }
            
            this.frags.text = "Frags: " + _localPlayer.frags.ToString();

            this.Score.text = "Points: " + ((int)_localPlayer.Score) + "$";
        }


    }
}

