using UnityEngine;
using System.Collections;
[RequireComponent(typeof(NetworkView), typeof(AudioListener))]
public class Tower : Base
{
    public int life = 1;
    public Detonator dt;
    [RPC]
    public void SetLife(int life)
    {
        this.life = life;
        if (life < 0)
        {
            dt.autoCreateForce = false;
            GameObject g =(GameObject)Instantiate(dt.gameObject, pos, rot);
            var e = g.AddComponent<Explosion>();
            e.exp = 3000;
            e.radius = 8;
            e.damage = 50;
            RPCShow(false);
        }
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        RPCShow(enabled);
        base.OnPlayerConnected1(np);
    }

}

public enum TowerType {  } 