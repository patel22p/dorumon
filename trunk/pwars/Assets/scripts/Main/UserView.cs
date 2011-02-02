
using System.Xml.Serialization;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using Random = System.Random;
[Serializable]
public struct ScoreView
{
    public int place;
    public int frags;
    public int deaths;
}
[Serializable]
public class UserView
{
    public static XmlSerializer xml = new XmlSerializer(typeof(UserView));
    public string nick = "";    
    public string AvatarUrl= "";
    public string Desctiption = "";
    public string FirstName = "";
    public string BallTextureUrl="";
    public int MaterialId;    
    public ScoreView[] scoreboard = new ScoreView[10];    
    [XmlIgnore]
    public bool guest = false;
} 