using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
using System.Collections;

[Serializable]
public abstract class IPlayer : Box
{
    public TextMesh title;
    public float nitro;
    public int Life;
    public Transform CamPos;
    public virtual bool dead { get { return !enabled; } }
    GunBase[] _guns;
    public GunBase[] guns { get { if (_guns == null)  _guns = this.GetComponentsInChildren<GunBase>(); return _guns; } set { _guns = value; } }
    public int selectedgun = 0;
    float shownicktime;
    public Team? team
    {
        get
        {
            if (OwnerID == -1) return null;
            else return _Game.players[OwnerID.GetHashCode()].team;
        }
    }

    protected override void Awake()
    {
        title = transform.GetComponentInChildren<TextMesh>();
        nitro = 10;
        base.Awake();

    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        base.OnPlayerConnected1(np);        
    }
    protected override void Start()
    {
        _Game.iplayers.Add(this);
        base.Start();
    }
    

    protected override void Update()
    {
        if (isOwner)
            nitro += Time.deltaTime / 5;

        if (title != null)
        {
            if (OwnerID != -1 && (team == Team.Red || team == Team.Blue))
                title.renderer.material.color = team == Team.Red ? Color.red : Color.blue;
            else
                title.renderer.material.color = Color.white;
        }

        if (title != null && _TimerA.TimeElapsed(50) && _Game.cameraActive)
        {
            if (!isOwner && _localPlayer != null && ((_localPlayer.team == team && !mapSettings.DM) || IsPointed()))
                shownicktime = 2;
            if (OwnerID != -1 && shownicktime > 0)
                title.text = _Game.players[OwnerID].nick + ":" + Life;
            else
                title.text = "";

        }

        shownicktime -= Time.deltaTime;
        base.Update();
    }
    //[RPC]
    //public void RPCSelectGun(int i)
    //{        
    //    CallRPC(i);
    //    print(pr + i);
    //    PlaySound("change");
    //    selectedgun = i;
    //    if(isOwner)
    //        _GameWindow.tabGunImages = selectedgun;
    //    foreach (GunBase gb in guns)
    //        gb.DisableGun();
    //    guns[i].EnableGun();
    //}
    [RPC]
    public virtual void Health()
    {        
    }

    
    public override void Dispose()
    {
        _Game.iplayers.Remove(this);
        base.Dispose();
    }
    
    
    bool IsPointed()
    {
        RaycastHit rah = ScreenRay();
        if (rah.collider == null) return false;
        return rah.collider.gameObject.transform.root == this.gameObject;
    }
    
    [RPC]
    public virtual void RPCSetLife(int NwLife, int killedby)
    {
        if (!enabled) return;        
        if(isController) CallRPC(NwLife,killedby);

        if (isEnemy(killedby))
            Life = NwLife;

        if (Life <= 0 && isController)
            RPCDie(killedby);
    }
    public bool isEnemy(int killedby)
    {
        //if ( && mapSettings.ZombiSurvive) return false;
        if (this is Player) Debug.Log("isEnemy me" + OwnerID + " patron" + killedby + " myteam"+team +" histeam"+ players[killedby].team);
        if (killedby == OwnerID) return false;        
        if (this is Zombie) return true;
        if (mapSettings.DM) return true;
        if (killedby != -1 && players[killedby] != null && players[killedby].team != team) return true;        
        return false;    
    }
    [RPC]
    public abstract void RPCDie(int killedby);
}
