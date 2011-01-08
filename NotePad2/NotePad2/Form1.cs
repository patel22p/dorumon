using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotePad2
{
    public partial class Form1 : Form
    {
        string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
        public Form1()
        {
            InitializeComponent();
            Directory.SetCurrentDirectory(dir);
            Load += new EventHandler(Form1_Load);
            Closing += new CancelEventHandler(Form1_Closing);
            textBox1.KeyDown += new KeyEventHandler(textBox1_KeyDown);
            textBox1.KeyUp += new KeyEventHandler(textBox1_KeyUp);
        }
        void Form1_Load(object sender, EventArgs e)
        {
            Timer t = new Timer();
            t.Interval = 2000;
            t.Enabled = true;
            t.Tick += new EventHandler(t_Tick);
            menuItem1.Click += delegate { SoftKey(-1); };
            menuItem2.Click += delegate { SoftKey(+1); };
            LoadText();
            t2.Tick += delegate { listBox1.Visible = false; t2.Enabled = false; };
            t2.Interval = 2000;
        }

        void SoftKey(int step)
        {
            CopyText(); 
            SaveText(); 
            id+=step; 
            LoadText(); 
            PasteText();
        }

        void Form1_Closing(object sender, CancelEventArgs e)
        {
            SaveText();
        }

        
        Timer t2 = new Timer();
        int id = 100;
        public string gn(int id)
        {
            return dir + "/text" + id + ".txt";
        }
        private void LoadText()
        {
            
            this.Text = id.ToString();
            textBox1.Text = "";
            if (File.Exists(gn(id)))
                using (var f = File.OpenText(gn(id)))
                    textBox1.Text = f.ReadToEnd();

            listBox1.Visible = true;
            listBox1.Items.Clear();
            t2.Enabled = false;
            t2.Enabled = true;
            for (int i = id-3; i < id+4; i++)
            {
                if (File.Exists(gn(i)))
                    using (var f = File.OpenText(gn(i)))
                    {
                        var si = f.ReadLine()??"";
                        listBox1.Items.Add(si);                      
                    }                
                else
                    listBox1.Items.Add("");                      
                
            }
            listBox1.SelectedIndex = 3;
        }
        

        void t_Tick(object sender, EventArgs e)
        {
            SaveText();
        }

        private void SaveText()
        {            
            using (var f = File.CreateText(gn(id)))
                f.Write(textBox1.Text);
        }
        void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            PasteText();
        }
        string txt;
        void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (Arrows(e))
            {
                CopyText();
            }
        }
        private void PasteText()
        {
            var t = textBox1;
            if (txt != null)
            {
                int old = t.SelectionStart;
                t.Text = t.Text.Insert(t.SelectionStart, txt);
                t.SelectionStart = old;
                t.SelectionLength = txt.Trim().Length;
            }
            txt = null;
        }
        private void CopyText()
        {
            if (textBox1.SelectionLength != 0)
            {
                var t = textBox1;
                var p = Next(t.SelectionStart, -1);
                var n = Next(t.SelectionStart + t.SelectionLength, 1) - p;
                txt = t.Text.Substring(p, n);
                t.Text = t.Text.Remove(p, n);
                t.SelectionStart = p;
            }
        }

        private int Next(int start, int step)
        {
            for (int i = start; ; i += step)
            {
                if (i < 0) return 0;
                if (i > textBox1.TextLength - 1) return textBox1.TextLength - 1;
                if (textBox1.Text[i] == '\n')
                {
                    return i + 1;
                }
            }
        }
        private static bool Arrows(KeyEventArgs e)
        {
            var b = e.KeyData == Keys.Down || e.KeyData == Keys.Up || e.KeyData == Keys.RMenu || e.KeyData == Keys.LMenu;
            return b;
        }
        
    }
}