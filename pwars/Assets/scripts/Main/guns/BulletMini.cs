using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletMini : BulletBase
{
    public float velocity = 100;
    public int damage=34;
    
    public float exp=50;
    public float exprad = 2; 
    protected override void Hit(RaycastHit hit )
    {

        base.Hit(hit);
        Destroy(gameObject);
        IPlayer iplayer = hit.collider.gameObject.transform.root.GetComponent<IPlayer>();
        if (iplayer as Player != null || iplayer as Zombie != null && _SettingsWindow.Blood)
            _Game.Emit(_Game.BloodEmitors, _Game.Blood, hit.point, transform.rotation);            
        else if (iplayer as IPlayer != null)
            _Game.Emit(_Game.impactSparkEmiters, _Game.impactSpark, hit.point, transform.rotation);            
        else
            _Game.Emit(_Game.metalSparkEmiters, _Game.metalSpark, hit.point,transform.rotation);
        
        if (iplayer != null && iplayer.isController && !iplayer.dead)
        {
            if (iplayer is Player)
                ((Player)iplayer).freezedt = 40;
            iplayer.RPCSetLife(iplayer.Life - damage, OwnerID);
        }
        
    }

    protected override void FixedUpdate()
    {

        this.transform.position += transform.TransformDirection(Vector3.forward) * velocity * Time.deltaTime;
        base.FixedUpdate();
    }


}