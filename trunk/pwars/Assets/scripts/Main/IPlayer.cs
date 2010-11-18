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
    public GunBase[] guns;
    public int selectedgun = -1;
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
        selectedgun = -1;
        guns = this.GetComponentsInChildren<GunBase>();
        base.Awake();
        
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {        
        base.OnPlayerConnected1(np);
        print();
        if (selectedgun != -1)
            networkView.RPC("RPCSelectGun", np, selected);
        
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

        if (title != null && _TimerA.TimeElapsed(50))
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
    [RPC]
    public void RPCSelectGun(int i)
    {        
        CallRPC(i);
        PlaySound("sounds/change");
        selectedgun = i;
        if(isOwner)
            _GameWindow.tabGunImages = selectedgun;
        foreach (GunBase gb in guns)
            gb.DisableGun();
        guns[i].EnableGun();
    }
    [RPC]
    public virtual void Health()
    {        
    }
    
    //protected override void OnDisable()
    //{
    //    _Game.iplayers.Remove(this);
    //    base.OnDisable();
    //}
    
    
    
    bool IsPointed()
    {
        RaycastHit rah = ScreenRay();
        if (rah.collider == null) return false;
        return Root(rah.collider.gameObject) == this.gameObject;
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
        return this is Zombie 
            || !mapSettings.TeamZombiSurvive && killedby != OwnerID &&
            (killedby == -1 || players[killedby] == null || players[killedby].team != team || mapSettings.DM);
    }
    [RPC]
    public abstract void RPCDie(int killedby);
}
