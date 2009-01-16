using System;
using System.Collections.Generic;
using System.Text;
#if(!SILVERLIGHT)
using System.IO.Compression;
#endif
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;

namespace doru
{
    public class ExceptionB : Exception
    {
        public ExceptionB(string s) : base(s) { }
    }
    [XmlRoot("dictionary")]

    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {

            return null;

        }



        public void ReadXml(System.Xml.XmlReader reader)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();



            if (wasEmpty)

                return;



            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {

                reader.ReadStartElement("item");



                reader.ReadStartElement("key");

                TKey key = (TKey)keySerializer.Deserialize(reader);

                reader.ReadEndElement();



                reader.ReadStartElement("value");

                TValue value = (TValue)valueSerializer.Deserialize(reader);

                reader.ReadEndElement();



                this.Add(key, value);



                reader.ReadEndElement();

                reader.MoveToContent();

            }

            reader.ReadEndElement();

        }



        public void WriteXml(System.Xml.XmlWriter writer)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            foreach (TKey key in this.Keys)
            {

                writer.WriteStartElement("item");



                writer.WriteStartElement("key");

                keySerializer.Serialize(writer, key);

                writer.WriteEndElement();



                writer.WriteStartElement("value");

                TValue value = this[key];

                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();



                writer.WriteEndElement();

            }

        }

        #endregion

    }

    

    
    public class ExceptionA : Exception { public ExceptionA(string s) : base(s) { } public ExceptionA() { } };
    public partial class Helper
    {

        public static List<string> RemoveDuplicates(List<string> inputList)
        {
            Dictionary<string, int> uniqueStore = new Dictionary<string, int>();
            List<string> finalList = new List<string>();

            foreach (string currValue in inputList)
            {
                if (!uniqueStore.ContainsKey(currValue))
                {
                    uniqueStore.Add(currValue, 0);
                    finalList.Add(currValue);
                }
            }
            return finalList;
        }

        public static void Replace<T>(ref T[] _source, T[] _oldarray, T[] _newarray)
        {
            Replace(ref _source, _oldarray, _newarray, 0, -1);
        }
        public static void Replace(ref string _source, string _oldarray, string _newarray, int count)
        {
            byte[] _bytes = _source.ToBytes();
            Replace(ref _bytes, _oldarray.ToBytes(), _newarray.ToBytes(), 0, count);
            _source = _bytes.ToStr();
        }
        public static void Replace(ref byte[] _source, string _oldarray, string _newarray, int count)
        {
            Replace(ref _source, _oldarray.ToBytes(), _newarray.ToBytes(), 0, count);
        }
        public static void Replace<T>(ref T[] _source, T[] _oldarray, T[] _newarray, int count)
        {
            Replace(ref _source, _oldarray, _newarray, 0, count);
        }
        public static void Replace<T>(ref T[] _source, T[] _oldarray, T[] _newarray, int startpos, int count)
        {
            for (int c = 0; c < count || count == -1; c++)
            {
                startpos = _source.IndexOf2(_oldarray, startpos);
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
                    throw new ExceptionA("Count Didnt Match");
                }
            }
            return;
        }

        public static string Randomstr(int size)
        {
            byte[] _bytes = new byte[size];
            for (int i = 0; i < size; i++)
            {
                _bytes[i] = (byte)_Random.Next(64, 90);
            }

            string s = Encoding.Default.GetString(_bytes);
            return s;
        }

        public static string convertToString<T>(IEnumerable<T> array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in array)
                sb.Append(item + ",");
            return sb.ToString();
        }
        public static Random _Random = new Random();
        public static int putToNextFreePlace<T>(T item, IList<T> items)
        {
            int id = items.IndexOf(default(T));
            if (id == -1)
            {
                items.Add(item);
                return items.Count - 1;
            }
            else
            {
                items[id] = item;
                return id;
            }
        }
        public static bool Compare<T>(T[] source, T[] pattern)
        {
            return Compare(source, pattern, 0);
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

    }
    
    public static class Extensions
    {
        public static T2 TryGetValue<T,T2>(this Dictionary<T,T2> dict,T t)
        {
            T2 t2;
            dict.TryGetValue(t, out t2);
            return t2;
        }
        public static string[] Split(this string a, string b)
        {
            return a.Split(new string[] { b }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static void AddUnique<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) list.Add(item);
        }
        public static bool Equals2(this byte[] a,byte[] b )
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
        public static string TrimStart(this string s,string a)
        {
            if (s.StartsWith(a)) return s.Substring(a.Length, s.Length-a.Length);
            return s;
        }
        public static T Pop<T>(this List<T> list)
        {
            T t = list[0];
            list.Remove(t);
            list.Add(t);
            return t;
        }
        public static byte[] Hex2(this string s)
        {
            StringBuilder sb = new StringBuilder();
            MatchCollection ms = Regex.Matches(s, @"0x\d{4,4}   (.+?)   ");
            foreach (Match m in ms)
                sb.AppendLine(m.Groups[1].Value);
            return sb.ToString().Hex();

        }
        public static byte[] Hex(this string s)
        {
            MatchCollection ms = Regex.Matches(s, "[0-9a-fA-F]{2,2}");
            byte[] _bytes = new byte[ms.Count];
            for (int i = 0; i < ms.Count; i++)
            {
                _bytes[i] = byte.Parse(ms[i].Value, System.Globalization.NumberStyles.HexNumber);
            }
            return _bytes;
        }
        
        public static byte[] Read(this Stream _Stream)
        {
            return _Stream.Read((int)(_Stream.Length - _Stream.Position));
        }
        public static T Random<T>(this IList<T> list,T t2)
        {
            T t;
            while ((t = list[_Random.Next(list.Count - 1)]).Equals(t2)) ;
            return t;
        }
        public static T Random<T>(this IList<T> list)
        {
            return list[_Random.Next(list.Count - 1)];
        }
        public static string Random(this string[] _Tags)
        {
            return _Tags[_Random.Next(_Tags.Length)];
        }
        public static void Write(this Stream _Stream, byte[] _bytes)
        {
            if (_bytes.Length == 0) throw new Exception();
            _Stream.Write(_bytes, 0, _bytes.Length);
        }
        public static void Write(this Stream _Stream, string s)
        {
            byte[] _bytes = Encoding.Default.GetBytes(s);
            _Stream.Write(_bytes, 0, _bytes.Length);
        }

        public static string GetString(this System.Text.Encoding e, byte[] str)
        {
            return e.GetString(str, 0, str.Length);
        }
        public static string[][] Split(this string[] stringArray, char spec)
        {
            string[][] mString = new string[stringArray.Length][];
            for (int i = 0; i < stringArray.Length; i++)
            {
                mString[i] = stringArray[i].Trim(spec).Split(spec);
            }
            return mString;
        }
        public static string WriteLine(this Stream _Stream, string s)
        {
            _Stream.Write(s + "\r\n");
            return s;
        }
        public static string ReadLine(this Stream _Stream)
        {
            return _Stream.Cut("\n").ToStr().TrimEnd('\r', '\n');
        }
        public static string Replace(this string s, string a, string b)
        {
            Debugger.Break();
            return s.Replace(a, b);
        }
        public static string RandomString(this Random _Random, int length)
        {
            const string r = "1234567890qwertyuiopasdfghjklzxcvbnm";
            StringBuilder _StringBuilder = new StringBuilder();
            for (int i2 = 0; i2 < length; i2++)
            {
                _StringBuilder.Append(r[_Random.Next(0, r.Length)]);
            }
            return _StringBuilder.ToString(0, _StringBuilder.Length - 1);
        }

        public static byte[] NextBytes(this Random _Random, int l)
        {
            byte[] _bytes = new byte[l];
            _Random.NextBytes(_bytes);
            return _bytes;
        }
        public static void StartBackground(this Thread t)
        {
            t.IsBackground = true;
            t.Start();
        }

        public static void ReplaceOnce(this string text, string a, string b, out string text2)
        {
            int i = text.IndexOf(a);
            if (i == -1) throw new ExceptionA();
            text = text.Remove(i, a.Length);
            text2 = text.Insert(i, b);
        }
        public static T[] ReverseA<T>(this T[] a)
        {
            return a.ReverseA(a.Length);
        }
        public static T[] ReverseA<T>(this T[] a, int len)
        {
            T[] b = new T[len];
            for (int i = 0; i < len; i++)
            {
                b[a.Length - i - 1] = a[i];
            }
            return b;
        }

        public static string ToHex(this byte b)
        {
            return String.Format("0x{0:x}", b);
        }

        public static string ToHex(this byte[] _bytes)
        {
            StringBuilder sb = new StringBuilder();
            int x = 0;
            while (true)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; ; i++)
                    {
                        sb.Append(_bytes[x] > 15 ? String.Format("{0:X}", _bytes[x]) : String.Format("0{0:X}", _bytes[x]));
                        if (++x == _bytes.Length) return sb.ToString().Trim();
                        if (i == 8) break;
                        sb.Append(" ");
                    }
                    sb.Append("-");
                }
                sb.Append("\r\n");
            }
        }
        public static string ToStr(this byte[] _Bytes)
        {
            return Encoding.Default.GetString(_Bytes);
        }
        public static byte[] ToBytes(this string _String)
        {
            return Encoding.Default.GetBytes(_String);
        }
        public static UInt32 ReadUInt32(this Stream _Stream)
        {
            return BitConverter.ToUInt32(_Stream.Read(4).ReverseA(), 0);
        }
        public static UInt16 ReadUInt16(this Stream _Stream)
        {
            return BitConverter.ToUInt16(_Stream.Read(2).ReverseA(), 0);
        }
        public static void WriteUint32(this Stream _Stream, UInt32 i)
        {
            _Stream.Write(BitConverter.GetBytes(i).ReverseA());
        }
        public static void WriteUint16(this Stream _Stream, UInt16 i)
        {
            _Stream.Write(BitConverter.GetBytes(i).ReverseA(2), 0, 2);
        }
        public static byte ReadB(this Stream _Stream)
        {
            int i = _Stream.ReadByte();
            if (i == -1) throw new IOException();
            return (byte)i;
        }


        public static byte[] Read(this Stream _Stream, int length)
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

        public static string Crypter(this string s)
        {
            char[] _chars1 = new char[] { 'o', 'l', 'e', 's' };
            char[] _chars2 = new char[] { '0', 'I', '3', '5' };
            StringBuilder _StringBuilder = new StringBuilder(s);
            for (int i = 0; i < s.Length; i++)
            {
                if (_Random.Next(3) == 1)
                {
                    for (int j = 0; j < _chars1.Length; j++)
                    {
                        if (s[i] == _chars1[j])
                        {
                            _StringBuilder[i] = _chars2[j];
                            break;
                        }
                    }
                }
            }
            return _StringBuilder.ToString();
        }
        public static byte[] Join(this byte[] a, byte[] b)
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
        public static byte[] Cut(this byte[] source, int start, out byte[] _bytes2)
        {
            byte[] _bytes = new byte[source.Length - start];
            _bytes2 = new byte[start];
            for (int i = 0; i < start; i++)
            {
                _bytes2[i] = source[i];
            }
            for (int i = start; i < source.Length; i++)
            {
                _bytes[i - start] = source[i];
            }
            return _bytes;
        }
        public static byte[] Substr(this byte[] source, int length)
        {
            byte[] _bytes2 = new byte[length];
            for (int i = 0; i < length; i++)
            {
                _bytes2[i] = source[i];
            }
            return _bytes2;
        }
        public static string Substr(this string s, string a)
        {
            return s.Substring(0, s.IndexOf(a));
        }
        public static byte[] Cut(this byte[] source, int start)
        {
            byte[] _bytes = new byte[source.Length - start];
            for (int i = start; i < source.Length; i++)
            {
                _bytes[i - start] = source[i];
            }
            return _bytes;
        }
        public static bool Contains(this byte[] _bytes, string s)
        {
            return _bytes.ToStr().Contains(s);
        }
        private static Random _Random = new Random();





        public static int IndexOf2(this byte[] source, string pattern)
        {
            return IndexOf2(source, Encoding.Default.GetBytes(pattern));
        }
        public static int IndexOf2<T>(this T[] source, T[] pattern)
        {
            return IndexOf2(source, pattern, 0);
        }

        public static byte[] Cut(this Stream source, string pattern)
        {
            return Cut(source, Encoding.Default.GetBytes(pattern));
        }
        public static byte[] Cut(this Stream source, byte[] pattern)
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
        public static int IndexOf2<T>(this T[] source, T[] pattern, int startpos)
        {
            for (int i = startpos; i < source.Length; i++)
            {
                if (source.Length - i < pattern.Length) return -1;
                if (Helper.Compare(source, pattern, i)) return i;
            }
            return -1;
        }
    }
    public static class Win32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static void SetConsoleWindowVisibility(bool visible, string title)
        {
            IntPtr hWnd = FindWindow(null, title);
            if (hWnd != IntPtr.Zero)
            {
                if (!visible)
                    ShowWindow(hWnd, 0);
                else
                    ShowWindow(hWnd, 1);
            }
        }
        public static void HideConsoleBar(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                int style = GetWindowLong(hWnd, GWL_EXSTYLE);
                style &= ~WS_EX_APPWINDOW;
                SetWindowLong(hWnd, GWL_EXSTYLE, style);
            }
            else Debugger.Break();
        }

        public static void HideConsoleBar(string title)
        {
            IntPtr hWnd = FindWindow(null, title);
            HideConsoleBar(hWnd);
        }
        public static int WS_EX_APPWINDOW = 0x40000;
        public static int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateWaitableTimer(IntPtr
        lpTimerAttributes,
        bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll")]
        private static extern bool SetWaitableTimer(IntPtr hTimer, [In] ref long
        pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr
        lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        private static extern Int32 WaitForSingleObject(IntPtr handle, uint
        milliseconds);

        static IntPtr handle;
        public static void SetWaitForWakeUpTime(int secconds)
        {
            long duetime = -10000000 * secconds;
            handle = CreateWaitableTimer(IntPtr.Zero, true, "MyWaitabletimer");
            SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero,
            IntPtr.Zero, true);
            //duetime = -t;
            Console.WriteLine("{0:x}", duetime);
            handle = CreateWaitableTimer(IntPtr.Zero, true,
            "MyWaitabletimer");
            SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero,
            IntPtr.Zero, true);
            uint INFINITE = 0xFFFFFFFF;
            int ret = WaitForSingleObject(handle, INFINITE);
            //MessageBox.Show("Wake up call");
        }


        public struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        [DllImport("User32.dll")]
        public static extern bool LockWorkStation();

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();

        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }

        public static long GetTickCount()
        {
            return Environment.TickCount;
        }

        public static long GetLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            if (!GetLastInputInfo(ref lastInPut))
            {
                throw new Exception(GetLastError().ToString());
            }

            return lastInPut.dwTime;
        }
    }
