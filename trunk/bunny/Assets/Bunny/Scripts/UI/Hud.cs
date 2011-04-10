using UnityEngine;
using System.Collections;

public class Hud : MonoBehaviour
{

    #region HudSingleton (Static Get)
    public static Hud Instance
    {
        get
        {
            if (!_instance)
            {
                // Create instance
                _instance = (Hud)GameObject.FindObjectOfType(typeof(Hud));
                if (!_instance)
                {
                    GameObject container = new GameObject();
                    container.name = "HUD Manager";
                    _instance = container.AddComponent(typeof(Hud)) as Hud;
                }
            }

            return _instance;
        }
    }
    private static Hud _instance;
    #endregion

    public Texture2D BerryTXT, HeartTXT, FrameTXT;
    public int IconSize;
    public Vector2 BerryPosition, HeartPosition;

    private Vector2 berryPos, heartPos;

    private int displayHP = 0;
    private int displayScore = 0;

    public GUIStyle HUDFont;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // This gameobject.
    }

    void Start()
    {
        // Calculate the positions relative to the screen size.
        berryPos = new Vector2((Screen.width * BerryPosition.x) - (IconSize / 2), (Screen.height * BerryPosition.y) - (IconSize / 2));
        heartPos = new Vector2((Screen.width * HeartPosition.x) - (IconSize / 2), (Screen.height * HeartPosition.y) - (IconSize / 2));
    }

    void Update()
    {
        switch (GameData.Instance.GameState)
        {
            case GAMESTATE.GAME:
                UpdateHUD();
                break;
            default:
                break;
        }
    }

    void UpdateHUD()
    {
        // We want to update the displayed data.
        if (PlayerController.Instance != null)
        {
            displayHP = PlayerController.Instance.GetHP();
            displayScore = PlayerController.Instance.GetScore();
        }
        else
        {
            Debug.LogWarning("Trying to access playercontroller eventhough it does not exsist.. returns null ;(");
        }
    }

    // Display shit.
    void OnGUI()
    {
        if (GameData.Instance.GameState != GAMESTATE.STARTMENU)
        {
            GUI.DrawTexture(new Rect(berryPos.x, berryPos.y, IconSize, IconSize), BerryTXT);
            GUI.DrawTexture(new Rect(heartPos.x, heartPos.y, IconSize, IconSize), HeartTXT);

            GUI.Label(new Rect(berryPos.x, berryPos.y, IconSize, IconSize), displayScore.ToString(), HUDFont);
            GUI.Label(new Rect(heartPos.x, heartPos.y, IconSize, IconSize), displayHP.ToString(), HUDFont);
        }
    }
}