using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using doru;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Notepad.Properties;


namespace Notepad
{

    
    public partial class Window1
    {
        
        public class Model : NotifyPropertyChanged
        {

            public string _Text { get { return _Settings._Text; } set { _Settings._Text = value; OnPropertyChanged("_Text"); } }
            public string _Path { get { return _Settings._FilePath; } set { _Settings._FilePath = value ;} }
            public DateTime _Date { get { return _Settings._Date; } set { _Settings._Date = value; } }
            public string _Title { get { return Get<string>("_Title"); } set { Set("_Title", value); } }
            Settings _Settings { get { return Settings.Default; } }
            public bool _Loaded;
            
            public void Load()
            {
                if (_Loaded == true) Trace.Fail("already loaded");
                _Loaded = true;
                Open(_Path);
            }
            public void Open(string _Path)
            {
                
                "loading File".Trace();
                try
                {
                    if (_Path == "") throw new ExceptionA("empty path");
                    _Date = new FileInfo(_Path).LastWriteTime;
                    _Text = File.ReadAllText(_Path);

                    "loaded".Trace();
                }
                catch (ExceptionA e) { e.Message.Trace(); } catch (IOException) { this._Path = _Path; }
                _Title = _Path + " " + _Text.Length / 1000;
                
            }            
            
            public void SaveAs(string _path)
            {
                _Settings._FilePath = _path;
                Save();
            }

            public void Closing()
            {
                
            }
            public void Closed()
            {                
                Save();                
                _Settings.Save();
                "closed".Trace();
            }

            public void Save()
            {                
                try
                {
                    "Saving".Trace();
                    if (_Path == "") throw new ExceptionA("file unsaved, path null");
                    FileInfo fi = new FileInfo(_Path);
                    if (fi.Length == _Text.Length) throw new ExceptionA("file unsaved, length same");
                    if (fi.LastWriteTime > _Date)
                    {
                        "writing deferences".Trace();
                        string c = File.ReadAllLines(_Path).Union(_Text.SplitString()).Join("\r\n");
                        File.WriteAllText(_Path, c);
                    }
                    else
                        File.WriteAllText(_Path, _Text);                    
                    "saved".Trace();
                    Open(_Path);
                }
                catch (Exception e) { e.Message.Trace(); }
            }
            

            public void Nw()
            {
                _Text = "";
                _Path = null;                
            }

            public void Update()
            {
                Save();
            }
        }
    }
}
