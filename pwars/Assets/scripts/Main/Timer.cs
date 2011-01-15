
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Ext
{
    

    public static IEnumerable<Transform> GetTransforms(this Transform ts)
    {
        yield return ts;
        foreach (Transform t in ts)
        {
            foreach (var t2 in GetTransforms(t))
                yield return t2;
        }
    }
    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> coll, int N)
    {
        return coll.Reverse().Take(N).Reverse();
    }

    public static T Random<T>(this IEnumerable<T> source)
    {
        return source.Skip(UnityEngine.Random.Range(0, source.Count())).FirstOrDefault();
    }
    public static T AddOrGet<T>(this GameObject g) where T : Component
    {
        var c = g.GetComponent<T>();
        if (c == null) return g.AddComponent<T>();
        else
            return c;
    }
    
    public static T Parse<T>(this string s)
    {
        return (T)Enum.Parse(typeof(T), s);
    }
    public static T GetComponentInParrent<T>(this Transform t) where T : Component
    {
        for (int i = 0; ; i++)
        {
            if (t == null || i > 4) return null;
            var c = t.GetComponent<T>();
            if (c != null) return c;
            t = t.parent;
        }
        
    }
    public static MonoBehaviour GetMonoBehaviorInParrent(this Transform t)
    {
        for (int i = 0; ; i++)
        {
            if (t == null || i > 2) return null;
            var c = t.GetComponent<MonoBehaviour>();
            if (c != null) return c;
            t = t.parent;
        }
    }
    //public static IEnumerable<T> ShuffleIterator<T>(
    //   this IEnumerable<T> source, Random rng)
    //{
    //    T[] buffer = source.ToArray();
    //    for (int n = 0; n < buffer.Length; n++)
    //    {
    //        int k = rng.Next(n, buffer.Length);
    //        yield return buffer[k];

    //        buffer[k] = buffer[n];
    //    }
    //}

}
public class FindAsset : Attribute
{
    public string name;
    public bool overide;
    public FindAsset() { }
    public FindAsset(string from)
    {
        name = from;
    }
} 
public class FindTransform : Attribute
{
    public string name;
    public FindTransform() { }
    public FindTransform(string enumName)
    {
        name = enumName;
    }
    public bool scene;
    public FindTransform(string enumName, bool FindInScene)
    {
        scene = FindInScene;
        name = enumName;
    }
}
public class GenerateEnums : Attribute
{
    public string name;
    public GenerateEnums(string enumName)
    {
        name = enumName;
    }
}
[Serializable]
public class MapSetting
{
    public List<GameMode> supportedModes = new List<GameMode>();
    public string mapName = "none";
    public string title = "none";
    public GameMode gameMode;
    public int fragLimit = 20;
    public string[] ipaddress;
    public int port = 5300;

    public bool host;
    public int maxPlayers = 4;
    public float timeLimit = 15;
    public bool TeamZombiSurvive { get { return gameMode == GameMode.TeamZombieSurvive; } }
    public bool TDM { get { return gameMode == GameMode.TeamDeathMatch; } }
    public bool DM { get { return gameMode == GameMode.DeathMatch; } }
    public bool ZombiSurvive { get { return gameMode == GameMode.ZombieSurive; } }
    public bool Team { get { return TeamZombiSurvive || TDM; } }
    public bool zombi { get { return ZombiSurvive || TeamZombiSurvive; } }
}

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
        public int miliseconds;
        public void Update()
        {
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
                
                try
                {
                    select._Action2();
                }
                catch (Exception e) { Debug.LogError("Timer:" + e.Message + "\r\n\r\n" + select.stacktrace + "\r\n\r\n"); }
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



