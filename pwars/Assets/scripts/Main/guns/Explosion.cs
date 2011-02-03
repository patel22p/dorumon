using UnityEngine;
using System.Collections;

public class Explosion : bs
{

    public Shared self;
    public AnimationCurve damage;
    internal float exp = 500;
    internal float radius = 1;
    internal float plDamageFactor = 1;
    internal float zmDamageFactor = 1; 
    public float DamageFactor = 1;
    public float damageFactor(Object o)
    {
        return _Game.mapSettings.damageFactor * (o is Zombie ? zmDamageFactor : 1) * (o is Player ? plDamageFactor : 1);
    }
    public void Start()
    {
        
        foreach (Destroible ip in GameObject.FindObjectsOfType(typeof(Destroible)))
        {
            float dist = Vector3.Distance(ip.transform.position, transform.position);
            
            if (ip != self && dist < radius && ip.isController && ip.Alive)
            {
                if (ip.isOwner)
                    _Cam.exp = 1;
                if (ip.isEnemy(OwnerID) || ip.OwnerID == _localPlayer.OwnerID)
                {
                    ip.RPCSetLife(ip.Life - damage.Evaluate(dist) * DamageFactor * 4 * damageFactor(ip), OwnerID);
                }
            }
        }
        //_TimerA.AddMethod(10, delegate
        {
            foreach (Shared b in GameObject.FindObjectsOfType(typeof(Shared)))
                if (b != self && (!(b is Player) || ((Player)b).Alive))
                {
                    b.rigidbody.AddExplosionForce(exp * b.rigidbody.mass * .3f * fdt, transform.position, radius * 3);
                }
        }
        //);
    }
}
