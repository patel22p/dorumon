using System;
using System.Xml.Serialization;
[Serializable]
public class MapSetting
{
    public override string ToString()
    {
        return mapName;        
    }
    public MapSetting Clone()
    {
        return (MapSetting)this.MemberwiseClone();
    }
    public int[] patrons = new int[] { 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static XmlSerializer xml = new XmlSerializer(typeof(MapSetting)); 
    public float damageFactor = 1;
    public bool slow = false;
    public float pointsPerZombie = 2;
    public float pointsPerPlayer = 1;
    public float pointsPerStage = 30;
    public int StartMoney = 1;
    public float zombieDamage = 10;
    public int zombiesPerStage = 2;
    public int zombiesAtStart = 10;
    public float zombieLifeFactor = 1;
    public float zombieSpeedFactor = 1;
    
    public bool haveALaser;
    public bool kickIfErrors;
    public bool kickIfAfk = true;
    public int maxPing = 150;
    public GameMode[] supportedModes = new GameMode[] { GameMode.DeathMatch, GameMode.ZombieSurvival, GameMode.TeamDeathMatch, GameMode.CustomZombieSurvival };
    public string mapName = "none";
    public string title = "none";
    public GameMode gameMode;
    public int fragLimit = 20;        
    public int maxPlayers = 2;
    public float timeLimit = 15;
    public int stage;
    public bool TeamZombiSurvival { get { return false; } }
    public bool TeamDeathMatch { get { return gameMode == GameMode.TeamDeathMatch; } }
    public bool DeathMatch { get { return gameMode == GameMode.DeathMatch; } }
    public bool DM { get { return gameMode == GameMode.DeathMatch || gameMode == GameMode.TeamDeathMatch; } }
    public bool ZombiSurvival { get { return gameMode == GameMode.ZombieSurvival; } }
    public bool CustomZombiSurvival { get { return gameMode == GameMode.CustomZombieSurvival; } }
    public bool Team { get { return TeamZombiSurvival || TeamDeathMatch; } }
    public bool zombi { get { return ZombiSurvival || TeamZombiSurvival || CustomZombiSurvival; } }
}