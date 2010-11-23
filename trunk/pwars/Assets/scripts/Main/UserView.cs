
using System.Xml.Serialization;
using UnityEngine;
//[System.Serializable]
public class UserView : MonoBehaviour
{
    [XmlIgnoreAttribute]
    public Texture2D texture;
    //public override int GetHashCode()
    //{
    //    return nwid.GetHashCode();
    //}

    //public override bool Equals(object obj)
    //{
    //    return ((user)obj).nwid == this.nwid;
    //}        
    public string first_name = "";
    public string last_name = "";
    public string nickname = "";
    public VK.status st = new VK.status();
    public string nick { get { return nickname == "" ? first_name + " " + last_name : nickname; } set { nickname = value; } }
    public string photo = "";
    public int vkId;
    public NetworkPlayer nwid;
    public int totalZombieKills;
    public int totalZombieDeaths;
    public int totalKills;
    public int totalDeaths;

    public int deaths;
    public bool online;
    public bool installed;
    public int frags;
    public int score;
    public int ping;
    public int fps;
    public Team team;
} 