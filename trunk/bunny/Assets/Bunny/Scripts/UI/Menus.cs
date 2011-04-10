using UnityEngine;
using System.Collections;

public enum SelectedMenu { MAINMENU, NEWGAME, CONTINUE, OPTIONS, EXITGAME }

public class Menus : MonoBehaviour {
    // DATA
    private DataHandler myData;

	// Window properties
	private Rect MainMenuWindow;
	private Rect NewGameWindow;
	private Rect ContinueWindow;
	private Rect OptionsWindow;
	private Rect ExitWindow;

    public Rect WindowOffset;

    // GUistyles for buttons
    public GUIStyle NewgameButton;
    public GUIStyle ContinueButton;
    public GUIStyle StartButton;
    public GUIStyle QuitButton;
    public GUIStyle OptionsButton;
    public GUIStyle LoadProfileButton;
    public GUIStyle ApplyButton;
    public GUIStyle CancelButton;
    public GUIStyle NewGameLayout;
    public GUIStyle ContinueProfileButton;
    public GUIStyle SmallFont;
    public GUIStyle ContinueWindowStyle;
    public GUIStyle BackToMainMenu;
    public GUIStyle OptionsWindowStyle;
    public GUIStyle OptionswindowButtonsStyle;
    public GUISkin DefaultSkin;
	
	// Menustate
	private SelectedMenu selMenu = SelectedMenu.MAINMENU;
	private int selGridInt = 0;
    private string[] selStrings;
	private Vector2 scrollPosition = Vector2.zero;
	
	// TEMPORARY CRAP
	private bool showMenu = false;

	// Options
	private bool showDropdown = false;
	private int lastResSelection;
	private int qualSlider;
	private float gammaSlider = 100.0f;
	private float SFXVolSlider = 100.0f;
	private float MusicVolSlider = 100.0f;
	private int curRes = 1;
	
    // New profile
    string newProfileName = "Profile Name";

	// Loadgame
    private Profile[] profileList;
	private Vector2 loadScroll = Vector2.zero;

    // Message variables
    private string message = "";
    private float messageTime = 0.0f;
    public float DisplayMessagesFor = 5.0f;
    
    void Awake()
    {
        if (!GameData.Instance)
        {
            Application.Quit();
        }

    }

	void Start()
	{
        if (GameObject.FindWithTag("Player"))
        {
            if (Screen.showCursor) Screen.showCursor = false; 	// Hide cursor in freeroam.
            if (!Screen.lockCursor) Screen.lockCursor = true; 	// Lock cursor in freeroam.
        }

        // Data proto
        myData = new DataHandler();
        myData.ValidateProfiles(); // Check that profile folder exists, if not create one. Also validate profiles so that they are not corrupt.	
		
		// Proto
		lastResSelection = selGridInt;
		qualSlider = (int)QualitySettings.currentLevel;
		
		// Works, however for prototyping i need to do stuff
		selStrings = new string[Screen.resolutions.Length];
		int i = 0;
		foreach(Resolution res in Screen.resolutions)
		{
			selStrings[i] = res.width + "x" + res.height + "@"+res.refreshRate;
			i++;
		}
	}
	
	void Update()
	{
        if (Input.GetKeyDown(KeyCode.Escape) && Application.loadedLevel!=0)
        {
            if (showMenu)
                selMenu = SelectedMenu.MAINMENU;
            showMenu = !showMenu;
        }
        else if (Application.loadedLevel==0)
        {
            showMenu = true;
        }
	}
	
	// GUI CRAP
	void OnGUI()
	{
        // Window sizes
        // Mainmenu
        MainMenuWindow = new Rect(Screen.width / 6 - WindowOffset.width, Screen.height / 3 - (WindowOffset.height * 2), 300 + (WindowOffset.x), 300 + WindowOffset.y);

        // New game
        NewGameWindow = new Rect(Screen.width / 2 - 256, Screen.height / 2 - 256, 512, 512);

        ContinueWindow = new Rect(Screen.width / 2 - (500/2), Screen.height / 2 - (500 / 2), 480 + (WindowOffset.x), 500+WindowOffset.y);

        // Options
        OptionsWindow = new Rect((Screen.width / 2) - (493 / 2), (Screen.height / 2) - (308 / 2), 493, 330);

        // Exit
        ExitWindow = new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 50, 200, 150);

        GUI.skin = DefaultSkin;

        // Display current error message or success
        if (messageTime > Time.time)
        {
            float addition = 0;
            if (message.Length > 25)
            {
                addition = (message.Length / 25) * 15;
            }
            GUI.Box(new Rect((Screen.width / 2) - 256, Screen.height / 6, 512, 64 + addition), "");
            GUI.Label(new Rect((Screen.width / 2) - 256, Screen.height / 6, 512, 64 + addition), message);
        }
				
