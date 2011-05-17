using System.Linq;
using UnityEngine;
using System.Collections;
using gui = UnityEditor.EditorGUILayout;
using GUI = UnityEngine.GUILayout;
using UnityEditor;
using System.Collections.Generic;
using doru;
public class Game : bs
{
    public List<Shared> shareds = new List<Shared>();
    [FindTransform]
    public GUIText score;
    public int scores;
    public PowerType powerType;
    [FindTransform]
    public GUITexture powerIcon;
    public GUITexture[] lifeIcon;
    public Texture2D[] powerTextures;
    internal float powerTime;
    public TimerA timer = new TimerA();

    [FindAsset("Nut_1p")]
    public GameObject nutPrefab;
    [FindAsset("Berry_5p")]
    public GameObject berryPrefab;

    public override void Init()
    {
        IgnoreAll("Nut", "Level");
        IgnoreAll("Dead", "Level");
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Nut"), ~(1 << LayerMask.NameToLayer("Level")));
        base.Init();
    }
    void Awake()
    {
        //Time.maximumDeltaTime = 1 / 3;
    }
    void Start()
    {

    }
    void Update()
    {
        for (int i = 0; i < lifeIcon.Length; i++)
            lifeIcon[i].enabled = _Player.life > i;
         
        UpdatePower();

        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;


        score.text = scores + "";
        timer.Update();
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
