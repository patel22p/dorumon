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

public class Localize
{
    public static bool rus;
    public bool Rus { get { return rus; } set { rus = value; } }
    public LcString ipaddress = "Ipaddress:   port:5300";
    public LcString mustlogin = "You must login in first to play multiplayer";
    public LcString timelimit = "You can play in multiplayer only above specified time";
    public LcString firstname = "Enter username first";
    public LcString connectingto = "connecting to ";
    public LcString connect = "Connect";
    public LcString guest = "Guest ";
    public LcString physxwarsver = "Physx Wars Version ";
}
public class LcString
{
    
    public string russian = " ";
    public string english = " ";
    public static implicit operator string(LcString str)
    {
        return Localize.rus ? str.russian : str.english;
    }
    public static implicit operator LcString(string str)
    {
        LcString lc = new LcString();
        lc.english = str;
        return lc;
    }
}
