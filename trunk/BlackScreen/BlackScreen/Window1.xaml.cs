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
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;

namespace BlackScreen
{
    public partial class Window1 : Window
    {
        DispatcherTimer dt;
        public bool writeDnevnik;
        string dnevnikPath = Environment.CurrentDirectory + @"\Dnevnik.txt";
        DateTime lastCheckOut = DateTime.Now;
        Random rand = new Random();
        public Window1()
        {
            InitializeComponent();
            InitWin();
            InitIcon();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            inputTextBox.KeyDown += new KeyEventHandler(textBox1_KeyDown);
            ResetDt();
            {
                var dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromMilliseconds(500);
                dt.Tick += delegate { Update(); };
                dt.Start();
            }            
            this.Focus();
            LoadDnevnik();
        }
        void LoadDnevnik()
        {
            output.Text = File.ReadAllText(dnevnikPath);
        }
        private void Update()
        {
            var txt = TimeElapsed();
            if (!this.inputTextBox.IsVisible)
                Remember.Text = txt;
            Title = txt;
            if (visib) { this.Activate(); this.Focus(); this.ShowWindow(); }
        }
        private string TimeElapsed()
        {
            var txt = (DateTime.Now - lastCheckOut).ToString().Split('.')[0];
            return txt;
        }
        void ni_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.ShowWindow();
                dt.Stop();
            }
            else
                Process.Start("notepad", Environment.CurrentDirectory + @"\Remember.txt");
        }
        void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && writeDnevnik)
            {
                File.WriteAllText(dnevnikPath, TimeElapsed() + "\t\t"+DateTime.Now+"\t" + inputTextBox.Text + "\r\n" + File.ReadAllText(dnevnikPath));
                //HideWindow();                
                inputTextBox.Visibility = Visibility.Hidden;
                writeDnevnik = false;
                lastCheckOut = DateTime.Now;                
                e.Handled = true;
                LoadDnevnik();
            }
        }
        void ShowWindow()
        {
            visib = true;
            //this.ShowInTaskbar = false;
            Show();            
            //this.WindowState = WindowState.Normal;            
        }
        bool visib = true;
        private void HideWindow()
        {
            visib = false;
            inputTextBox.Visibility = Visibility.Hidden;
            writeDnevnik = false;
            //this.Visibility = Visibility.Hidden;
            Hide();
            //this.WindowState = WindowState.Minimized;            
        }
        private void InitWin()
        {
            Cursor = Mouse.OverrideCursor = Cursors.None;
            //Hide();
            WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Height = this.Width = 5000;
            this.Top = this.Left = 0;
            this.Background = new SolidColorBrush(Colors.Black);
            this.Topmost = true;
            this.ShowInTaskbar = false;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl)) return;

            if (e.Key == Key.Q)
            {
                string[] txt = File.ReadAllLines("Remember.txt").TakeWhile(a => a != "").ToArray();
                if (txt.Length > 0)
                {
                    var ss = txt[rand.Next(txt.Length - 1)].Split('\t');
                    Remember.Text = ss[0];
                }

                e.Handled = true;
                this.inputTextBox.Text = "";
                this.inputTextBox.Visibility = Visibility.Visible;
                writeDnevnik = true;
                this.inputTextBox.Focus();
                
            }
            if (e.Key == Key.E)
            {
                Process.Start("notepad", dnevnikPath);                
                HideWindow();
                e.Handled = true;
            }
            if (e.Key == Key.W)
            {
                e.Handled = true;
                HideWindow();
            }
            if (e.Key == Key.T)
            {
                e.Handled = true;
                
                if (output.Visibility == Visibility.Hidden)
                {
                    output.Visibility = Visibility.Visible;
                    output.Focus();
                }
                else
                    output.Visibility = Visibility.Hidden;
            }

            ResetDt();
            base.OnKeyDown(e);
        }
        private void ResetDt()
        {
            if (dt != null) dt.Stop();
            dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMinutes(20);
            dt.Tick += delegate { this.ShowWindow(); dt.Stop(); };
            dt.Start();
        }
        
        private void InitIcon()
        {
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("Main.ico");
            ni.MouseDown += new System.Windows.Forms.MouseEventHandler(ni_MouseDown);
            ni.Visible = true;
        }
    }
}
