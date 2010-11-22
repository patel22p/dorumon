using UnityEngine;
using System.Collections;

public class GunHealth : GunBase
{
    public bool power;
    public Renderer render;
    public new Light light;
    protected override void Awake()
    {
        light = GetComponentInChildren<Light>();
        base.Awake();
    }
    
    void Start()
    {
        _Name = "Шприц";
        
    }
    protected override void Update()
    {
        if (power)
        {
            foreach (IPlayer p in _Game.iplayers)
                if (_TimerA.TimeElapsed(500) && isOwner && !p.isOwner && !(p is Zombie) && p.enabled && Vector3.Distance(p.transform.position, this.cursor.position) < 3)
                {
                    p.Health();
                }

            render.enabled = light.enabled = true;
        }
        else
            render.enabled = light.enabled = false;
        base.Update();
    }
    public override void onShow(bool enabled)
    {
        light.enabled = false;        
        base.onShow(enabled);
    }
    protected override void LocalUpdate()
    {

        if (isOwner && enabled)
        {
            if (Input.GetMouseButtonDown(0))
                RPCSetPower(true);
            else if (Input.GetMouseButtonUp(0))
                RPCSetPower(false);
        }

    }
    [RPC]
    public void RPCSetPower(bool enable)
    {
        CallRPC(enable);
        if (enable) PlayRandSound("heal");
        power = enable;
    }

}
