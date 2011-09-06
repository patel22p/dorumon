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
using System.Windows.Shapes;
using System.Diagnostics;
using doru;
using System.Windows.Threading;
using System.IO;

namespace BlackScreen
{
    public partial class Window3 : Window
    {
        string remendpath = Environment.CurrentDirectory + @"\Remember.txt";
        TimerA timer = new TimerA();
        TimeSpan tmElapsed;
        string dnevnikPath = Environment.CurrentDirectory + @"\Dnevnik.txt";
        public Window3()
        {

            InitializeComponent();
            double offs = 0;
            this.Left = -offs;
            this.Top = -offs;
            this.Width = SystemParameters.PrimaryScreenWidth * 2+offs*2;
            this.Height = SystemParameters.PrimaryScreenHeight+offs*2;
            
            Background = new SolidColorBrush(Colors.Black);
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(100);
            dt.Tick += delegate { Update(); };
            dt.Start();
            LoadDnevnik();
        }

     
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl)) return;
            if (e.Key == Key.S)
            {
                var tbTasks = textBox2;
                tbTasks.Text = tm(tmElapsed) + "\t\t" + DateTime.Now + "\t\t" + (tbTasks.Text.StartsWith("\r\n") ? "" : "\r\n") + tbTasks.Text;
                tbTasks.Focus();
                tmElapsed = TimeSpan.FromMilliseconds(-1);
            }            
            e.Handled = true;
            if (e.Key == Key.W)
            {
                HideCanvas(!showCanvas);                
            }
            var stext = textBox2.SelectedText;
            if (e.Key == Key.F && stext != "")
            {
                var i = Find(stext);
                if (i == -1)
                {
                    textBox2.SelectionStart = 0;
                    textBox2.SelectionLength = stext.Length;
                    i = Find(stext);
                }
                textBox2.SelectionStart = i;
                textBox2.SelectionLength = stext.Length;                
            }
            base.OnKeyDown(e);
        }

        private void HideCanvas(bool show)
        {
            showCanvas = show;
            Canvas1.Visibility = showCanvas ? Visibility.Visible : Visibility.Hidden;
            Mouse.OverrideCursor = showCanvas ? Cursors.Arrow : Cursors.None;                
        }

        private int Find(string stext)
        {
            var i = textBox2.Text.IndexOf(stext, textBox2.SelectionStart + textBox2.SelectionLength, StringComparison.OrdinalIgnoreCase);
            return i;
        }
        bool showCanvas=true;

        bool visib;
        void LoadDnevnik()
        {
            var tbTasks = textBox2;
            var t = File.ReadAllText(dnevnikPath);
            tbTasks.Text = t.Substring(0, t.Length);
        }
        string wrd;
        Random r = new Random();
        void Update()
        {
            if (timer.TimeElapsed(30000) || wrd == null)
            {
                var lst = File.ReadAllLines(remendpath).TakeWhile(a => a != "").ToList();
                wrd = lst[r.Next(lst.Count - 1)].Replace("\t", "\t\t\t\t\t\t\t\t\t\t\t");
            }
            textBox3.Text = wrd;

            if (timer.TimeElapsed(5000) && visib)
                Save();

            var t = TimeSpan.FromMilliseconds(timer._MilisecondsElapsed);
            tmElapsed += t;
            
            textBox1.Text = DateTime.Now + "\t\tElapsed " + tm(tmElapsed);
            var s = textBox2.Text;
            if (s.IndexOf("\n") != -1)
                Title = tmElapsed.Minutes + ":" + tmElapsed.Seconds + " " + s.Substring(0, s.IndexOf("\n") - 1);
            timer.Update();
        }

        private void Save()
        {
            File.WriteAllText(dnevnikPath, textBox2.Text);
        }
        string tm(TimeSpan t)
        {
            var s = t.ToString();
            if (s.IndexOf(".") != -1)
                return s.Substring(0, s.IndexOf("."));
            else
                return s;
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Save();
            base.OnClosing(e);
        }
        protected override void OnActivated(EventArgs e)
        {
            visib = true;
            HideCanvas(true);
            Topmost = true;
            base.OnActivated(e);
        }
        protected override void OnDeactivated(EventArgs e)
        {
            visib = false;
            HideWindow();
            base.OnDeactivated(e);
        }
        private void HideWindow()
        {
            WindowState = WindowState.Minimized;
            Topmost = false;
        }
    }
}
