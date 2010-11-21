using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class ShadowToTexture : MonoBehaviour
{
    public GameObject g;
    public Renderer[] rl;
    public List<Texture> tl = new List<Texture>();
    void Awake()
    {
        g = GameObject.Find("Level");
    }
    void Start()
    {
        rl = g.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rl)
            foreach (Material m in r.materials)
                tl.Add(m.mainTexture);
    }
    void OnPreRender()
    {

        foreach (Renderer r in rl)
            foreach (Material m in r.materials)
                m.mainTexture = null;
    }
    void OnPostRender()
    {
        int i = 0;
        foreach (Renderer r in rl)
            foreach (Material m in r.materials)
            {
                m.mainTexture = tl[i];
                i++;
            }

    }

}
