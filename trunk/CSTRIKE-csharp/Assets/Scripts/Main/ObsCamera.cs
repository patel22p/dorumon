using doru;
using System.Linq;
using UnityEngine;
using System.Collections;
using System;

public enum CamMode { thirdPerson2, thirdPerson, firstPerson, Free }
public class ObsCamera : Bs
{
    public Player pl;
    public CamMode camMode;
    internal float KilledByTime = float.MinValue;
    //bool thirdPerson = true;
    Vector3 xy;
    public void LateUpdate()
    {
       //note fix follow bot no gui
       //note fix follow bot - cursor
       //note killby camera
        if (Offline) return;
        if (Input.GetKeyDown(KeyCode.F1))
        {
            camMode = CamMode.firstPerson;
            _Hud.PrintPopup("Camera Mode: " + camMode);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            camMode = _Player != null ? CamMode.thirdPerson2 : CamMode.thirdPerson;
            _Hud.PrintPopup("Camera Mode: " + camMode);
        }

        if (_Player != null)
        {
            pl = _Player;
            if (camMode == CamMode.Free || camMode == CamMode.thirdPerson)
                camMode = CamMode.firstPerson;
        }
        else
        {
            
            if (_Game.Players.Count() == 0)
                camMode = CamMode.Free;
            else if (pl == null)
                pl = _Game.Players.First();

            if (Input.GetKeyDown(KeyCode.Space))
            {                
                camMode = new[] { CamMode.Free, CamMode.thirdPerson, CamMode.firstPerson }.Next(camMode); 
                //camMode = (CamMode)Clamp2((int)camMode, Enum.GetNames(typeof(CamMode)).Length - 1);
                _Hud.PrintPopup("Camera Mode: " + camMode);
            }

            if (Input.GetMouseButtonDown(0) && camMode != CamMode.Free)
            {
                pl = _Game.Players.Next(pl);
                _Hud.PrintPopup("Following: " + pl.pv.PlayerName);
            }
        }

        var t = TimeSpan.FromMilliseconds(_Game.GameTime);
        if ((camMode == CamMode.firstPerson || camMode == CamMode.thirdPerson2) && pl.pv !=null)
        {
            if (pl == null) return;
            _Hud.SetPlayerHudActive(true);
            _Hud.SetSpectatorHudActive(false);            
            _Hud.life.text = "b" + pl.Life;
            _Hud.shield.text = "a" + pl.Shield;
            _Hud.money.text = "$" + pl.pv.PlayerMoney;
            _Hud.time.text = "e" + t.Minutes + ":" + t.Seconds;
            if (_Game.BombPlace != null && pl.c4.patrons > 0)
            {
                _Hud.bomb.enabled = true;
                _Hud.bomb.material.color = Time.time % .4 < .2f && (_Game.BombPlace.position - pl.pos).magnitude < 5 ? Color.red : Color.green;
            }
            else
                _Hud.bomb.enabled = false;
                        
            _Hud.Patrons.text = pl.gun.patrons + "|   " + pl.gun.globalPatrons;
            if (camMode == CamMode.firstPerson)
            {
                SetRenderers(pl);
                pos = pl.camera.camera.transform.position;
                rot = pl.camera.camera.transform.rotation;
            }
            else if (camMode == CamMode.thirdPerson2)
            {                
                rote += GetMouse();
                pos = pl.pos + Vector3.up * 2f + rot * Vector3.back * 3;
            }
        }        
        else
        {
            _Hud.SetPlayerHudActive(false);
            _Hud.SetSpectatorHudActive(true);
            
            
            _Hud.SpecInfo.text = t.Minutes + ":" + t.Seconds + "   " + _Game.pv.PlayerMoney;
            if (camMode == CamMode.Free)
            { 
                Vector3 move = GetMove() * Time.deltaTime * 10;
                camera.transform.position += transform.rotation * move;
                rote += GetMouse();
            }
            else if (camMode == CamMode.thirdPerson)
            {                
                if (pl == null) return;
                rote += GetMouse();
                pos = pl.pos + Vector3.up * 1.5f + rot * Vector3.back * 3;                
            }
        }

    }
    private void SetRenderers(Player pl)
    {
        pl.observing = true;
        pl.SetGunRenderersActive(true);
        pl.SetPlayerRendererActive(false);
        
    }
    

    public void OnRenderObject()
    {
        if ((camMode == CamMode.firstPerson || camMode == CamMode.thirdPerson2) && pl != null)
        {
            LineMaterial.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            GL.Color(Color.green);
            foreach (var a in list)
            {
                var v = new Vector3(Screen.width, Screen.height) / 2 + new Vector3(a.x, a.y);
                v += a.normalized * (1 + pl.gun.cursorOffset);
                v.x /= Screen.width;
                v.y /= Screen.height;
                GL.Vertex(v);
            }
            GL.End();
        }
    }
    static Material lineMaterial;
    static Material LineMaterial
    {
        get
        {
            if (!lineMaterial)
            {
                lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                    "SubShader { Pass { " +
                    "    Blend SrcAlpha OneMinusSrcAlpha " +
                    "    ZWrite Off Cull Off Fog { Mode Off } " +
                    "    BindChannels {" +
                    "      Bind \"vertex\", vertex Bind \"color\", color }" +
                    "} } }") { hideFlags = HideFlags.HideAndDontSave, shader = { hideFlags = HideFlags.HideAndDontSave } };
            }
            return lineMaterial;
        }
    }
    const float d = 3, len = 5;
    Vector3[] list = new[]{ 
            Vector3.left *d, Vector3.left*(d+len),
            Vector3.up *d, Vector3.up*(d+len),
            Vector3.right *d, Vector3.right*(d+len),
            Vector3.down *d, Vector3.down*(d+len)};

}
