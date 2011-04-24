using System;
using UnityEngine;
using System.Linq;
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
    }

    private void UpdatePointer()
    {
        var scr = _Game.scores.Where(a => a != null).OrderBy(a => Vector3.Distance(a.pos, _Player.pos)).FirstOrDefault();
        if (scr != null)
            _Cam.Pointer.rotation = Quaternion.Lerp(_Cam.Pointer.rotation, Quaternion.LookRotation(scr.pos - _Cam.pos), .2f);
    }
    //Quaternion fake;
    private void UpdateScoreBoard()
    {
        if (Input.GetKey(KeyCode.E) || _Game.pause)
        {
            string t = "{0,-20}{1,-10}{2,-10}\r\n";
            string text = "";
            text += string.Format(t, "Name", "Scores", "TotalScores");
            text += "______________________________________________\r\n";
            foreach (var p in _Game.players)
                text += string.Format(t, p.nick, p.scores + "/" + _Game.scores.Count, p.totalscores);
            tabble.text = text;
        }
        else
            tabble.text = "";
    }
    private void UpdatePlayerScores()
    {
        
        var scores = _Game.scores;
        
        var pause = _Game.pause;
        //_GameGui.scores.text = _Player.nick + ":" + _Player.scores + "/" + scores.Count;
        _GameGui.scores.text = _Player.scores + "/" + scores.Count;
        _GameGui.scoresAll.text = "";
        foreach (var p in _Game.players)
        {
            if (p != _Player)
                _GameGui.scoresAll.text += p.nick + ":" + p.scores + "/" + scores.Count + "\r\n";

            if (p.scores == scores.Count && !pause && Network.isServer && scores.Count > 0)
                _Game.networkView.RPC("WinGame", RPCMode.All);
        }


    }
    private void UpdateOther()
    {
        leftdown.text = "";
    }
}