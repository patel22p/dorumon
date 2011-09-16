using System.Linq;
using UnityEngine;
using System.Collections;
using System;

public class ObsCamera : Bs {

    public Player pl;
    public enum CamMode { firstPerson, thirdPerson, Free }
    public CamMode camMode;
    internal float KilledByTime = float.MinValue;
    public bool obsHack;
    public void LateUpdate()
    {
        if (DebugKey(KeyCode.O))
            obsHack = !obsHack;
        if (Time.time - KilledByTime < 5 && pl!=null)
        {
            tr.LookAt(pl.pos);
            return;
        }
        if (_Player != null && !_Player.dead && !obsHack)
        {
            pl = _Player;
            camMode = CamMode.firstPerson; 
        }
        else
            UpdateSpectatorMode();

        var t = TimeSpan.FromMilliseconds(_Game.GameTime);
        if (camMode == CamMode.firstPerson)
        {
            _Hud.SetPlayerHudActive(true);
            _Hud.SetSpectatorHudActive(false);
            pos = pl.camera.camera.transform.position;
            rot = pl.camera.camera.transform.rotation;
            _Hud.life.text = "b" + pl.Life;
            _Hud.shield.text = "a" + pl.Shield;
            _Hud.money.text = "$" + pl.PlayerMoney;
            _Hud.time.text = "e" + t.Minutes + ":" + t.Seconds;
            SetRenderers(pl);
        }
        else
        {
            _Hud.SetPlayerHudActive(false);
            _Hud.SetSpectatorHudActive(true);
            _Hud.SpecInfo.text = t.Minutes + ":" + t.Seconds + "   " + _Game.PlayerMoney;
            if (camMode == CamMode.Free)
            {
                Vector3 move = GetMove() * Time.deltaTime * 10;
                camera.transform.position += transform.rotation * move;
                rote += GetMouse();
            }
            else if (camMode == CamMode.thirdPerson)
            {
                rote += GetMouse();
                pos = pl.pos + rot * Vector3.back * 3;
            }    
        }
        
    }

    private void UpdateSpectatorMode()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            camMode++;
            camMode = (CamMode)Clamp2((int)camMode, Enum.GetNames(typeof(CamMode)).Length - 1);
            _Hud.PrintPopup("Camera Mode: " + camMode);
        }

        if (Input.GetMouseButtonDown(0) && camMode != CamMode.Free)
        {
            pl = _Game.AlivePlayers.Next(pl);
            _Hud.PrintPopup("Following: " + pl.name);
        }
        if (_Game.AlivePlayers.Count() == 0)
            camMode = CamMode.Free;
        else if (pl == null)
            pl = _Game.Players.First();
    }

    private void SetRenderers(Player pl)
    {
        pl.observing = true;
        pl.SetGunRenderersActive(true);
        pl.SetPlayerRendererActive(false);        
    }
    public static int Clamp2(int a, int max)
    {
        if (a > max)
            return 0;
        if (a < 0)
            return max;
        return a;
    }
    
}
