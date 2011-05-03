using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
public enum MenuAction { wait, join, single }
public class MenuGui : bs {

	void Start () {
	
	}
    void OnGUI()
    {
        if (gui.Button("Wait For Player"))
            SendAction(MenuAction.wait);
        if (gui.Button("Join Game"))
            SendAction(MenuAction.join);
        if (gui.Button("Start Single Player"))
            SendAction(MenuAction.single);

    }
    private void SendAction( MenuAction a)
    {
        transform.parent.SendMessage("Action", a);
    }
	void Update () {
	
	}
}
