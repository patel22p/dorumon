
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace doru
{
    //[Serializable]
    public class TimerA
    {
        public int _Ticks = Environment.TickCount;
        public int oldtime;
        public int fpstimes;
        public double totalfps;
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
        public Dictionary<int, float> timers = new Dictionary<int, float>();
        public int miliseconds;
        public void Update()
        {
            foreach (var a in timers.Keys)
                timers[a] += Time.deltaTime;

            miliseconds = Environment.TickCount - _Ticks;
            _MilisecondsElapsed = miliseconds - oldtime;
            if (_MilisecondsElapsed > 0)
            {
                oldtime = miliseconds;
                fpstimes++;
                totalfps += Time.timeScale / Time.deltaTime;
                UpdateAction2s();
            }
        }
        private void UpdateAction2s()
        {
            CA select = null;
            foreach (var _CA in _List)
            {
                _CA._Miliseconds -= _MilisecondsElapsed;
                if (_CA._Miliseconds < 0 && (_CA.func == null || _CA.func()) && select == null)
                {
                    select = _CA;
                }
            }
            if (select != null)
            {
                _List.Remove(select);                
                //try
                {
                    select._Action2();
                }
                //catch (Exception e) { Debug.LogError("Timer:" + e.Message + "\r\n\r\n" + select.stacktrace + "\r\n\r\n"); }
            }
        }
        public int _MilisecondsElapsed = 0;
        public double _SecodsElapsed { get { return _MilisecondsElapsed / (double)1000; } }
        public int _oldTime { get { return miliseconds - _MilisecondsElapsed; } }
        public bool TimeElapsed(object _this,object _id,float seconds)
        {
            int id = _this.GetHashCode() ^ _id.GetHashCode();            
            float elapsed;
            if (timers.TryGetValue(id, out elapsed))
            {
                if (elapsed > seconds)
                {
                    timers[id] = 0;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                timers.Add(id, 0);
                return false;
            }
        }
        public bool TimeElapsed(int _Milisecconds)
        {
            if (_MilisecondsElapsed > _Milisecconds) return true;
            if (miliseconds % _Milisecconds < _oldTime % _Milisecconds)
                return true;
            else
                return false;
        }
        public void AddMethod(Action _Action2)
        {
            AddMethod(-1, _Action2, null);
        }
        public void AddMethod(Func<bool> func, Action _Action2) { AddMethod(-1, _Action2, func); }
        public void AddMethod(int _Miliseconds, Action _Action2) { AddMethod(_Miliseconds, _Action2, null); }
        public void AddMethod(int _Miliseconds, Action _Action2, Func<bool> func)
        {
            CA ca = _List.FirstOrDefault(a => a._Action2 == _Action2);
            if (ca == null)
            {
                ca = new CA();
                _List.Add(ca);
            }

            ca.stacktrace = UnityEngine.StackTraceUtility.ExtractStackTrace();
            ca._Action2 = _Action2;
            ca._Miliseconds = _Miliseconds;
            ca.func = func;
        }
        public void Clear()
        {
            Debug.Log("Timer Clear");
            _List.Clear();
        }
        List<CA> _List = new List<CA>();
        class CA
        {
            public string stacktrace;
            public int _Miliseconds;
            public Func<bool> func;
            public Action _Action2;
        }

    }
}



