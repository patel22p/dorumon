using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml.Serialization;


public class z3Labby : Base
{

    

    void Start()
    {
        printC("start zlabby");
        if (!build) GameObject.Find("menu").active = false;
        userviews.Clear();
        printC(lc.zlabby);
        
        if(Network.isServer)
            RPCServerData(version.ToString(), (int)_hw.gameMode, (int)_hw.selectedlevel, _hw.fraglimit); 

        if (!skip) rpcwrite(lc.plj + z2Menu.Nick);

        if (dm || tdm)
        {

            localuser.totaldeaths += localuser.deaths;
            localuser.totalkills += localuser.frags;
        }
        else
        {
            localuser.totalzombiedeaths += localuser.deaths;
            localuser.totalzombiekills += localuser.frags;
        }


        localuser.nwid = Network.player;
        localuser.team = Team.Spectator;
        localuser.nick = Nick;
        RPCSetUserView(localuser.nwid, localuser.nick, localuser.uid, localuser.photo, localuser.totalkills, localuser.totaldeaths, localuser.totalzombiekills, localuser.totalzombiedeaths);
        userviews.Add(localuser.nwid.GetHashCode(), localuser);
        

    }

    [RPC]
    private void RPCServerData(string v, int gamemode, int level, int frags)
    {
        CallRPC(true, v, gamemode, level, frags);
        if (v != version.ToString())
        {
            printC(string.Format(lc.WrongVersion.ToString(), v, version.ToString()));
            //Network.Disconnect();
        }
        _hw.gameMode = (GameMode)gamemode;
        _hw.selectedlevel = (GameLevels)level;
        _hw.fraglimit = frags;

    }
    [RPC]
    private void RPCSetUserView(NetworkPlayer nwid, string nick, int uid, string photo, int tk, int td, int tzk, int tzd)
    {

        CallRPC(true, localuser.nwid, localuser.nick, localuser.uid, localuser.photo, tk, td, tzk, tzd);
        if (nwid == Network.player) return;

        z0Vk.user user = new z0Vk.user();
        user.nick = nick;
        user.uid = uid;
        user.photo = photo;
        user.nwid = nwid;
        user.totalkills = tk;
        user.totaldeaths = td;
        user.totalzombiekills = tzk;
        user.totalzombiedeaths = tzd;

        if (photo != "")
            new WWW2(photo).done += delegate(WWW2 www)
            {
                print("loaded texture");
                user.texture = www.www.texture;
                DontDestroyOnLoad(user.texture);
            };
        userviews.Add(nwid.GetHashCode(), user);

    }


}
