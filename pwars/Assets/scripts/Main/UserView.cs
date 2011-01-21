
using System.Xml.Serialization;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
[Serializable]
public class UserView
{
    public static XmlSerializer xml = new XmlSerializer(typeof(UserView));
    //public Texture2D texture
    //{
    //    get
    //    {
    //        if (t == null)
    //        {

    //            t = new Texture2D(256, 256);
    //            t.LoadImage(data);
    //        }
    //        return t;
    //    }
    //    set { data = value.EncodeToPNG(); }
    //}
    //Texture2D t;
    //byte[] data;
    public string first_name = "";
    public string last_name = "";
    public string nickname = "";
    public string nick { get { return nickname == "" ? first_name + " " + last_name : nickname; } set { nickname = value; } }
    public string photo = "";
    public int totalZombieKills;
    public int totalZombieDeaths;
    public int totalKills;
    public int totalDeaths;
} 