#if (!SILVERLIGHT)
public class MemoryStreamA : MemoryStream
    {
        public SortedList<int, byte[]> _List = new SortedList<int, byte[]>();
        public int _i;
        public override int Read(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                int a = base.Read(buffer, offset, count);
                if (a > 0) return a;
                Thread.Sleep(20);
            }
        }
        public override int ReadByte()
        {
            int b;
            while (true)
            {
                b = base.ReadByte();
                if (b != -1) break;
                Thread.Sleep(20);
            }
            return b;
        }
        public void Write(byte[] buffer, int index)
        {
            _List.Add(index, buffer);
            if (_i == 0) _i = index - 1;
            if (index <= _i) throw new Exception("Cannot Write Index Error");
            while (true)
            {                
                if (_List.ContainsKey(_i + 1))
                {
                    _i++;
                    Write(_List[_i],0,_List[_i].Length);
                }
                else
                    break;
            }

        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            long oldpos = Position;
            Seek(0, SeekOrigin.End);
            base.Write(buffer, offset, count);
            Seek(oldpos, SeekOrigin.Begin);
        }
    }

    public class intA
    {

        public override string ToString()
        {
            return i.ToString();
        }
        string file;
        public intA(string s)
        {
            file = s;
        }
        int? _i;
        
        public int i
        {
            get
            {
                if (_i != null) return _i.Value;
                else
                {
                    if (File.Exists(file)) _i = int.Parse(File.ReadAllText(file));
                    else _i = 0;
                }
                return _i.Value;
            }
            set { _i = value; File.WriteAllText(file, _i.ToString()); }
        }
    }
    public class ListA : List<string>
    {
        string file;
        public ListA(string file)
        {
            this.file = file;
            if (File.Exists(file))
                foreach (string s2 in File.ReadAllLines(file))
                {
                    base.Add(s2);
                }
        }
        public new bool Add(string s)
        {
            if (!Contains(s))
            {
                base.Add(s);
                return true;
            }
            else return false;
        }
        public void Flush()
        {
            File.WriteAllLines(file, this.ToArray());
        }
    }
    public class ListB<T> : List<T>
    {
        public ListB()
        {
        }
        string file;
        public ListB(string file)
        {
            this.file = file;
            _XmlSerializer = new XmlSerializer(this.GetType(), new Type[] { typeof(T) });
            if (File.Exists(file))
                using (FileStream _FileStream = File.Open(file, FileMode.Open))
                {
                    List<T> list = (List<T>)_XmlSerializer.Deserialize(_FileStream);
                    foreach (T t in list)
                    {
                        Add(t);
                    }
                }
        }
        XmlSerializer _XmlSerializer;

        public void Flush()
        {
            lock ("flush")
            {
                using (FileStream _FileStream = new FileStream("users.xml", FileMode.Create, FileAccess.Write))
                    _XmlSerializer.Serialize(_FileStream, this);
            }
        }
    }
    public partial class Helper
    {
        public static void GenerateXsd(Type _type, Type[] _types, string filename)
        {
            XmlReflectionImporter _XmlReflectionImporter = new XmlReflectionImporter();
            XmlSchemas _XmlSchemas = new XmlSchemas();

            XmlSchemaExporter _XmlSchemaExporter = new XmlSchemaExporter(_XmlSchemas);
            foreach (Type _Type in _types)
                _XmlReflectionImporter.IncludeType(_Type);

            _XmlSchemaExporter.ExportTypeMapping(_XmlReflectionImporter.ImportTypeMapping(_type));

            using (FileStream _FileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                _XmlSchemas[0].Write(_FileStream);
        }
    }
    public class ClientWait
    {
        public int _Port;
        private List<TcpClient> _TcpClients = new List<TcpClient>();

        public void Start()
        {
            TcpListener _TcpListener = new TcpListener(IPAddress.Any, _Port);
            _TcpListener.Start();
            while (true)
            {
                TcpClient _TcpClient = _TcpListener.AcceptTcpClient();
                lock ("clientwait")
                    _TcpClients.Add(_TcpClient);
                Thread.Sleep(10);
            }
        }

        public List<TcpClient> GetClients()
        {
            lock ("clientwait")
            {
                List<TcpClient> _Return = _TcpClients;
                _TcpClients = new List<TcpClient>();
                return _Return;
            }
        }
    }
    public class Sender
    {
        public TcpClient _TcpClient;
        public Socket _Socket { get { return _TcpClient.Client; } }
        public void Send(byte[] _Buffer2)
        {
            if (_TcpClient.Connected)
                try
                {
                    byte[] _Buffer3 = new byte[_Buffer2.Length + 1];
                    _Buffer3[0] = (byte)_Buffer2.Length;
                    Buffer.BlockCopy(_Buffer2, 0, _Buffer3, 1, _Buffer2.Length);
                    if (_Buffer2.Length == 0) Debugger.Break();
                    _Socket.Send(_Buffer3);
                }
                catch (SocketException e) { Trace.WriteLine(e.Message); }
        }
    }
    public class Listener
    {
        public TcpClient _TcpClient;
        private List<byte[]> _Messages = new List<byte[]>();

        public List<byte[]> GetMessages()
        {
            lock ("Get")
            {
                List<byte[]> _Return = _Messages;
                _Messages = new List<byte[]>();
                return _Return;
            }
        }

        public bool _Connected
        {
            get { return _TcpClient.Connected; }
        }
        public void Start()
        {
            //_TcpClient.ReceiveBufferSize = 100;
            //_TcpClient.SendBufferSize = 100;            
            MemoryStream _MemoryStream = new MemoryStream();
            byte[] _Buffer = new byte[9999];
            long _position = 0;
            while (_TcpClient.Connected)
            {
                try
                {
                    int count = _TcpClient.Client.Receive(_Buffer);

                    _MemoryStream.Write(_Buffer, 0, count);
                    _MemoryStream.Seek(_position, SeekOrigin.Begin);
                    while (true)
                    {
                        int _length = _MemoryStream.ReadByte();
                        if (_length == -1 || _MemoryStream.Length <= _position + _length) break;
                        Byte[] _Buffer1 = new byte[_length];

                        _MemoryStream.Read(_Buffer1, 0, _length);
                        _position = _MemoryStream.Position;
                        lock ("Get")
                            _Messages.Add(_Buffer1);
                    }
                    _MemoryStream.Seek(0, SeekOrigin.End);
                }
                catch (SocketException) { }
                catch (IOException) { }
                Thread.Sleep(1);
            }
        }
    }
    public class TcpRedirect
    {
        public string _LocalhostReplace;
        public delegate Socket GetSocket();
        public event GetSocket _RemoteIpDelegate;
        public int _LocalPort;
        Thread _Thread;
        public void StartAsync()
        {
            _Thread = new Thread(Start);
            _Thread.IsBackground = true;
            _Thread.Start();
        }
        const string hosts = @"C:\WINDOWS\system32\drivers\etc\hosts";
        TcpListener _TcpListener;

        public void Start()
        {
            if (Directory.Exists("logs")) Directory.Delete("logs", true);
            if (_LocalhostReplace != null) File.WriteAllText(hosts, "127.0.0.1 " + _LocalhostReplace);
            Thread.Sleep(500);
            _TcpListener = new TcpListener(IPAddress.Any, _LocalPort);
            _TcpListener.Start();
            int i = 0;
            try
            {
                while (true)
                {
                    i++;
                    Socket _LocalSocket = _TcpListener.AcceptSocket();
                    File.WriteAllText(hosts, "");
                    Socket _RemoteSocket = _RemoteIpDelegate();
                    if (_LocalhostReplace != null) File.WriteAllText(hosts, "127.0.0.1 " + _LocalhostReplace);
                    Trace.Write("<<<<<Client Connected>>>>>>>>" + i + "\n");
                    Client _LocalListen = new Client() { _ListenSocket = _LocalSocket, _SendToSocket = _RemoteSocket, _name = i + "Sended" };
                    _LocalListen.StartAsync();
                    Client _RemoteListen = new Client() { _ListenSocket = _RemoteSocket, _SendToSocket = _LocalSocket, _name = i + "Received" };
                    _RemoteListen.StartAsync();
                    Thread.Sleep(2);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("TpcRedirect: " + e.Message);
            }
        }
        public void Stop()
        {
            _TcpListener.Stop();
            File.WriteAllText(hosts, "");
            Thread.Sleep(500);
        }
        public class Client
        {
            public Socket _ListenSocket;
            public Socket _SendToSocket;
            public string _name;
            public void StartAsync()
            {
                Thread _Thread = new Thread(Start);
                _Thread.IsBackground = true;
                _Thread.Start();
            }
            void Start()
            {
                Directory.CreateDirectory("logs");
                FileStream _FileStream = new FileStream("logs/" + _name + ".txt", FileMode.Create, FileAccess.Write);

                Byte[] data = new byte[99999];
                try
                {
                    while (true)
                    {
                        int count = _ListenSocket.Receive(data);
                        if (count == 0) break;
                        Trace.WriteLine(_name + "<<<<<<<<<<<<<<<<<<<<" + count + " Bytes>>>>>>>>>>>>>>>>>");
                        Trace.WriteLine(Encoding.Default.GetString(data, 0, count));
                        _FileStream.Write(data, 0, count);
                        _SendToSocket.Send(data, count, SocketFlags.None);
                    }
                }
                catch (SocketException e) { Trace.WriteLine(e.Message); }
                Trace.WriteLine(_name + " <<<<<<<<<<<<<<Disconnected>>>>>>>>>>>>");
                _ListenSocket.Close();
                _SendToSocket.Close();
                _FileStream.Close();
            }
        }
    }
    public static class Extensions2
    {
        static Extensions2()
        {
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
        }
        public static void Send(this Socket _Socket, string s)
        {
            _Socket.Send(Encoding.Default.GetBytes(s));
        }
        public static string ReceiveText(this Socket _Socket)
        {
            byte[] _buffer = new byte[99999];
            int count = _Socket.Receive(_buffer);
            if (count == 0) throw new ExceptionA();
            return Encoding.Default.GetString(_buffer.Substr(count));
        }
        public static byte[] Receive(this Socket _Socket)
        {

            byte[] _buffer = new byte[99999];
            int count = _Socket.Receive(_buffer);
            if (count == 0) throw new SocketException();
            return _buffer.Substr(count);

        }
        public static byte[] Receive(this Socket _Socket, int length)
        {
            byte[] _buffer = new byte[length];
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                while (true)
                {
                    int count = _Socket.Receive(_buffer, length, 0);
                    if (count == 0) throw new ExceptionA("Read Socket failed");
                    _MemoryStream.Write(_buffer, 0, count);
                    length -= count;
                    if (length == 0) return _MemoryStream.ToArray();
                }
            }
        }
        public static string Save(this string s)
        {
            Encoding.Default.GetBytes(s).Save();
            return s;
        }
        public static Random _Random = new Random();
        public static byte[] Save(this byte[] s)
        {
            string path = "./logs/" + DateTime.Now.ToString().Replace(":", "-") + _Random.RandomString(4) + ".html";
            File.WriteAllBytes(path, s);
            Trace.WriteLine(Path.GetFullPath(path));
            return s;
        }
    }
    public static class Proxy
    {
        public static Socket Socks5Connect(string _proxyAddress, int _proxyPort, string _DestAddress, int _DestPort)
        {
            TcpClient _TcpClient = new TcpClient(_proxyAddress, _proxyPort);
            Socket _Socket = _TcpClient.Client;

            _Socket.Send(new byte[] { 5, 1, 0 });
            byte[] _bytes = _Socket.Receive(2);
            Trace.WriteLine("<<<<<<<socks5 received1>>>>>>>>");
            if (_bytes[0] != 5 && _bytes[1] != 1) throw new ExceptionA();
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                _BinaryWriter.Write(new byte[] {
                    5, // version
                    1, // tcp stream
                    0, // reserved
                    3 //type - domainname 
                });
                _BinaryWriter.Write(_DestAddress);
                _BinaryWriter.Write(BitConverter.GetBytes((UInt16)_DestPort).ReverseA());
                _Socket.Send(_MemoryStream.ToArray());
            }
            Trace.WriteLine("<<<<<<<<socks5 received2>>>>>>>>");
            byte[] _response = _Socket.Receive(10);
            if (_response[1] != 0) throw new ExceptionA("socket Error: " + _response[1]);
            return _Socket;
        }
    }
    public static class Http
    {
    public static T Trace<T>(this T t)
        {
            object o = (object)t;
            if (o is byte[])
            {
                Encoding.Default.GetString((byte[])o);
                return t;
            }
            else
                if (o is bool)
                {
                    System.Diagnostics.Trace.Write(((bool)o) ? "1" : "0");
                    return t;
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine(t);
                    return t;
                }
        }
        public static void Length(ref string _bytes)
        {
            Helper.Replace(ref _bytes, "_length_", (_bytes.Length - 4 - _bytes.IndexOf("\r\n\r\n")).ToString(), 1);
        }
        public static void Length(ref byte[] _bytes)
        {
            Helper.Replace(ref _bytes, "_length_".ToBytes(), (_bytes.Length - 4 - _bytes.IndexOf2("\r\n\r\n")).ToString().ToBytes(), 1);
        }
        public static byte[] ReadHttp(Socket _Socket)
        {
            NetworkStream _NetworkStream = new NetworkStream(_Socket);
            _NetworkStream.ReadTimeout = 10000;
            return ReadHttp(_NetworkStream);
        }
        public static byte[] ReadHttp(Stream _Stream)
        {
            byte[] _headerbytes;


            _headerbytes = _Stream.Cut("\r\n\r\n");
            byte[] _Content = null;
            string _header = Encoding.Default.GetString(_headerbytes);
            Match _Match = Regex.Match(_header, @"Content-Length: (\d+)");
            if (_Match.Success)
            {
                int length = int.Parse(_Match.Groups[1].Value);
                if (length == 0) return _headerbytes;
                _Content = _Stream.Read(length);
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
            return _headerbytes.Join(_Content);

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
            GZipStream _GZipStream = new GZipStream(new MemoryStream(_bytes), CompressionMode.Decompress, false);
            byte[] _buffer2 = new byte[99999];
            int count = _GZipStream.Read(_buffer2, 0, _buffer2.Length);
            return _buffer2.Substr(count);
        }

        static readonly byte[] _rn = new byte[] { 13, 10 };
        private static byte[] ReadChunk(Stream _Stream)
        {

            MemoryStream _MemoryStream = new MemoryStream();
            while (true)
            {
                byte[] _bytes = _Stream.Cut("\r\n");
                int length = int.Parse(Encoding.Default.GetString(_bytes), System.Globalization.NumberStyles.HexNumber);
                if (length == 0) break;
                _MemoryStream.Write(_Stream.Read(length), 0, length);
                if (!Helper.Compare(_Stream.Read(2), _rn)) throw new ExceptionA("ReadChunk: cant find Chunk");
            }
            return _MemoryStream.ToArray();
        }
    }
    public static class Spammer3
    {

        public static string title;
        public static string _Title
        {
            set { if (title != value) title = Console.Title = value; }
        }

        public static Random _Random = new Random();
        public static string ReplaceRandoms(string text, string[] _RandomTags)
        {
            text = Regex.Replace(text, @"_randomtext(\d+)_", delegate(Match m)
            {
                StringBuilder _StringBuilder = new StringBuilder();
                for (int i2 = 0; i2 < int.Parse(m.Groups[1].Value); i2++)
                {
                    _StringBuilder.Append(_RandomTags[_Random.Next(_RandomTags.Length)] + " ");
                }
                return _StringBuilder.ToString(0, _StringBuilder.Length - 1);
            });

            foreach (Match m in Regex.Matches(text, @"_randomcode(\d+)_"))
            {
                text = text.Replace(m.Value, _Random.RandomString(int.Parse(m.Groups[1].Value)));
            }
            return text;
        }

        public static bool done;
        public static bool Beep = true;
        public static void Setup() { Setup("../../"); }
        public static bool _supsend;
        public static bool LogToConsole=true;
        public static void Setup(string s)
        {                              
            if (done == true) return;
            done = true;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            
            Process _Process = Process.GetCurrentProcess();
            if ((from p in Process.GetProcesses() where p.ProcessName == _Process.ProcessName select p).Count() > 1)
            {
                Console.Beep(100,100);
                _Process.Kill();
            }
            Directory.SetCurrentDirectory(s);
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
            if (Console.LargestWindowHeight != 0)
            {
                Console.Title = Assembly.GetEntryAssembly().GetName().Name;
                if(LogToConsole)
                    Trace.Listeners.Add(new TextWriterTraceListener(Console.OpenStandardOutput()));                                
            }
            Trace.AutoFlush = true;
            Trace.WriteLine("Programm Started " + DateTime.Now);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(e.ExceptionObject);
            if (Console.LargestWindowHeight != 0 && Beep)
                Console.Beep(100, 100);
        }
        
    }
#else
    public static class Debug
    {
        public static void WriteLine<T>(T o)
        {
            Console.WriteLine(o);
        }
        public static void Write<T>(T o)
        {
            Console.Write(o);
        }
    }
    public static class Encoding
    {
        public static System.Text.Encoding Default { get { return System.Text.Encoding.UTF8; } }
    }
#endif

    public static class STimer
    {
        public static void AddMethod(double _Time, Action _Action)
        {

            _Timer.AddMethod(_Time, _Action);
        }

        static Timer2 _Timer = new Timer2();
        public static double _TimeElapsed { get { return _Timer._TimeElapsed; } }
        public static void Update()
        {
            _Timer.Update();
        }
        public static double _SecodsElapsed { get { return _Timer._TimeElapsed / 1000; } }
        public static bool TimeElapsed(int _Milisecconds)
        {
            return _Timer.TimeElapsed(_Milisecconds);
        }

        public static double? GetFps()
        {
            return _Timer.GetFps();
        }
    }
    public class Timer2
    {
        DateTime _DateTime = DateTime.Now;
        double oldtime;

        int fpstimes;
        double totalfps;
        public double GetFps()
        {
            if (fpstimes > 0)
            {
                double fps = (totalfps / fpstimes);
                fpstimes = 0;
                totalfps = 0;
                if (fps == double.PositiveInfinity) return 0;
                return fps;
            }
            else return 0;
        }
        double time;
        public void Update()
        {
            while ((DateTime.Now - _DateTime).TotalMilliseconds - oldtime == 0) Thread.Sleep(1);

            time = (DateTime.Now - _DateTime).TotalMilliseconds;
            _TimeElapsed = time - oldtime;
            oldtime = time;
            fpstimes++;
            totalfps += 1000 / _TimeElapsed;

            UpdateActions();
        }

        private void UpdateActions()
        {
            for (int i = _List.Count - 1; i >= 0; i--)
            {
                CA _CA = _List[i];
                _CA._Time -= _TimeElapsed;
                if (_CA._Time < 0)
                {
                    _List.Remove(_CA);
                    _CA._Action();
                }
            }
        }

        public double _TimeElapsed = 0;
        public double _oldTime { get { return time - _TimeElapsed; } }
        public bool TimeElapsed(double _Milisecconds)
        {
            if (_TimeElapsed > _Milisecconds) return true;
            if (time % _Milisecconds < _oldTime % _Milisecconds)
                return true;
            else
                return false;
        }

        public void AddMethod(double time, Action _Action)
        {
            if (_List.FirstOrDefault(a => a._Action == _Action) == null)
                _List.Add(new CA { _Action = _Action, _Time = time });
        }

        List<CA> _List = new List<CA>();
        class CA
        {
            public double _Time;
            public Action _Action;
        }
    }




}