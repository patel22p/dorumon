using System;
using UnityEngine;
using doru;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

public class Gamebs : bs
{
    
    internal Vector3 spawn;
    internal Transform level;
    public EGame EditorPrefab;
    internal List<Tool> tools { get { return EditorPrefab.tools; } }
    public override void Awake()
    {

        for (int i = 0; i < tools.Count; i++)
            tools[i].toolid = i;
        LoadMap();
        base.Awake();
    }
    public void LoadMap()
    {
         var o = GameObject.Find("Level");
         if (o != null) Destroy(o);
        level = new GameObject("Level").transform;

        Debug.Log("Loading Map");
        try
        {
            var db = (DB)DB.xml.Deserialize(new StringReader(Map));

            foreach (Tooldb t in db.tools)
            {
                var w = (bs)Instantiate(tools[(int)t.toolid]);
                w.pos = t.Pos;
                w.rot = t.rot;                
                w.scale = t.scale;
                w.transform.parent = level;
                var txt = w.GetComponent<TextMesh>();
                if (txt != null)
                    txt.text = t.text;
                var score = w.GetComponent<Score>();
                if (score != null)
                    score.spawn = t.spawn;
                var wall = w.GetComponent<Wall>();
                if (wall != null)
                    wall.SpeedTrackVell = t.speedTrackVell;
                var anim = w.GetComponent<AnimHelper>();
                if (anim != null)
                    anim.animationSpeedFactor = t.animSpeedFactor;
            }
            spawn = db.startpos;
        }
        catch (XmlException e) { Debug.Log(e); }
    }
    
    public void SaveLevel()
    {
        var db = new DB();
        db.startpos = spawn;
        foreach (Transform t in level)
        {
            var w = t.GetComponent<Tool>();
            var b = new Tooldb { toolid = w.toolid, Pos = w.pos, rot = w.rot, scale = w.scale };

            var txt = t.GetComponent<TextMesh>();
            if (txt != null)
                b.text = txt.text;
            var score = t.GetComponent<Score>();
            if (score != null)
                b.spawn = score.spawn;
            var anim = _EGame.SelectedPrefab.GetComponent<AnimHelper>();
            if (anim != null)
            {
                b.animSpeedFactor = anim.animationSpeedFactor;
            }

            var wall = t.GetComponent<Wall>();
            if (wall != null)
                b.speedTrackVell = wall.SpeedTrackVell;

            db.tools.Add(b);
            
        }

        using (var ms = new MemoryStream())
        {
            DB.xml.Serialize(ms, db);
            ms.Position = 0;
            Map = new StreamReader(ms).ReadToEnd();
            
        }
    }
    public string Map { get { return PlayerPrefs.GetString("Map"); } set { PlayerPrefs.SetString("Map", value); } }
}