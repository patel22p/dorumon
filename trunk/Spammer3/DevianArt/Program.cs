using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

using System.Diagnostics;
using doru;

namespace DevianArt
{
    public class Program
    {
        static void Main(string[] args)
        {
            Spammer3.Setup();
            new Program();            
        }
        
        public Program()
        {
            TcpClient _TcpClient = new TcpClient("forum.deviantart.com", 80);
            Socket _Socket = _TcpClient.Client;
            _Socket.Send(File.ReadAllBytes("1 Sended.html"));
            byte[] _bytes= Http.ReadHttp(new NetworkStream(_Socket)).Save();
            
        }
    }
}

