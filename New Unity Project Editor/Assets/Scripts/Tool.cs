using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class Tool : bs2 {

	void Start () {
        
	}
    public override void Init()
    {
        var Texture = UnityEditor.EditorUtility.GetAssetPreview(this);
        File.WriteAllBytes("Assets/" + gameObject.name + ".png", Texture.EncodeToPNG());
        base.Init();
    }
    public Texture2D Texture;
	void Update () {
	    
	}
}
