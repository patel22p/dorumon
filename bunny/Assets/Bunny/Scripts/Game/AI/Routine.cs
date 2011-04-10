using UnityEngine;
using System.Collections;

public enum ACTIONTYPE { WAIT, ANIMATIONACTION, MOVETO }

// Custom routines for NPC's
[System.Serializable]
public class Routine
{
    public ACTIONTYPE Action;

    // Wait routine
    public float WaitTime;

    // Animation routine
    public int AnimateLoops;
    public string AnimationName;

    // Move to point in routine
    public int MoveToPoint;
    public bool RandomizePoint; // If you want to randomize the point in the routine.
}