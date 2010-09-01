using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;


public class H
{

    public static byte[] Join(byte[] a, byte[] b)
    {
        byte[] c = new byte[a.Length + b.Length];
        int j = 0;
        for (int i = 0; i < a.Length; i++, j++)
        {
            c[j] = a[i];
        }
        for (int i = 0; i < b.Length; i++, j++)
        {
            c[j] = b[i];
        }
        return c;
    }

    public static bool Compare<T>(T[] source, T[] pattern)
    {
        return Compare(source, pattern, 0);
    }

    public static byte[] Read(Stream _Stream, int length)
    {
        if (length == 0) throw new Exception("length == 0");
        byte[] _buffer = new byte[length];
        using (MemoryStream _MemoryStream = new MemoryStream())
        {
            while (true)
            {
                int count = _Stream.Read(_buffer, 0, length);
                if (count == 0) throw new IOException("Read Stream failed");
                _MemoryStream.Write(_buffer, 0, count);
                length -= count;
                if (length == 0) return _MemoryStream.ToArray();
            }
        }
    }

    public static byte[] Substr(byte[] source, int length)
    {
        byte[] _bytes2 = new byte[length];
        for (int i = 0; i < length; i++)
        {
            _bytes2[i] = source[i];
        }
        return _bytes2;
    }

    public static string Randomstr(int size)
    {
        char[] chars = new char[size];
        string s = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
        for (int i = 0; i < size; i++)
        {
            chars[i] = s[_Random.Next(s.Length - 1)];
        }
        return new string(chars);
    }
    public static Random _Random = new Random();
    public static void Replace(ref string _source, string _oldarray, string _newarray, int count)
    {
        byte[] _bytes = ToBytes(_source);
        Replace(ref _bytes, ToBytes(_oldarray), ToBytes(_newarray), 0, count);
        _source = ToStr(_bytes);
    }
    public static bool Compare<T>(T[] source, T[] pattern, int startpos)
    {
        if (source.Length - startpos < pattern.Length) return false;
        for (int j = 0; j < pattern.Length; j++, startpos++)
        {
            if (startpos >= source.Length || !pattern[j].Equals(source[startpos])) return false;
        }
        return true;
    }

    public static int IndexOf2<T>(T[] source, T[] pattern, int startpos)
    {
        for (int i = startpos; i < source.Length; i++)
        {
            if (source.Length - i < pattern.Length) return -1;
            if (H.Compare(source, pattern, i)) return i;
        }
        return -1;
    }

    public static void Replace<T>(ref T[] _source, T[] _oldarray, T[] _newarray, int startpos, int count)
    {
        for (int c = 0; c < count || count == -1; c++)
        {
            startpos = IndexOf2(_source, _oldarray, startpos);
            if (startpos != -1)
            {
                int length = _source.Length - _oldarray.Length + _newarray.Length;
                T[] dest = new T[length];
                int i = 0;
                for (; i < startpos; i++)
                    dest[i] = _source[i];
                for (int j = 0; j < _newarray.Length; i++, j++)
                    dest[i] = _newarray[j];
                for (int j = startpos + _oldarray.Length; i < length; i++, j++)
                    dest[i] = _source[j];
                _source = dest;
            }
            else
            {
                if (count == -1)
                {
                    return;
                }
                throw new Exception("Count Didnt Match");
            }
        }
        return;
    }

    public static string ToStr(byte[] _Bytes)
    {
        return Encoding.Default.GetString(_Bytes);
    }

    public static byte[] ToBytes(string _String)
    {
        return Encoding.Default.GetBytes(_String);
    }
    public static int IndexOf2(byte[] source, string pattern)
    {
        return IndexOf2(source, Encoding.Default.GetBytes(pattern));
    }
    public static int IndexOf2<T>(T[] source, T[] pattern)
    {
        return IndexOf2(source, pattern, 0);
    }


    public static void Replace<T>(ref T[] _source, T[] _oldarray, T[] _newarray, int count)
    {
        Replace(ref _source, _oldarray, _newarray, 0, count);
    }

    public static byte[] Cut(Stream source, string pattern)
    {
        return Cut(source, Encoding.Default.GetBytes(pattern));
    }

    public static void Write(Stream s, string _str)
    {
        Write(s, ToBytes(_str));
    }

    public static string getMd5Hash(string input)
    {
        MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }

    public static string Replace(string s, params string[] ss)
    {
        for (int i = 0; i < ss.Length; i++)
        {
            if (!s.Contains(ss[i])) throw new Exception(ss[i] + " cannot be replaced");
            s = s.Replace(ss[i], ss[++i]);
        }
        return s;
    }

    public static void Write(Stream _Stream, byte[] _bytes)
    {
        if (_bytes.Length == 0) throw new Exception();
        _Stream.Write(_bytes, 0, _bytes.Length);
    }

    public static byte[] Cut(Stream source, byte[] pattern)
    {
        MemoryStream _MemoryStream = new MemoryStream();
        while (true)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                int b = source.ReadByte();
                if (b == -1) throw new IOException("Cut: unable to cut");
                _MemoryStream.WriteByte((byte)b);
                if (pattern[i] != b) break;
                if (i == pattern.Length - 1) return _MemoryStream.ToArray();
            }
        }
    }

}
