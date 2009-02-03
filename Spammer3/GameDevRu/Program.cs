using System;
using System.Collections.Generic;
using System.Linq;
using doru;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace GameDevRu
{
    class Program
    {
        static void Main(string[] args)
        {
            Spammer3.Setup();
            new Program();
        }
        public Program()
        {
            Spamm();
        }
        public Socket Connect()
        {
            return new TcpClient("www.gamedev.ru", 80).Client;
        }
        intA _intA = new intA("i.txt");
        public void Spamm()
        {
            Socket _Socket = Connect();
            for (; _intA.i>0 ; _intA.i--)
            {
                try
                {
                    _Socket.Send(Http.Length(String.Format(Res._Post, _intA.i, _message)));
                    Http.ReadHttp(_Socket).Save(_intA.i.ToString());
                }
                catch (IOException e) { e.Message.Trace("error:"); _Socket = Connect(); }//_intA.i++; }
            }
        }
        public static string _message { get { return File.ReadAllText("../message.txt", Encoding.Default);  } }
    }
}
