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
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Threading;
using System.Reflection;
using ManagedWinapi;

namespace Notepad
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public static Window1 _Window1;
        Model _Model;
        public Window1()
        {
            Logging.Setup();
            
            _Window1 = this;
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            ShowInTaskbar = false;
            RegisterHotkey();

            _Model = new Model();
            this.DataContext = _Model;
            foreach (string s in Environment.GetCommandLineArgs())
            {
                if (File.Exists(s) && System.IO.Path.GetExtension(s) != ".exe")
                {
                    _Model.Open(s);
                }
            }
            if (!_Model._Loaded) _Model.Load();
            KeyDown += new KeyEventHandler(Window1_KeyDown);
            Closed += new EventHandler(Window1_Closed);
            Closing += new System.ComponentModel.CancelEventHandler(Window1_Closing);
            _RitchTextBox.Focus();
            _RitchTextBox.TextChanged += new TextChangedEventHandler(RitchTextBox_TextChanged);

            new DispatcherTimer().StartRepeatMethod(600, Update);
            this.Focus();            
        }

        private void RegisterHotkey()
        {
            Hotkey _Hotkey = new Hotkey();
            _Hotkey.WindowsKey = true;
            _Hotkey.KeyCode = System.Windows.Forms.Keys.C;
            _Hotkey.HotkeyPressed += new EventHandler(Hotkey_HotkeyPressed);
            _Hotkey.Enabled = true;
        }

        void Hotkey_HotkeyPressed(object sender, EventArgs e)
        {            
            Visibility = Visibility.Visible;            
            Focus();
        }

        public new void Focus()
        {
            _RitchTextBox.Focus();
            base.Focus();
        }
        void RitchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Model._Text = _RitchTextBox.Text;
        }
        public void Update()
        {            
            _Model.Update();
        }
        

        void Window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _Model.Closing();
        }

        void Window1_Closed(object sender, EventArgs e)
        {
            _Model.Closed();
        }

        void Window1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Visibility = Visibility.Collapsed;
            }
            if (e.Key == Key.F3)
                _RitchTextBox.FindNext();
            
        }

        private void Nw(object sender, ExecutedRoutedEventArgs e)
        {
            _Model.Nw();
        }

        private void Open(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog _OpenFileDialog =new OpenFileDialog();
            _OpenFileDialog.ShowDialog();           
            string file =_OpenFileDialog.FileName;
            if (file!="" && new FileInfo(file).Length < 1024 * 1000)
            {
                _Model.Open(file);
                _RitchTextBox.Text = _Model._Text;
            }            
        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            _Model.Save();
        }

        private void Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }                

        private void Find(object sender, ExecutedRoutedEventArgs e)
        {
            if (_FindWindow == null || !_FindWindow.IsLoaded)
            {
                _FindWindow = new FindWindow();
                _FindWindow.Show();
                _FindWindow._Textbox.Text = _RitchTextBox.SelectedText;
                if (_RitchTextBox.SelectionLength == 0) _FindWindow._Textbox.Text = Clipboard.GetText();
                _FindWindow._Textbox.SelectAll();
                _FindWindow._Textbox.KeyDown += new KeyEventHandler(_Textbox_KeyDown);
            }
        }
        
        void _Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _RitchTextBox.Find(_FindWindow._Textbox.Text.Trim('\r','\n'));
            }
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {                
                _FindWindow.Close();                
            }            
        }
        FindWindow _FindWindow;
        
    }
    
}
