private var host : String = "83.221.146.11";
private var pinger;
private var pingTime : int = 0;

function Awake() {
	// Choose correct ping class according to platform
	if (System.Environment.OSVersion.Platform == System.PlatformID.Unix) {
		pinger = GetComponent("PingCustom");
	} else {
		pinger = GetComponent("PingWin");
	}
}

function OnGUI() {
	host = GUILayout.TextField(host, GUILayout.Width(100));
	if (GUILayout.Button("Ping")) {
		pingTime = pinger.Ping(host, 1000);
		// Retry ping if a bogus value is returned
		if (pingTime < 0 || pingTime > 1500)
			pingTime = pinger.Ping(host, 1000);
	}
	if (pingTime == -1)
		GUILayout.Label("Ping NOT supported in current player");
	else
		GUILayout.Label(pingTime + " ms");
}