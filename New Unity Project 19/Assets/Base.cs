using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;


public class Base : MonoBehaviour
{
    public new static void print(object ob)
    {
        test.console += "\r\n" + ob;        
    }
    public static Test test;
    public static Vkontakte vkontakte;
}

