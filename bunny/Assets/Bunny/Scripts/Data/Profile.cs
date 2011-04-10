using UnityEngine;
using System.Collections;


public class Profile
{
    // Profiling variables
    public string ProfileName { get; set; }
    public int ProfileSlot { get; set; } // Mainly used in menus, since.. well, we dont want to read all the time shit.
    public bool FirstTime{ get; set; }

    // Player variables
    public Vector3 PlayerPosition { get; set; }
    public Quaternion PlayerRotation { get; set; }
    public int PlayerHealth { get; set; }
    public int PlayerScore { get; set; }

    // Game progress data
    public int CurrentLevel { get; set; }
    public ArrayList FinishedQuests { get; set; }
    public ArrayList CurrentWeapons { get; set; }

    // Camera data
    public Vector2 CameraAxis { get; set; }
    public float CameraDistance { get; set; }

    public Profile()
    {
        // Empty constructor
    }

    // New profile constructor
    public Profile(string profName, int slot)
    {
        // Possible used as a default starting profile?
        ProfileName = profName; // Desired profile name.
        CurrentLevel = 1;       // Starting level, yes?
        PlayerHealth = 10;      // ??:D
        ProfileSlot = slot;     // The game save slot... Shouldnt change or things will go haywire
        FirstTime = true;       // Only used once, cause all the positions and rotations arent stored yet. So we can use the Scenes default positions and orientations and so forth.
    }
}
