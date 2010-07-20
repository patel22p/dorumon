function OnGUI () {
	if (networkView.isMine)
	{
		if (GUI.Button (new Rect(20,Screen.height-60, 80, 19), "Grow Castle"))
		{
			transform.localScale.y += 1;
			transform.position.y = transform.localScale.y*0.66;
		}
	}
}