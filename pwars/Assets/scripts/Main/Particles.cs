using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Particles : MonoBehaviour
{
    ParticleEmitter[] emitors;
    void Awake()
    {
        emitors = this.GetComponentsInChildren<ParticleEmitter>();
    }
    public void Emit(Vector3 pos, Quaternion rot) { Emit(pos, rot, Vector3.zero); }
    
    public void Emit(Vector3 pos, Quaternion rot, Vector3 vel)
    {
        Transform obj = this.transform;
        obj.position = pos;
        obj.rotation = rot;
        if (obj.audio != null) obj.audio.Play();
        foreach (ParticleEmitter emitor in emitors)
        {
            if (emitor.particleCount < 50)
            {
                emitor.worldVelocity = vel;
                emitor.Emit();
            }
        }
    }

    
    void Update()
    {
        
    }
}
