using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
using doru;

public class GuiFpsCounter : Base
{
    public TimerA timer = new TimerA();

    protected override void Update()
    {
        timer.Update();
    }
}



namespace doru
{
    
    public class TimerA
    {
        public delegate void Action();
        int _Ticks = Environment.TickCount;
        int oldtime;

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
        int miliseconds;
        public void Update()
        {
            miliseconds = Environment.TickCount - _Ticks;
            _MilisecondsElapsed = miliseconds - oldtime;
            if (_MilisecondsElapsed > 0)
            {
                oldtime = miliseconds;
                fpstimes++;
                totalfps += 1000 / _MilisecondsElapsed;

                UpdateActions();
            }
        }

        private void UpdateActions()
        {
            for (int i = _List.Count - 1; i >= 0; i--)
            {
                CA _CA = _List[i];
                _CA._Miliseconds -= _MilisecondsElapsed;
                if (_CA._Miliseconds < 0)
                {
                    _List.Remove(_CA);
                    _CA._Action();
                }
            }
        }

        public int _MilisecondsElapsed = 0;
        public double _SecodsElapsed { get { return _MilisecondsElapsed / (double)1000; } }
        public int _oldTime { get { return miliseconds - _MilisecondsElapsed; } }

        public bool TimeElapsed(int _Milisecconds)
        {
            if (_MilisecondsElapsed > _Milisecconds) return true;
            if (miliseconds % _Milisecconds < _oldTime % _Milisecconds)
                return true;
            else
                return false;
        }
        public void AddMethod(int _Miliseconds, Action _Action)
        {
            CA ca = new CA();
            ca._Action = _Action;
            ca._Miliseconds = _Miliseconds;
            _List.Add(ca);
        }

        List<CA> _List = new List<CA>();
        class CA
        {
            public int _Miliseconds;
            public Action _Action;
        }
    }
}