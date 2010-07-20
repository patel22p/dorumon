using UnityEngine;
using System.Collections;

public class Player : Base
{
    public string Nick;
    public int Life;
    public int score;
    public GunBase[] gunlist { get { return this.GetComponentsInChildren<GunBase>(); } }
    public bool isdead;
    protected override void Start()
    {
        if (isMine)
        {
            RPCSetID(Network.player);
            RPCSpawn();
        }
        
    }

    [RPC]
    public void RPCSpawn()
    {
        CallRPC();
        Show(true);        
        foreach (GunBase gunBase in gunlist)
            gunBase.Reset();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        Life = 100;        
        transform.position = SpawnPoint(int.Parse(OwnerID.Value.ToString()));
        
    }
    protected override void Update()
    {        
                
    }
    public override void OnSetID()
    {
        if (isMine)
            name = "LocalPlayer";
        else
            name = "RemotePlayer" + OwnerID;
    }

    [RPC]
    public void RPCSetID(NetworkPlayer player)
    {        
        CallRPC(player);
        foreach (Base a in GetComponentsInChildren(typeof(Base)))
        {
            a.OwnerID = player;
            a.OnSetID();
        }
    }

    public static Spawn spawn { get { return Find<Spawn>(); } }
    public static Vector3 SpawnPoint(int i)
    {
        //Random.Range(0, spawn.transform.childCount);
        return spawn.transform.GetChild(i).transform.position;
    }
}
