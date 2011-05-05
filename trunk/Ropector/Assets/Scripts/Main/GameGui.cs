using System;
using UnityEngine;
using System.Linq;
using doru;
public class GameGui:bs
{
    [FindTransform]
    public GUIText tabble;
    [FindTransform]
    public GUIText leftdown;
    [FindTransform]
    public GUIText leftup;
    [FindTransform]
    public GUIText rightup;
    [FindTransform]
    public GUIText scores;
    [FindTransform]
    public GUIText scoresAll;    
    //[FindTransform]
    //public GUIText time;
    [FindTransform]
    public GUIText CenterTime;
    public GUITexture deadAnim;
    TimerA timer = new TimerA();
    void Start()
    {        
        foreach (var a in this.GetComponentsInChildren<GUIText>())
            a.text = "";
    }
    

    public void WriteInfo(string s)
    {        
        leftdown.text += s + "\r\n";
    }
    public void Update()
    {
        
        if (_Player != null)
        {
            UpdateScoreBoard();
            UpdatePlayerScores();
            UpdateOther();
            UpdatePointer();
        }
        timer.Update();
    }
    Score scr;
    private void UpdatePointer()
    {
        
        if (timer.TimeElapsed(500))
            scr = _Game.scores.Where(a => a != null).OrderBy(a => Vector3.Distance(a.pos, _Player.pos)).FirstOrDefault();
        //Debug.Log(scr == null);
        if (scr != null)
            _Cam.Pointer.rotation = Quaternion.Lerp(_Cam.Pointer.rotation, Quaternion.LookRotation(scr.pos - _Cam.pos), .2f);
    }
    //Quaternion fake;
    private void UpdateScoreBoard()
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.F) || _Game.pause)
        {
            string t = "{0,-20}{1,-10}{2,-10}{3,-10}{4,-10}{5,-10}\r\n";
            string text = "";
            text += string.Format(t, "Name", "Ping", "Errors", "Fps", "Scores", "TotalScores");
            text += "_______________________________________________________________________\r\n";
            foreach (var p in _Game.players.OrderByDescending(a => a.scores))
                text += string.Format(t, p.nick, p.ping, p.errors, p.fps, p.scores + "/" + _Game.scores.Count, p.totalscores);
            tabble.text = text;
        }
        else
            tabble.text = "";
    }
    private void UpdatePlayerScores()
    {
        var scores = _Game.scores;
        var pause = _Game.pause;
        _GameGui.scores.text = _Player.nick + ":" + _Player.scores + "/" + scores.Count;
        _GameGui.scoresAll.text = "";
        foreach (var p in _Game.players.OrderByDescending(a => a.scores))
        {
            if (p != _Player)
                _GameGui.scoresAll.text += p.nick + ":" + p.scores + "/" + scores.Count + "\r\n";

            if (Network.isServer)
                if (p.scores == scores.Count && !pause && scores.Count > 0)
                {                    
                    _Game.networkView.RPC("WinGame", RPCMode.All, p.nick);
                }
        }


    }
    private void UpdateOther()
    {
        leftdown.text = "";
    }
}