using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {

    public Player pl;
	void Start () {

        this.GetComponentInChildren<Animation>()["Score"].normalizedTime = Random.value;
	}
	
	void Update () {
        var dist = Vector3.Distance(pl.transform.position, this.transform.position);
        var d = 5;
        if (dist < d)
        {
            //Debug.Log("asd");
            var norm = (pl.transform.position - transform.position).normalized;
            transform.position += norm * (d - dist) * .2f;
            //animation.Stop();
            //transform.position += norm;
        }
        if (dist < .5f)
        {
            Destroy(this.gameObject);
        }
	}
    public void Destroy()
    {

    }
}
