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

public class Login:Base
{

    void Start()
    {
        if(!build) GameObject.Find("text").active = false;
    }
}
