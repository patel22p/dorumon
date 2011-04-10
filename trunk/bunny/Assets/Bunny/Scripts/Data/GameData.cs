using UnityEngine;
using System.Collections;

public enum GAMESTATE { STARTMENU, PAUSED, GAME, GAMEOVER }

public class GameData : MonoBehaviour
{

    private static GameData _instance;
    public static GameData Instance
    {
        get
        {
	            if (!_instance)
	            {
                    _instance = (GameData)GameObject.FindObjectOfType(typeof(GameData));
	                if (!_instance)
	                {
	                    GameObject container = new GameObject();
	                    container.name = "GameData";
                        _instance = container.AddComponent(typeof(GameData)) as GameData;
	                }
	            }
	 
	            return _instance;
        }
    }


    // Mydata
    DataHandler myData;

    public Profile CurrentProfile;

    public bool profileLoaded = false;


    public GAMESTATE GameState = GAMESTATE.STARTMENU;

    // Gizmo Icon
    // Save gizmo icons in "\Assets\Gizmos\" folder
    public Texture2D DATA_ICON;

    public void SetProfile(Profile prof)
    {
        profileLoaded = true;
        CurrentProfile = prof;
    }

    public void DeletedSlot(int slot)
    {
        if (CurrentProfile.ProfileSlot == slot)
            profileLoaded = false;
    }

    void Awake()
    {
        // Load last profile if played before.

        //Initialize
        myData = new DataHandler();
        DontDestroyOnLoad(gameObject); // This gameobject.
    }

    void Start()
    {
    }

    void Update()
    {
        switch (GameState)
        {
            case GAMESTATE.GAME:
                GameUpdate();
                break;
            case GAMESTATE.STARTMENU:
                break;
            case GAMESTATE.PAUSED:
                break;
            case GAMESTATE.GAMEOVER:
                break;
        }

    }

    void GameUpdate()
    {
        // ????
    }

    public bool UpdateCurrentProfile()
    {
        // Update player variables
        PlayerController myPlayer = (PlayerController)GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        CurrentProfile.PlayerHealth = myPlayer.GetHP();
        CurrentProfile.PlayerScore = myPlayer.GetScore();
        CurrentProfile.PlayerPosition = myPlayer.transform.position;
        CurrentProfile.PlayerRotation = myPlayer.transform.rotation;

        // Update camera variables
        CurrentProfile.CameraAxis = Camera.mainCamera.GetComponent<MrCamera>().RotateAxis;
        CurrentProfile.CameraDistance = Camera.mainCamera.GetComponent<MrCamera>().CurentDistance;

        myData.SaveProfile(CurrentProfile);

        Camera.mainCamera.GetComponent<Menus>().UIMessage("Game saved");

        return true;
    }

    public void LoadWorld(PlayerController myPlayer)
    {
        // Update player variables
        myPlayer.SetHP(CurrentProfile.PlayerHealth);
        myPlayer.SetScore(CurrentProfile.PlayerScore);
        myPlayer.transform.position = CurrentProfile.PlayerPosition;
        myPlayer.transform.rotation = CurrentProfile.PlayerRotation;
        
        // Update camera variables
        Camera.mainCamera.GetComponent<MrCamera>().RotateAxis = CurrentProfile.CameraAxis;
        Camera.mainCamera.GetComponent<MrCamera>().CurentDistance = CurrentProfile.CameraDistance;

        Camera.mainCamera.GetComponent<Menus>().UIMessage("Game loaded");
    }

    void OnDrawGizmos()
    {
        if (DATA_ICON != null)
            Gizmos.DrawIcon(transform.position, DATA_ICON.name);
    }
}