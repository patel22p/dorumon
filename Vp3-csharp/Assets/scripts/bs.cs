using UnityEngine;
using System.Collections;

public class bs : Base
{

    public bool isMine { get { return networkView.isMine; } }

    public int ID
    {
        get { return networkView.owner.ID; }
    }

    public static float gravConst = 30;
    public static Game _Game;
    public static Player _Player;
}
