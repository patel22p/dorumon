using UnityEngine;
using System.Collections;
public class GuiScoreBoard : Base
{


    public Player localPlayer { get { return Find<Player>("Player(Clone)"); } }
    public string debug="";
    public string life { get { return "" + (localPlayer == null ? 0 : (localPlayer).Life); } }
    protected override void OnGUI()
    {

        if (Network.peerType != NetworkPeerType.Disconnected && Input.GetKey(KeyCode.Tab))
        {

            Vector2 v = new Vector2(500, 300) / 2;
            Rect r = new Rect((Screen.width / 2) - v.x, (Screen.height / 2) - v.y, v.x * 2, v.y * 2);
            GUI.Box(r, "ScoreBoard");
            GUILayout.BeginArea(r);
            {
                GUILayout.Space(20);
                foreach (Player a in FindObjectsOfType(typeof(Player)))
                    GUILayout.Label("isMine:" + a.networkView.isMine + "  Owner:" + a.OwnerID.ToString() + "     Nick:" + a.Nick + " Score:" + a.score +
                        "     Life:" + a.Life + "    Ping:" + Network.GetLastPing(a.OwnerID.Value) +
                        "   IPAddress:" + a.OwnerID.Value.ipAddress + "port:" + a.OwnerID.Value.port);
                GUILayout.Label("<<<<Debug>>>>");
                
                foreach (NetworkPlayer a in Network.connections)
                    GUILayout.Label(a.ipAddress + " " + a.port + " " + a);
                GUILayout.Label(debug);
            }
            GUILayout.EndArea();
        }
        
    }    

}
