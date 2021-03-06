
using doru;
using System.Linq;
using UnityEngine;
using System.Collections;
using System;


public class ObsCamera : Bs
{
    public Player pl;
    //public CamMode camMode;
    public bool thirdPerson;
    internal float KilledByTime = float.MinValue;
    //bool thirdPerson = true;
    Vector3 xy;
    public SSAOEffect sSAOEffect;
    public ContrastEnhance contrastEnhance;
    public override void Awake()
    {
        if (!_Loader.EnableHighQuality)
        {
            contrastEnhance.enabled = sSAOEffect.enabled = false;
            QualitySettings.currentLevel = QualityLevel.Fastest;
        }
        else if (!Application.isEditor && !Android)
            contrastEnhance.enabled = sSAOEffect.enabled = true;
        base.Awake();
    }

    public void LateUpdate()
    {
        if (Offline) return;

        if (Input.GetKeyDown(KeyCode.C) && !_Game.chatInput.enabled)
        {
            thirdPerson = !thirdPerson;
            _Hud.PrintPopup(thirdPerson ? "3rd person Cam" : "FPS cam");
        }
        
        if (_Player != null)
            pl = _Player;
        else
        {
            if (_Game.Players.Count() == 0)
                return;
            if (pl == null)
                pl = _Game.Players.First();

            if (Input.GetMouseButtonDown(0))
            {
                pl = _Game.Players.Next(pl);
                _Hud.PrintPopup("Following: " + pl.pv.PlayerName);
            }
        }

        var t = TimeSpan.FromMilliseconds(_Game.GameTime);

        
        if (pl != null)
        {
            
            _Hud.SetPlayerHudActive(true);            
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
            if (!thirdPerson)
            {
                SetRenderers(pl);
                pos = pl.camera.camera.transform.position;
                rot = pl.camera.camera.transform.rotation;
            }
            else
            {
                //todo camera collision
                rote += GetMouse();
                pos = pl.pos + Vector3.up * 2f + rot * Vector3.back * 3;
            }

        }        
        else
        {
            _Hud.SetPlayerHudActive(false);
        }
        

    }
    private void SetRenderers(Player pl)
    {
        pl.SetGunRenderersActive(true);
        pl.SetPlayerRendererActive(false);
        pl.observing = true;
    }
    

    public void OnRenderObject()
    {
        if (_Player != null || (!thirdPerson && pl != null))
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
