using doru;
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
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;

namespace BlackScreen
{

    public partial class Window2 : Window
    {
        TimerA timer = new TimerA();
        TimeSpan tmLeft;
        TimeSpan tmElapsed;
        string dnevnikPath = Environment.CurrentDirectory + @"\Dnevnik.txt";
        string remendpath = Environment.CurrentDirectory + @"\Remember.txt";
        public Window2()
        {
            InitializeComponent();
            InitWin();
            InitDt();
            LoadDnevnik();
            InitIcon();
        }
        string tm(TimeSpan t)
        {
            var s = t.ToString();
            if (s.IndexOf(".") != -1)
                return s.Substring(0, s.IndexOf("."));
            else
                return s;
        }
        string[] wrd = new string[] { "", "" };
        List<string> lst = new List<string>();
        Random r = new Random();
        private void Update()
        {
            if (timer.TimeElapsed(30000))
            {
                lst = File.ReadAllLines(remendpath).TakeWhile(a => a != "").ToList();
                wrd = lst[r.Next(lst.Count - 1)].Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            }

            tbTimeLeft.Text = "Remind:" + wrd[0] + "\t" + DateTime.Now + "\t\tLeft " + tm(tmLeft) + "\t\tElapsed " + tm(tmElapsed) + " \tRemind:" + wrd[1];
            var t = TimeSpan.FromMilliseconds(timer._MilisecondsElapsed); 
            if (tmLeft != new TimeSpan())
                tmLeft -= t;                
            tmElapsed += t;
            if (tmLeft < new TimeSpan())
            {
                tmLeft = TimeSpan.FromMinutes(5);
                ShowWindow();
            }
            if (timer.TimeElapsed(5000) && visib)
                File.WriteAllText(dnevnikPath, tbTasks.Text);

            timer.Update();
        }
        void LoadDnevnik()
        {
            tbTasks.Text = File.ReadAllText(dnevnikPath);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl)) return;
            if (e.Key == Key.W)
                HideWindow();
            if (e.Key == Key.E)
                tmLeft += TimeSpan.FromMinutes(5);
            if (e.Key == Key.S)
            {
                tbTasks.Text = tm(tmElapsed) + "\t\t" + DateTime.Now + "\t\t" + (tbTasks.Text.StartsWith("\r\n") ? "" : "\r\n") + tbTasks.Text;
                tbTasks.Focus();
                tmElapsed = tmLeft = TimeSpan.FromMilliseconds(-1);
            }
            if (e.Key == Key.Q)
                tmLeft += TimeSpan.FromMinutes(-5);
            if (e.Key == Key.T)
                tbTasks.Visibility = tbTasks.IsVisible ? Visibility.Hidden : Visibility.Visible;
            e.Handled = true;
            base.OnKeyDown(e);
        }
        private void InitDt()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(100);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();
        }
        private void InitWin()
        {
            //Cursor = Mouse.OverrideCursor = Cursors.None;
            Hide();
            WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Height = this.Width = 5000;
            this.Top = this.Left = 0;
            this.Background = new SolidColorBrush(Colors.Black);
            this.Topmost = true;
            this.ShowInTaskbar = false;
        }
        private void InitIcon()
        {
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("Main.ico");
            ni.MouseDown += new System.Windows.Forms.MouseEventHandler(ni_MouseDown);
            ni.Visible = true;
        }
        void ni_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                this.ShowWindow();
            else
                Process.Start("notepad", remendpath);
        }
        bool visib;
        void ShowWindow()
        {
            visib = true;
            Show();
        }
        private void HideWindow()
        {
            visib = false;
            Hide();
        }
        void dt_Tick(object sender, EventArgs e)
        {
            Update();
        }
    }
}
