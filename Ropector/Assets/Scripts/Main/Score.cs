using UnityEngine;
using System.Collections;

public class Score : bs {

    
	void Start () {
        _Game.scores.Add(this);
        this.GetComponentInChildren<Animation>()["Score"].normalizedTime = Random.value;
	}
	
	void Update () {
        if (_Player == null) return;
        var dist = Vector3.Distance(_Player.transform.position, this.transform.position);
        var d = 5;
        if (dist < d)
        {
            var norm = (_Player.transform.position - transform.position).normalized;
            transform.position += norm * (d - dist) * Time.deltaTime * 20;
        }
        if (dist < .5f)
        {
            _Player.lastpos = this.pos;
            _GameGui.scores.animation.Play(AnimationPlayMode.Stop);
            _Player.networkView.RPC("SetScores", RPCMode.All, _Player.scores + 1, _Loader.totalScores + 1); 
            Destroy(this.gameObject);
        }
	}
    public void Destroy()
    {

    }
}
