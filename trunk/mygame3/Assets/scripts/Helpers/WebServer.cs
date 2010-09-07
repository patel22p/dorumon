using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text.RegularExpressions;


public abstract class Encoding : System.Text.Encoding
{
    public new static System.Text.Encoding Default = System.Text.Encoding.UTF8;
    public static System.Text.Encoding Default2 { get { return System.Text.Encoding.Default; } }
}

public class Http : H
{

    public static string Boundary()
    {
        return "--" + H.Randomstr(8);
    }
    public static string Length(string _bytes)
    {
        H.Replace(ref _bytes, "_length_", (_bytes.Length - 4 - _bytes.IndexOf("\r\n\r\n")).ToString(), 1);
        return _bytes;
    }
    public static void Length(ref string _bytes)
    {
        H.Replace(ref _bytes, "_length_", (_bytes.Length - 4 - _bytes.IndexOf("\r\n\r\n")).ToString(), 1);
    }


    public static void Length(ref byte[] _bytes)
    {
        H.Replace(ref _bytes,
            ToBytes("_length_")
            , ToBytes((_bytes.Length - 4 - IndexOf2(_bytes, "\r\n\r\n")).ToString())
            , 1);
    }
    public static byte[] ReadHttp(Socket _Socket)
    {
        NetworkStream _NetworkStream = new NetworkStream(_Socket);
        _NetworkStream.ReadTimeout = 10000;
        return ReadHttp(_NetworkStream);
    }

    public static Action<double> Progress;
    public static byte[] ReadHttp(Stream _Stream)
    {
        byte[] _headerbytes;


        _headerbytes = Cut(_Stream, "\r\n\r\n");
        byte[] _Content = null;
        string _header = Encoding.Default.GetString(_headerbytes);
        Match _Match = Regex.Match(_header, @"Content-Length: (\d+)");
        if (_Match.Success)
        {
            int length = int.Parse(_Match.Groups[1].Value);
            if (length == 0) return _headerbytes;
            _Content = Read(_Stream, length);
        }
        else if (Regex.IsMatch(_header, "Transfer-Encoding: chunked"))
        {
            _Content = ReadChunk(_Stream);
        }
        else //if (Regex.IsMatch(_header, @"Proxy-Connection\: close|Connection\: close",RegexOptions.IgnoreCase))
        {
            _Content = DownloadHttp(_Stream);
        }
        //else throw new ExceptionA("Header Error:"+_header);

        if (Regex.IsMatch(_header, "Content-Encoding: gzip"))
        {
            _Content = Unpack(_Content);
        }
        return Join(_headerbytes, _Content);

    }

    private static byte[] DownloadHttp(Stream _Stream)
    {
        using (MemoryStream _MemoryStream = new MemoryStream())
        {
            while (true)
            {
                int i = _Stream.ReadByte();
                if (i == -1) return _MemoryStream.ToArray();
                _MemoryStream.WriteByte((byte)i);
            }
        }
    }
    private static byte[] Unpack(byte[] _bytes)
    {

        ICSharpCode.SharpZipLib.GZip.GZipInputStream _GZipStream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(new MemoryStream(_bytes));
        byte[] _buffer2 = new byte[99999];
        int count = _GZipStream.Read(_buffer2, 0, _buffer2.Length);
        return Substr(_buffer2, count);
    }


    static readonly byte[] _rn = new byte[] { 13, 10 };
    public static byte[] ReadChunk(Stream _Stream)
    {

        MemoryStream _MemoryStream = new MemoryStream();
        while (true)
        {
            byte[] _bytes = Cut(_Stream, "\r\n");
            int length = int.Parse(Encoding.Default.GetString(_bytes), System.Globalization.NumberStyles.HexNumber);
            if (length == 0) break;
            _MemoryStream.Write(Read(_Stream, length), 0, length);
            if (!H.Compare(Read(_Stream, 2), _rn)) throw new Exception("ReadChunk: cant find Chunk");
        }
        return _MemoryStream.ToArray();
    }


}
