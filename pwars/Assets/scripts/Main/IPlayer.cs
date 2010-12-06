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
    public List<Vector3> plPathPoints = new List<Vector3>();
    public int Life;
    public Transform CamPos;
    public bool dead { get { return !Alive; } set { Alive = !value; } }
    public bool Alive;
    
    float shownicktime;
    public Team? team
    {
        get
        {
            if (OwnerID == -1) return null;
            else return _Game.players[OwnerID.GetHashCode()].team;
        }
    }
    public override void Init()
    {        
        base.Init();
        title = transform.GetComponentInChildren<TextMesh>();
        nitro = 10;
    }
    protected override void Awake()
    {        
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
        if (!(this is Zombie))
        {
            if (isOwner)
                nitro += Time.deltaTime / 5;

            UpdateTitle();
        }
        base.Update();
    }
    private void UpdateTitle()
    {
        if (title != null)
        {
            if (OwnerID != -1 && (team == Team.Red || team == Team.Blue))
                title.renderer.material.color = team == Team.Red ? Color.red : Color.blue;
            else
                title.renderer.material.color = Color.white;
        }

        if (title != null && _TimerA.TimeElapsed(50) && _Game.cameraActive)
        {

            if (!isOwner && _localPlayer != null && ((_localPlayer.team == team && !mapSettings.DM) || IsPointed(1 << LayerMask.NameToLayer("Player"), 10)))
                shownicktime = 2;
            if (OwnerID != -1 && shownicktime > 0)
                title.text = _Game.players[OwnerID].nick + ":" + Life;
            else
                title.text = "";
        }

        shownicktime -= Time.deltaTime;
    }
    
    [RPC]
    public virtual void RPCSetLife(int NwLife, int killedby)
    {
        if (dead) return;        
        if(isController) CallRPC(NwLife,killedby);

        if (isEnemy(killedby) || NwLife > Life)
            Life = NwLife;

        if (Life <= 0 && isController)
            RPCDie(killedby);
    }
    public bool isEnemy(int killedby)
    {
        if (this is Zombie) return true;
        if (killedby == OwnerID) return true;
        if (killedby == -1) return true;
        if (mapSettings.DM) return true;
        if (killedby != -1 && players[killedby] != null && players[killedby].team != team) return true;        
        return false;    
    }
    [RPC]
    public abstract void RPCDie(int killedby);
}
