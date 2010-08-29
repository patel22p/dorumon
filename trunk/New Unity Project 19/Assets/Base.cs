using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;


public class Base : MonoBehaviour
{
    public new void print(object ob)
    {
        test.console += "\r\n";
    }
    public static Test test;
    public static Vkontakte vkontakte;
}

