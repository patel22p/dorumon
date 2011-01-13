
using System.Xml.Serialization;
using UnityEngine;
//[System.Serializable]
public class UserView : MonoBehaviour
{
    [XmlIgnoreAttribute]
    public Texture2D texture;
    public string first_name = "";
    public string last_name = "";
    public string nickname = "";
    public string nick { get { return nickname == "" ? first_name + " " + last_name : nickname; } set { nickname = value; } }
    public string photo = "";
    public int vkId;
    public int totalZombieKills;
    public int totalZombieDeaths;
    public int totalKills;
    public int totalDeaths;
    public bool online;
    public bool installed;
} 