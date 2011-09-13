using UnityEngine;
using System.Collections;
using System;

public class Hud : Bs {
    public GUIText life;
    public GUIText shield;
    public GUIText time;
    public GUIText money;
    public GUIText ScoreBoard;
    public GUIText PlayerName;
	void Update () {
        life.text = "b" + _Player.Life;
        shield.text = "a" + _Player.Shield;
        time.text = "e" + TimeSpan.FromMilliseconds(_Game.GameTime);        
	}
}
