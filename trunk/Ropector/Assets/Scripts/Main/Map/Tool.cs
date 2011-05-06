using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public class Tool : bs2 {

	void Start () {

    }

    public override void Init()
    {
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
        var Texture = UnityEditor.EditorUtility.GetAssetPreview(this);
        File.WriteAllBytes("Assets/" + gameObject.name + ".png", Texture.EncodeToPNG());
#endif
        base.Init();
    }

    public Texture2D Texture;
    void Update()
    {
	}
    public Tooldb Save()
    {
        return new Tooldb { Pos = pos, q = rot, scale = scale };
    }

}
[Serializable]
public class Tooldb
{
    public Vector3 Pos;
    public Vector3 scale;
    public Quaternion q;
}
[Serializable]
public class DB
{
    public static XmlSerializer xml = new XmlSerializer(typeof(DB), new[] { typeof(Tooldb) });
    public List<Tooldb> tools = new List<Tooldb>();
}