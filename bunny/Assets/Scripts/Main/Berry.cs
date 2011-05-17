using System;
using UnityEngine;
using System.Linq;
public enum BerryType { Berry, Nut } 
public class Berry : bs
{
    public BerryType berryType;
    public int points = 1;
    float timetoactive=2;
    public Animation anim;
    void Awake()
    {
        if (disableScripts)
            enabled = false;
    }
    void Start()
    {
        if (anim != null)
        {
            var r = anim["rotate"];
            r.time = UnityEngine.Random.Range(0, r.length);
        }
    }
    
    void Update()
    {
        timetoactive -= Time.deltaTime;
        if (timetoactive > 0)
            return;
        if (mindist < 4 || _Game.timer.TimeElapsed(1000))
            UpdateBerry();
    }
    float mindist;
    private void UpdateBerry()
    {
        mindist = float.MaxValue;
        foreach (var p in _Game.shareds)
        {
            var dist = Vector3.Distance(p.pos, this.pos);
            mindist = Mathf.Min(dist, mindist);
            var d = 3;
            if (dist < d)
            {
                var norm = (p.pos - transform.position).normalized;
                transform.position += norm * (d - dist) * Time.deltaTime * 5;
            }
            if (dist < 0.25f)
            {
                if (p is Player)
                {
                    _Game.scores += points;
                    _Game.score.animation.Play();
                }
                if (berryType == BerryType.Berry)
                    p.berries++;
                else
                    p.nuts++;
                Destroy(this.gameObject);
            }
        }
    }
}