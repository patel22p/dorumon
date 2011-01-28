
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
    public string _nick;
    public string nick = "guest" + new Random().Next(10);    
    public string AvatarUrl;
    public Texture2D Avatar
    {
        get
        {
            return LoadTexture(0, ref AvatarUrl);
        }
    }
    public string Desctiption;
    public string FirstName;
    [Names("BallImage")]
    public string BallTextureUrl;
    public const string proxy = Menu.webserver + "image.php?url=";
    public Texture2D BallTexture
    {        
        get
        {
            return LoadTexture(1,ref BallTextureUrl);
        }
    }
    Texture2D[] txt = new Texture2D[2];    
    private Texture2D LoadTexture(int id,ref string url)
    {
        if (txt[id] == null && url != "" && url != null)
        {
            var nu = url;
            Debug.Log("GettIng texture:" + nu);
            var w = new WWW(nu);
            Base2._TimerA.AddMethod(() => w.isDone, delegate
            {
                txt[id] = w.texture;
            });
            url = "";
        }
        return txt[id];
    }
    public int MaterialId;    
    public ScoreView[] scoreboard = new ScoreView[10];
    public bool guest = true;
    
    
} 