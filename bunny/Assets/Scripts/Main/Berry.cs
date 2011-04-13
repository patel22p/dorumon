using System;
using UnityEngine;

public class Berry : bs
{
    public int points = 1;
    float timetoactive=2;
    public Animation anim;
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
        //transform.Rotate(Vector3.up * Time.deltaTime * 100);
        timetoactive -= Time.deltaTime;
        if (timetoactive > 0)
            return;
        var dist = Vector3.Distance(_Player.pos, this.pos);
        var d = 3;
        if (dist < d)
        {
            var norm = (_Player.pos - transform.position).normalized;
            transform.position += norm * (d - dist) * Time.deltaTime * 5;
        }
        if (dist < 0.25f)
        {
            _Game.scores += points;
            _Game.score.animation.Play();
            Destroy(this.gameObject);
        }
    }
}