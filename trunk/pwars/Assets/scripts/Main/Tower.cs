using UnityEngine;
using System.Collections;
//[RequireComponent(typeof(NetworkView), typeof(AudioListener))]
public class Tower : IPlayer
{
    
    public Detonator dt;
    [RPC]
    public override void RPCSetLife(int NwLife, int killedby)
    {
        base.RPCSetLife(NwLife, killedby);
    }

    [RPC]
    public override void RPCDie(int killedby)
    {
        Alive = false;
        dt.autoCreateForce = false;
        GameObject g = (GameObject)Instantiate(dt.gameObject, pos, rot);
        var e = g.AddComponent<Explosion>();
        e.exp = 3000;
        e.radius = 8;
        e.damage = 50;
        RPCShow(false);
        
    }

    public override bool isEnemy(int killedby)
    {
        return true;
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        RPCShow(enabled);
        base.OnPlayerConnected1(np);
    }

}

public enum TowerType {  } 