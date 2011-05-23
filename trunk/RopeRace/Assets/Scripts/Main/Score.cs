using UnityEngine;
using System.Collections;

public class Score : bs {

    public bool spawn = true;
    Vector3 spawnpos;
	void Start () {
        spawnpos = pos;
        _Game.scores.Add(this);
        this.GetComponentInChildren<Animation>()["Score"].normalizedTime = Random.value;
	}
    
    
	void Update () {
        if (_Player != null)
        {
            var dist = Vector3.Distance(_Player.transform.position, this.transform.position);
            var d = 5;

            var v = _Player.pos - pos;
            if (dist < d && !Physics.Raycast(new Ray(pos, v), v.magnitude, ~(1 << (int)Mask.Player)))
            {
                var norm = (_Player.transform.position - transform.position).normalized;
                transform.position += norm * (d - dist) * Time.deltaTime * 20;
            }
            if (dist < 1)
            {
                if (spawn)
                    _Player.lastpos = spawnpos;
                _GameGui.scores.animation.Play(AnimationPlayMode.Stop);
                _Loader.totalScores++;
                _Player.networkView.RPC("SetScores", RPCMode.All, _Player.scores + 1, _Loader.totalScores);
                Destroy(this.gameObject);
            }
        }
	}
    public void Destroy()
    {

    }
}
