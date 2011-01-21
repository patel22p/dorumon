using System;
using System.Xml.Serialization;
[Serializable]
public class MapSetting
{
    public static XmlSerializer xml = new XmlSerializer(typeof(MapSetting));
    public float damageFactor = 1;
    public bool freezeOnBite;
    public float PointsPerZombie = 1;
    public int StartMoney = 1;
    public float ZombieDamage = 5;
    public float zombieLifeFactor = 1;
    public float zombieSpeedFactor = 1;
    public bool kickIfErrors;
    public bool kickIfAfk;
    public int MaxPing;
    public GameMode[] supportedModes;
    public string mapName = "none";
    public string title = "none";
    public GameMode gameMode;
    public int fragZLimit = 20;
    public string[] ipaddress;
    public int port = 5300;
    public bool host;
    public int maxPlayers = 4;
    public float timeLimit = 15;
    public int stage;
    public bool TeamZombiSurvive { get { return gameMode == GameMode.DotA; } }
    public bool TDM { get { return gameMode == GameMode.TeamDeathMatch; } }
    public bool DM { get { return gameMode == GameMode.DeathMatch; } }
    public bool ZombiSurvive { get { return gameMode == GameMode.ZombieSurive; } }
    public bool Team { get { return TeamZombiSurvive || TDM; } }
    public bool zombi { get { return ZombiSurvive || TeamZombiSurvive; } }
}