		if(showMenu)
		{
			if(!Screen.showCursor)Screen.showCursor=true; 	// Hide cursor in freeroam.
			if(Screen.lockCursor)Screen.lockCursor=false; 	// Lock cursor in freeroam.
			switch(selMenu)
			{
				case SelectedMenu.MAINMENU:
				    MainMenuWindow = GUI.Window(0, MainMenuWindow, DoMainMenu, "");
				break;
				case SelectedMenu.NEWGAME:
                    NewGameWindow = GUI.Window(0, NewGameWindow, DoNewGame, "", GUIStyle.none);
				break;
				case SelectedMenu.CONTINUE:
					ContinueWindow = GUI.Window(0, ContinueWindow, DoContinue, "Load Profile", ContinueWindowStyle);
				break;
				case SelectedMenu.OPTIONS:
					OptionsWindow = GUI.Window(0, OptionsWindow, DoOptions, "", OptionsWindowStyle);
				break;
				case SelectedMenu.EXITGAME:
					ExitWindow = GUI.Window(0, ExitWindow, DoExit, "Are you sure?", GUIStyle.none);
				break;
			}
		}
		else
		{
			if(Screen.showCursor)Screen.showCursor=false; 	// Hide cursor in freeroam.
			if(!Screen.lockCursor)Screen.lockCursor=true; 	// Lock cursor in freeroam.
		}
	}
	
	void DoMainMenu(int wid)
	{
        if (GameData.Instance.profileLoaded)
        {
            GUI.Label(new Rect(15, 15, MainMenuWindow.width - 30, 40), "Welcome " + GameData.Instance.CurrentProfile.ProfileName);
            if (GUI.Button(new Rect(70 + WindowOffset.width, 30 + WindowOffset.height, 180, 40), "", StartButton))
            {
                if (Application.loadedLevel!=GameData.Instance.CurrentProfile.CurrentLevel)
                {
                    GameData.Instance.GameState = GAMESTATE.GAME;
                    Application.LoadLevel(GameData.Instance.CurrentProfile.CurrentLevel);
                    selMenu = SelectedMenu.MAINMENU;
                }
                else
                {
                    showMenu = !showMenu;
                }
            }
        }
        else
        {
            if (GUI.Button(new Rect(70 + WindowOffset.width, 30 + WindowOffset.height, 180, 40), "", NewgameButton))
            {
                newProfileName = "Profile Name";
                selMenu = SelectedMenu.NEWGAME;
            }
        }
        if (GUI.Button(new Rect(70 + WindowOffset.width, 80 + WindowOffset.height, 180, 40), "", ContinueButton))
        {
            // Count the profiles in the folder
            ProfileListing();
            selMenu = SelectedMenu.CONTINUE;
        }
        if (GUI.Button(new Rect(70 + WindowOffset.width, 130 + WindowOffset.height, 180, 40), "", OptionsButton))
			selMenu = SelectedMenu.OPTIONS;
        if (GUI.Button(new Rect(70 + WindowOffset.width, 180 + WindowOffset.height, 180, 40), "", QuitButton))
			selMenu = SelectedMenu.EXITGAME;
	}
	
    // New game window GUI method
	void DoNewGame(int wid)
	{
        newProfileName = GUI.TextField(new Rect(NewGameWindow.width / 2 - 128, NewGameWindow.height / 2 - 32, 256, 64), newProfileName,12, NewGameLayout);
        if (GUI.Button(new Rect(NewGameWindow.width / 2 + 15, NewGameWindow.height / 2 + 64, 75, 75), "", ApplyButton))
        {
            if (myData.ProfileExists(newProfileName))
            {
                // It already exists.
                UIMessage("Profile already exists");
            }
            else
            {
                // Create a new profile
                UIMessage("Profile " + newProfileName + " created.");
                myData.NewProfileXML(newProfileName);
                selMenu = SelectedMenu.MAINMENU;
            }
        }
        if (GUI.Button(new Rect(NewGameWindow.width / 2 - 90, NewGameWindow.height / 2 + 64, 75, 75), "", CancelButton))
        {
            selMenu = SelectedMenu.MAINMENU;
        }
	}
	
	void DoOptions(int wid)
	{
        GUIStyle optionsStyle = new GUIStyle(SmallFont);
        optionsStyle.normal.textColor = Color.white;
//		Debug.Log(OptionsWindow);

        GUI.Label(new Rect(OptionsWindow.width / 2 - 75, 15, 150, 40), "Options");
        GUI.Label(new Rect(25, 45, 125, 30), "Screen Resolution", optionsStyle);
		if(GUI.Button(new Rect(175, 48, 125,26), (string)selStrings[selGridInt]))
		{
			showDropdown=!showDropdown;
			lastResSelection = selGridInt;
		}
		if(showDropdown)
		{
			if(selGridInt!=lastResSelection) showDropdown = false;
			GUI.Box(new Rect(335, 40, 150, 200), "");
			scrollPosition = GUI.BeginScrollView(new Rect(335, 40, 150, 200), scrollPosition, new Rect(0, -5, 125, (26*selStrings.Length) + 10), false, true);
			selGridInt = GUI.SelectionGrid(new Rect(0, 0, 135, 26*selStrings.Length), selGridInt, selStrings, 1);
			GUI.EndScrollView();
		}
		
		GUI.Label(new Rect(25,110,100,30), "Quality", optionsStyle);
		qualSlider = (int)GUI.HorizontalSlider(new Rect(150, 119, OptionsWindow.width -  325, 12),qualSlider, 0, 6);

        GUI.Label(new Rect(25, 140, 100, 30), "Gamma", optionsStyle);
		gammaSlider = (int)GUI.HorizontalSlider(new Rect(150, 149, OptionsWindow.width -  325, 12),gammaSlider, 0.0f, 200.0f);

        GUI.Label(new Rect(25, 170, 100, 30), "SFX Volume", optionsStyle);
		SFXVolSlider = (int)GUI.HorizontalSlider(new Rect(150, 179, OptionsWindow.width -  325, 12),SFXVolSlider, 0.0f, 200.0f);

        GUI.Label(new Rect(25, 200, 100, 30), "Music Volume", optionsStyle);
		MusicVolSlider = (int)GUI.HorizontalSlider(new Rect(150, 209, OptionsWindow.width -  325, 12),MusicVolSlider, 0.0f, 200.0f);

        if (GUI.Button(new Rect((OptionsWindow.width / 2) - 100, OptionsWindow.height - 80, 70, 70), "", CancelButton))
			selMenu = SelectedMenu.MAINMENU;
        if (GUI.Button(new Rect((OptionsWindow.width / 2) + 25, OptionsWindow.height - 80, 70, 70), "", ApplyButton))
		{
			// Do crap
			selMenu = SelectedMenu.MAINMENU;
            UIMessage("Settings saved... NOT! :D");
		}
	}

	void DoExit(int wid)
	{
        GUI.Label(new Rect(0, 25, 200, 50), "Are you sure?");
		if(GUI.Button(new Rect(25, 75, 75, 75), "", ApplyButton))
			Application.Quit();
		if(GUI.Button(new Rect(115, 75, 75, 75), "", CancelButton))
			selMenu=SelectedMenu.MAINMENU;
	}
	
	void DoContinue(int wid)
	{
        loadScroll = GUI.BeginScrollView(new Rect(5, 15, ContinueWindow.width - 10, ContinueWindow.height - 120), loadScroll, new Rect(5, 15, ContinueWindow.width - 30, (profileList.Length * 120) + 5));
		int i = 0;
		foreach(Profile p in profileList)
		{
            if (GUI.Button(new Rect(10, (120 * i) + 20, 480, 120), "", ContinueProfileButton))
            {
                // Load the game.
                UIMessage("Profile " + p.ProfileName + " loaded");
                GameData.Instance.SetProfile(p);
                if (Application.loadedLevel != 0)
                {
                    GameData.Instance.GameState = GAMESTATE.STARTMENU;
                    Application.LoadLevel(0);
                }
                selMenu = SelectedMenu.MAINMENU;
            }
            if (GUI.Button(new Rect(425, (120 * i) + 55, 50, 50), "", CancelButton))
            {
                // Delete the profile.
                myData.DeleteProfile(p.ProfileSlot);
                UIMessage("Deleted profile " + p.ProfileName);
                selMenu = SelectedMenu.MAINMENU;
            }

            // The text inside the shitter profile thingy texture thing stuff
            GUI.Label(new Rect(40, (120 * i) + 40, 200, 40), "Profile name: " + p.ProfileName, SmallFont);
			i++;
		}
		GUI.EndScrollView();
		if(GUI.Button(new Rect((ContinueWindow.width / 2) - 60, ContinueWindow.height - 45, 120, 30), "", BackToMainMenu))
			selMenu=SelectedMenu.MAINMENU;
	}

    void ProfileListing()
    {
        profileList = myData.ReturnProfiles();
    }

    public void UIMessage(string msg)
    {
        message = msg;
        messageTime = DisplayMessagesFor + Time.time;
    }
}
