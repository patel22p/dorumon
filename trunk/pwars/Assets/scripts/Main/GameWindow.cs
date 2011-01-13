using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class GameWindow : Base2 {

    public GUIText fpstext;
    public GUIText chatInput;
    public GUIText chatOutput;
    public GUIText CenterText;
    public GUITexture gunTexture;
    public GUIText gunPatrons;
    [FindTransform("teamscore")]
    public GUIText teamscore;
    [FindTransform("lifeupgrate")]
    public GUIText lifeupgrate;
    [FindTransform("speedupgrate")]
    public GUIText speedupgrate;
    [FindTransform("antigrav")]
    public GUITexture antigrav;
    [FindTransform("light")]
    public GUITexture light;
    [FindTransform("clock")]
    public GUITexture clock;
    public GUITexture BlueBar;
    public GUIText Score;
    public GUITexture RedBar;
    public GUIText time;
    public GUIText systemMessages;
    public GUIText level;
    public GUIText zombiesLeft;
    public GUIText frags;    
    public float energy
    {
        get { return BlueBar.pixelInset.width / 2; }
        set
        {
            Rect r = BlueBar.pixelInset;
            r.width = Mathf.Min(value * 2, 200); ;
            BlueBar.pixelInset = r;
        }
    }
    public float life
    {
        get { return RedBar.pixelInset.width / 2; }
        set
        {
            Rect r = RedBar.pixelInset;
            r.width = Mathf.Min(value * 2, 200);
            RedBar.pixelInset = r;
        }
    }
    public List<GUITexture> blood;
    public float uron = 255f;
    public float repair = .4f;        
    
    List<string> systemMessage = new List<string>();
    public void AppendSystemMessage(string s)
    {
        systemMessage.Insert(0, s);
        systemMessages.text = string.Join("\r\n", systemMessage.Take(5).Reverse().ToArray());
    }
    void AppendChatMessage(string s)
    {

    }

    void Start()
    {
        foreach(GUIText text in GetComponentsInChildren<GUIText>())
            if (!text.text.StartsWith(" ")) text.text = "";
    }

    public int fps;
    void Update()
    {
        
        if (_TimerA.TimeElapsed(500))
            fps = (int)_TimerA.GetFps();
        fpstext.text = "Fps: " + fps + " Errors:" + _Console.errorcount;

        var ts = System.TimeSpan.FromMinutes(_Game.timeleft);
        this.time.text = ts.Minutes + ":" + ts.Seconds;
        if (mapSettings.Team)
            this.teamscore.text = _Game.BlueFrags+":"+_Game.RedFrags;
        
        this.speedupgrate.text  = "Speed upgrate: "+_localPlayer.speedUpgrate;
        this.lifeupgrate.text = "Life upgrate: " + _localPlayer.lifeUpgrate;
        this.light.enabled = _localPlayer.haveLight;
        this.clock.enabled = _localPlayer.haveTimeBomb;
        this.antigrav.enabled = _localPlayer.haveAntiGravitation;

        if (_localPlayer != null)
        {
            this.life = _localPlayer.Life;
            this.gunPatrons.text = _localPlayer.gun.Text + ":" + _localPlayer.gun.patronsLeft;

            this.energy = (int)_localPlayer.nitro;
            if (mapSettings.zombi)
            {
                this.zombiesLeft.text = "Zombies : " + _Game.AliveZombies.Count().ToString();
                this.level.text = "Level: " + _Game.stage.ToString();
            }
            
            this.frags.text = "Frags: " + _localPlayer.frags.ToString();

            this.Score.text = "Score: " + _localPlayer.score.ToString();
        }

        foreach (GUITexture a in blood)
            if (a.guiTexture.color.a > 0)
                a.guiTexture.color -= new Color(0, 0, 0, Time.deltaTime * repair);
    }
    public void Hit(float hit)
    {
        GUITexture[] gs = this.GetComponentsInChildren<GUITexture>();
        GUITexture g = gs[Random.Range(0, gs.Length - 1)];
        g.color += new Color(0, 0, 0, Mathf.Min(.7f, hit / uron));
    }

}

