using UnityEngine;
using System.Collections;

/// <summary>
/// This script makes sure that only the right gameobject with scripts is active.
/// If the scripts are disabled they still receive callbacks, this causes problems.
/// E.g. the disabled mainmenu would receive OnJoinedRoom while having just switched to the game scene.
/// Disabling the gameobjects prevents this problem.
/// </summary>
public class EnableScript : MonoBehaviour {

    public GameObject game;
    public GameObject mainMenu;
    
    /// <summary>
    /// Enable one GO and remove the other
    /// </summary>
	void Awake () {
        if (PhotonNetwork.room != null)
        {
            game.active = true;
            Destroy(mainMenu);
        }
        else
        {
            Destroy(game);
            mainMenu.active = true;                     
        }
        Destroy(gameObject);//We dont need this anymore
	}
	
	
}
