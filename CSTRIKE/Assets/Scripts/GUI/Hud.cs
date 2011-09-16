using UnityEngine;
using System.Collections;
using System;

public class Hud : Bs {
    public GUIText life;
    public GUIText shield;
    public GUIText time;
    public GUIText money;
    public GUIText ScoreBoard;
    public Transform SpectatorHUD;    
    public Transform PlayerHUD;    
    public GUIText PlayerName;
    public GUIText SpecInfo;
    public GUIText SpecPlayerName;
    public GUIText PopupText;
    public GUIText KillText;
    public GUITexture PainLeft;
    public GUITexture PainRight;

    private bool PlayerHudActived=true;
    private bool SpectatorHudActived=true;    
    public void PrintPopup( string text)
    {
        PopupText.text = text;
        PopupText.animation.Rewind();
        PopupText.animation.Play();
    }
    public void Update()
    {
        SetPainRight(-Time.deltaTime * Temp);
        SetPainLeft(-Time.deltaTime * Temp);
    }
    
    public void SetPainRight(float v)
    {
        var c = PainRight.color;
        c.a = Mathf.Clamp(c.a + v, 0, 1);
        PainRight.color = c;
    }

    public void SetPainLeft(float v)
    {
        var c = PainLeft.color;
        c.a = Mathf.Clamp( c.a + v,0,1);
        PainLeft.color = c;
    }

    public void SetPlayerHudActive(bool value)
    {
        if (PlayerHudActived != value)
        {
            PlayerHudActived = value;
            _Hud.PlayerHUD.SetActive(value);
        }
    }
    public void SetSpectatorHudActive(bool value)
    {
        if (SpectatorHudActived != value)
        {
            SpectatorHudActived = value;
            _Hud.SpectatorHUD.SetActive(value);
        }
    }
}
