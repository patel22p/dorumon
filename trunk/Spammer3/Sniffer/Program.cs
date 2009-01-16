using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tamir.IPLib;
using Tamir.IPLib.Packets;
using System.Diagnostics;
using System.IO;
using doru;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Net.Sockets;
namespace Sniffer
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "Sniffer";
            Directory.SetCurrentDirectory(Path.GetFullPath("../../../"));
            foreach (string s in Directory.GetFiles("./", "*.html"))
                File.Delete(s);
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
            new Program();
        }
        public Program()
        {
            Trace.WriteLine("<<<<<<Program Started>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + DateTime.Now);
            Start();
        }

        private void Start()
        {
            PcapDeviceList _PcapDeviceList = SharpPcap.GetAllDevices();
            PcapDevice _PcapDevice = _PcapDeviceList[0];
            _PcapDevice.PcapOnPacketArrival += new SharpPcap.PacketArrivalEvent(_PcapDevice_PcapOnPacketArrival);
            _PcapDevice.PcapOpen(false);
            _PcapDevice.PcapSetFilter("tcp and ip");
            _PcapDevice.PcapCapture(-1);
        }
        List<int> ports = new List<int>();

        public class Client
        {
            public int Port;
            public string ip;
            public int i;
            public SortedList<int, byte[]> _SendStream = new SortedList<int, byte[]>();
            public SortedList<int, byte[]> _ReceiveStream = new SortedList<int, byte[]>();
        }
        List<Client> _List = new List<Client>();
        public enum Direction { Sended, Received }
        int i;
        SortedList<long, TCPPacket> _SortedList = new SortedList<long, TCPPacket>();

        void _PcapDevice_PcapOnPacketArrival(object sender, Packet packet)
        {
            if (!(packet is TCPPacket)) return;
            TCPPacket _TCPPacket = (TCPPacket)packet;

            //_SortedList.Add(_TCPPacket.SequenceNumber, _TCPPacket);
            byte[] _data = _TCPPacket.TCPData;

            if ((_data.Length == 0 || _data.Length == 6) && !_TCPPacket.Fin) return;

            Direction _Direction;
            int port;
            string ip;
            int[] remoteport = new[] { 80 };
            if (remoteport.Contains(_TCPPacket.DestinationPort))
            {
                _Direction = Direction.Sended;
                port = _TCPPacket.SourcePort;
                ip = _TCPPacket.DestinationAddress;
            }
            else if (remoteport.Contains(_TCPPacket.SourcePort))
            {
                _Direction = Direction.Received;
                port = _TCPPacket.DestinationPort;
                ip = _TCPPacket.SourceAddress;
            }
            else return;
            Client _Client = (from c in _List where c.Port == port select c).FirstOrDefault();
            if (_TCPPacket.Fin)
            {
                if (_Client != null)
                {
                    _List.Remove(_Client);
                }
                return;
            }
            if (_Client == null)
            {
                _Client = new Client
                {
                    ip = ip,
                    Port = port
                };
                i++;
                _Client.i = i;
                _List.Add(_Client);
            }

            if (_Direction == Direction.Sended)
            {
                Save(_TCPPacket, _Direction, _Client, _Client._SendStream);
            }
            else
            {
                Save(_TCPPacket, _Direction, _Client, _Client._ReceiveStream);
            }
        }

        private static void Save(TCPPacket _TCPPacket, Direction _Direction, Client _Client, SortedList<int, byte[]> _Stream)
        {
            if (!_Stream.ContainsKey(_TCPPacket.Id))
            {
                
                string s = "<<<<<<<<<" + _Direction + " " + _Client.ip + ":" + _Client.Port + " id:" + _Client.i + " bytes:" + _TCPPacket.Data.Length + ">>>>>>>>";
                Console.WriteLine(s); Trace.WriteLine(s);
                Trace.WriteLine(ASCIIEncoding.ASCII.GetString(_TCPPacket.Data));
                _Stream.Add(_TCPPacket.Id, _TCPPacket.Data);
                using (FileStream _FileStream = new FileStream(_Client.i + " " + _Direction + ".html", FileMode.Create, FileAccess.Write))
                {
                    using (MemoryStream _MemoryStream = new MemoryStream())
                    {

                        for (int i = _Stream.Keys.First(); _Stream.ContainsKey(i); i++)
                        {
                            _FileStream.Write(_Stream[i]);
                        }                        
                    }
                }
            }
        }


    }
}
