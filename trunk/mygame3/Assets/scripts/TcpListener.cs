using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System;

public class TcpListener : Base{

	// Use this for initialization

    Socket m_Socket;
	void Start () {

        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, 5301);
        try
        {
            m_Socket.Bind(ipLocal);
            m_Socket.Listen(100);             
            
        }
        catch (Exception e) { printC(e); enabled = false; }
           

	}
    ~TcpListener()
    {
        Cleanup();      
    }

    void Cleanup()
    {
        if (m_Socket != null)
            m_Socket.Close();
        m_Socket = null;
    }
    //ArrayList m_Connections = new ArrayList();
    void OnApplicationQuit()
    {
        Cleanup();        
    }

	// Update is called once per frame
	void Update () {
        ArrayList listenList = new ArrayList();
        listenList.Add(m_Socket);
        Socket.Select(listenList, null, null, 1000);
        for (int i = 0; i < listenList.Count; i++)
        {
            Socket newSocket = ((Socket)listenList[i]).Accept();
            newSocket.Close();
            //m_Connections.Add(newSocket);
            printC("socket connected");

            if (_Level == Level.z4game && Network.connections.Length == 0)
            {                
                _Loader.RPCLoadLevel(Level.z3labby.ToString(),RPCMode.AllBuffered);
            }

        }

	}
}
