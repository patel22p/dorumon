using System.Linq;
using UnityEngine;
using System.Collections;
using System;

public class ObsCamera : Bs
{

    public Player pl;
    public enum CamMode { thirdPerson2, thirdPerson, topDown, firstPerson, Free }
    public CamMode camMode;
    internal float KilledByTime = float.MinValue;
    //bool thirdPerson = true;
    Vector3 xy;
    public void LateUpdate()
    {
        //todo add 3nd camera
        if (DebugKey(KeyCode.F1))
        {
            camMode = CamMode.firstPerson;
            _Hud.PrintPopup("1nd ps Camera");
        }
        if (DebugKey(KeyCode.F2))
        {
            camMode = CamMode.thirdPerson2;

            _Hud.PrintPopup("3nd ps Camera");
        }
        if (DebugKey(KeyCode.F3))
        {
            camMode = CamMode.topDown;
            _Hud.PrintPopup("TopDown Camera");
        }
        if (_Player != null && !_Player.dead)
        {
            pl = _Player;
            if (camMode == CamMode.Free || camMode == CamMode.thirdPerson)
                camMode = CamMode.firstPerson;
        }
        else
            UpdateSpectatorMode();

        var t = TimeSpan.FromMilliseconds(_Game.GameTime);
        if (camMode == CamMode.firstPerson || camMode == CamMode.thirdPerson2 || camMode == CamMode.topDown)
        {
            _Hud.SetPlayerHudActive(true);
            _Hud.SetSpectatorHudActive(false);            
            _Hud.life.text = "b" + pl.Life;
            _Hud.shield.text = "a" + pl.Shield;
            _Hud.money.text = "$" + pl.PlayerMoney;
            _Hud.time.text = "e" + t.Minutes + ":" + t.Seconds;
            if (!pl.gun.handsReload.enabled)
                _Hud.Patrons.text = pl.gun.patrons + "|   30";
            if (camMode == CamMode.firstPerson)
            {
                SetRenderers(pl);
                pos = pl.camera.camera.transform.position;
                rot = pl.camera.camera.transform.rotation;
            }
            else if (camMode == CamMode.topDown)
            {
                posy = pl.posy + 6;
                rot = Quaternion.LookRotation(Vector3.down);
                xy += GetMouse();
                posx = pl.posx + xy.y / 20f;
                posz = pl.posz + -xy.x / 20f;

            }
            else if (camMode == CamMode.thirdPerson2)
            {
                rote += GetMouse();
                pos = pl.pos + Vector3.up + rot * Vector3.back * 3;
            }
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
    Vector3 MouseRot;

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
            pl = _Game.AlivePlayers.First();
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
