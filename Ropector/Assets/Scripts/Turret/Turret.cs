using System;
using UnityEngine;

public class Turret : bs
{
    void Update()
    {
        tmShoot -= Time.deltaTime;
        var v = Player.pos - pos;
        var r = new Ray(pos, v);
        RaycastHit h;
        if (!Physics.Raycast(r, out h, v.magnitude, ~(1 << 0)))
        {
            onLook();
        }
        turret.transform.LookAt(Player.pos);
            //OnColl(h.point, h.transform);
    }
    float tmShoot;
    private void onLook()
    {
        if (tmShoot < 0)
        {
            tmShoot = .1f;
            Shoot();
        }
    }

    private void Shoot()
    {
        
    }
    public bs turret;
}