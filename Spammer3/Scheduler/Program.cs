using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;

using System.Threading;
using doru;

namespace Scheduler
{
    public class Program
    {

        public static Process _Process = Process.GetCurrentProcess();
        static void Main(string[] args)
        {            
            if ((from p in Process.GetProcesses() where p.ProcessName == _Process.ProcessName select p).Count() > 1)
            {
                System.Windows.Forms.MessageBox.Show("Already exists");
                return;
            }
            Directory.SetCurrentDirectory(Path.GetFullPath(Path.GetDirectoryName(_Process.MainModule.FileName) +"../../../"));
			Trace.AutoFlush = true;
			Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
			Trace.WriteLine("started" + DateTime.Now);
            new Program();
        }
        public enum TaskType { Unknown,Date, Interval,Idle,Popup }
        public class Task
        {
            public TaskType _TaskType;
            public string _Popup;
            public string _filename;
			public string _arguments;
            public string _Wait;
			public string _DateTime;
			[XmlIgnore]
			public DateTime _DateTime1
			{
				get { return DateTime.Parse(_DateTime); }				
			}            
            [XmlIgnore]
            public TimeSpan _TimeSpan
            {
                get
                {                    
                    return TimeSpan.Parse(_DateTime);
                }                
            }
        }
        public class Database
        {
            public List<Task> _Tasks = new List<Task>();

        }
        Database _Database;
        const string _db = "db.xml";
        FileInfo _FileInfo;
        public void LoadDb()
        {
            if (_FileInfo==null || _FileInfo.LastWriteTime != new FileInfo(_db).LastWriteTime)
            {
                _FileInfo = new FileInfo(_db);
                using (FileStream _FileStream = new FileStream(_db, FileMode.Open, FileAccess.Read))
                {
                    _Database = (Database)_XmlSerializer.Deserialize(_FileStream);
                }
            }
        }
        DateTime _DateTime = DateTime.Now;
        public Program()
        {
            Start();
        }
		
        private void Start()
        {
			
            LoadDb();
            while (true)
            {
                Thread.Sleep(1000);
                Update();
            }
        }

        private void Update()
        {
			uint _IdleTime = Win32.GetIdleTime();
            foreach (Task _Task in _Tasks)
            {
                switch (_Task._TaskType)
                {
                    case TaskType.Date:
						if (_DateTime < _Task._DateTime1 && _Task._DateTime1 < DateTime.Now)
                        {                            
                            StartTask(_Task);
							Trace.WriteLine(_Task._DateTime+ "," + DateTime.Now+","+_Task._filename);
                        }
                        break;
                    case TaskType.Interval:
                        if (_Timer.TimeElapsed(_Task._TimeSpan.TotalMinutes))
                        {                            
                            StartTask(_Task);
							Trace.WriteLine(_Task._TimeSpan.ToString() + "," + DateTime.Now + "," + _Task._filename);
                        }
                        break;
					case TaskType.Idle:						
						if (_IdleTime > _Task._TimeSpan.TotalMilliseconds)
						{
							goto case TaskType.Interval;
						}
						break;
                    default:
                        Debugger.Break();
                        break;
                }
            }
            _DateTime = DateTime.Now;
            _Timer.Update();
            LoadDb();
        }
        public void StartTask(Task _Task)
        {
            if (_Task._Popup != null)
            {
                new Thread(delegate() { System.Windows.Forms.MessageBox.Show(_Task._Popup); }).Start();
            }
            else if (_Task._filename != null)
            {
                ProcessStartInfo _ProcessStartInfo = new ProcessStartInfo(_Task._filename);
                _ProcessStartInfo.WorkingDirectory = Path.GetDirectoryName(_Task._filename);
                _ProcessStartInfo.Arguments = _Task._arguments;
                Process _Process = Process.Start(_ProcessStartInfo);
                if (_Task._Wait != null) new Timer(TimerStack, _Process, (int)TimeSpan.Parse(_Task._Wait).TotalMilliseconds, Timeout.Infinite);
            }
            else throw new Exception();
        }
        public void TimerStack(object o)
        {
            try
            {
                Process _Process = (Process)o;
                _Process.Kill();
            }catch{ }
        }
        public static Timer2 _Timer = new Timer2();
        public class Timer2
        {
            double time;
            double oldtime;
            double _TimeElapsed;
            DateTime _DateTime = DateTime.Now;
            public void Update()
            {
                time = (DateTime.Now - _DateTime).TotalMinutes;
                _TimeElapsed = time - oldtime;
                oldtime = time;
            }
            public double _oldTime { get { return time - _TimeElapsed; } } //->time == oldtime
            public bool TimeElapsed(double minutes)
            {
                if (_TimeElapsed > minutes) return true;
                if (time % minutes < _oldTime % minutes)
                    return true;
                else
                    return false;
            }
        }

        public List<Task> _Tasks { get { return _Database._Tasks; } }
        
        XmlSerializer _XmlSerializer = Helper.CreateSchema("Scheduller",typeof(Database));
    }
}
