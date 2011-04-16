using System.Linq;
using UnityEngine;
using System.Collections;
using gui = UnityEditor.EditorGUILayout;
using GUI = UnityEngine.GUILayout;
using UnityEditor;
using System.Collections.Generic;
public class Game : bs
{


    [FindTransform]
    public GUIText score;

    public int scores;

    public PowerType powerType;
    [FindTransform]
    public GUITexture powerIcon;

    public Texture2D[] powerTextures;
    internal float powerTime;
    public override void Init()
    {
        IgnoreAll("Nut", "Level");
        
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Nut"), ~(1 << LayerMask.NameToLayer("Level")));
        base.Init();
    }
    void Awake()
    {

    }
    void Start()
    {

    }

    void Update()
    {
        UpdatePower();

        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;
        score.text = scores + "";
    }

    private void UpdatePower()
    {
        var c = powerIcon.color;
        c.a = Mathf.Max(0, powerTime) / 20;
        powerIcon.color = c;

        if (powerType != PowerType.none)
            powerIcon.texture = powerTextures[(int)powerType];

        powerTime -= Time.deltaTime;
        if (powerTime < 0)
            powerType = PowerType.none;
    }

}
