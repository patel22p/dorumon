
using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Threading;

namespace XamlParser
{
    public class Program : S
    {
        static readonly string path = @"C:\Users\igolevoc\Documents\New Unity Gui\WpfApplication2\";
        static readonly string output = @"C:\Users\igolevoc\Documents\PhysxWars\Assets\";
        //static readonly string output = @"C:\Users\igolevoc\Documents\IrcSample";
        [STAThread]
        static void Main(string[] args)
        {
            
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");            
            Start();
        }
        static string filename;
        
        static void Start()
        {
            new Program();
        }
        public Program() 
        {
            foreach (string file in Directory.GetFiles(path, "*.xaml"))
            {

                filename = Path.GetFileNameWithoutExtension(file);
                if (filename.Contains("Window"))
                {
                    
                    string xaml = File.ReadAllText(file);
                    object o = (Window)XamlReader.Parse(xaml);
                    if (o is Window)
                    {
                        Window w = (Window)o;
                        w.Show();
                        new Parser() { templ = template }.Start(w);
                    }
                }
            }
        }
        public class Parser
        {
            string ongui = "";
            string winfunc = "";
            string privfields = "";
            string prefFields = "";
            string pubfields = "";
            string start = "";
            public string templ;
            int fieldi;

            public void Start(Window w)
            {
                templ = templ.Replace("_name_", filename);
                double cx = w.Width / 2;
                double cy = w.Height / 2;

                
                foreach (Canvas c in ((Canvas)w.Content).Children)
                {                    
                    
                    string x1 = "";
                    string y1 = "";
                    if (c.HorizontalAlignment == System.Windows.HorizontalAlignment.Stretch || c.HorizontalAlignment == System.Windows.HorizontalAlignment.Center)
                        x1 = (-cx + c.GetX()) + "f + Screen.width/2";
                    if (c.VerticalAlignment == System.Windows.VerticalAlignment.Stretch || c.VerticalAlignment == System.Windows.VerticalAlignment.Center)
                        y1 = (-cy + c.GetY()) + "f + Screen.height/2";
                    if (c.HorizontalAlignment == System.Windows.HorizontalAlignment.Left)
                        x1 = c.GetX() + "f";
                    if (c.VerticalAlignment == System.Windows.VerticalAlignment.Top)
                        y1 = c.GetY() + "f";
                    if (c.HorizontalAlignment == System.Windows.HorizontalAlignment.Right)
                        x1 = (-w.Width + c.GetX()) + "f + Screen.width";
                    if (c.VerticalAlignment == System.Windows.VerticalAlignment.Bottom)
                        y1 = (-w.Height + c.GetY()) + "f + Screen.height";
                    
                    
                    fieldi++;
                    start += "\t\twndid" + fieldi + " = UnityEngine.Random.Range(0, 1000);\r\n";
                    WritePrivateField("int wndid" + fieldi + ";");
                    ongui += "\t\tGUI.Window(wndid" + fieldi + "," + "new Rect(" + x1 + "," + y1 + "," + Width(c) + "f," + Height(c) + "f)" + ", Wnd" + fieldi +",\"\""+ GetStyle(c)+ ");\r\n";
                    winfunc += "\tvoid Wnd" + fieldi + "(int id){\r\n";
                    winfunc += "\t\tif (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}\r\n";
                    winfunc += "\t\tfocusWindow = false;\r\n";
                    winfunc += "\t\tbool onMouseOver;\r\n";
                    Draw(c);
                    if (!w.Topmost)
                        WriteLine("if (GUI.Button(new Rect(" + Width(c) + "f - 25, 5, 20, 15), \"X\")) { enabled = false;onButtonClick();ActionAll(\"onClose\"); }");
                    //WriteLine(@"if(GUI.tooltip!="""") GUI.Label(new Rect(Input.mousePosition.x+10-rect.x,Screen.height -Input.mousePosition.y+10-rect.y, 100, 200), GUI.tooltip);");
                    winfunc += "\t}\r\n";                    

                }
                templ = templ.Replace("_fields_", prefFields+pubfields + privfields, "_funcs_", winfunc, "_start_", start, "_ongui_", ongui);
                File.WriteAllText(output + "scripts/GUI/" + filename + ".cs", templ, System.Text.Encoding.UTF8);
            }
            string GetStyle(FrameworkElement f)
            {
                
                int i = Panel.GetZIndex(f);
                //if (f.Tag != null)
                //{
                //    string prv = "GUIStyle style" + f.Tag + ";";
                //    if (!privfields.Contains(prv))
                //    {
                //        WritePrivateField(prv);
                //        start += "\t\tstyle" + f.Tag + " = GUI.skin.FindStyle(\"" + f.Tag + "\");\r\n";
                //    }
                //    return ", GUI.skin.FindStyle(\"" + f.Tag + "\")";
                //}
                //else 
                if (i != 0)
                    return ", GUI.skin.customStyles[" + i + "]";
                else
                    return "";


            }

            private void Draw(Canvas parent)
            {
                foreach (FrameworkElement c in parent.Children)
                {

                    bool hasname;
                    if (c.Name == "")
                    {
                        hasname = false;
                        c.Name = c.GetType().Name + ++fieldi;
                    }
                    else hasname = true;


                    if (hasname)
                    {
                        c.Name = char.ToUpper(c.Name[0]) + c.Name.Substring(1);
                        WritepublicField("bool focus" + c.Name+";");
                        WriteLine("if(focus"+c.Name+") { focus"+c.Name+" = false; GUI.FocusControl(\""+c.Name+"\");}");
                        WriteLine("GUI.SetNextControlName(\""+c.Name+"\");");
                    }
                    

                    if (c is TextBox)
                        if (hasname)
                            TextBox((TextBox)c);                            
                        else
                            Label((TextBox)c);
                    if (c is Canvas)
                        Canvas((Canvas)c,hasname);
                    if (c is Button)
                        Button((Button)c);
                    if (c is TabControl)
                        TabControl((TabControl)c);
                    if (c is Image)
                        Image((Image)c);
                    if (c is ListBox)
                        ListBox((ListBox)c);
                    if (c is StackPanel)
                        StackPanel((StackPanel)c);
                    if (c is Slider)
                        Slider((Slider)c);
                    if (c is System.Windows.Shapes.Path)
                        Line((System.Windows.Shapes.Path)c);
                    if (c is ProgressBar)
                        ProgressBar((ProgressBar)c);
                    if (c is CheckBox)
                        Button((CheckBox)c);
                    
                        
                }
            }

            
            float Height(FrameworkElement f)
            {
                if (f.Height + "" == "NaN")
                {
                    if (f.ActualHeight == 0) Debugger.Break();
                    return (float)f.ActualHeight;
                }
                return (float)f.Height;
            }
            float Width(FrameworkElement f)
            {
                if (f.Width+"" == "NaN")
                {
                    if (f.ActualWidth == 0) Debugger.Break();
                    return (float)f.ActualWidth;
                }
                return (float)f.Width;
            }
            private void Canvas(Canvas c,bool hasname)
            {
                if (hasname)
                {
                    WritepublicField("bool enabled" + c.Name + " = true;");
                    WriteLine("if(enabled" + c.Name + "){");
                }
                WriteLine("GUI.BeginGroup(new Rect(" + c.GetX() + "f, " + c.GetY() + "f, " + Width(c) + "f, " + Height(c)+ "f), \"\");");
                WriteLine("GUI.Box(new Rect(0, 0, " + Width(c) + "f, " + Height(c)+ "f), \"\");");
                Draw(c);
                WriteLine("GUI.EndGroup();");
                if(hasname)
                    WriteLine("}");
            }
            void Slider(Slider c)
            {
                WritePrefsField("Float", c.Name, c.Value + "f",c.ClipToBounds);
                //WritepublicField("float " + c.Name + " = " + c.Value+"f;");
                WriteLine(c.Name + " = GUI.HorizontalSlider(" + Rect(c) + ", " + c.Name + ", " + c.Minimum + "f, " + c.Maximum + "f);");
                WriteLine("GUI.Label(new Rect(" + (c.GetX() + Width(c)) + "f," + c.GetY() + "f,40,15),System.Math.Round(" + c.Name + ",1).ToString());");                
            }



            private void Button(System.Windows.Controls.Primitives.ButtonBase c)
            {
                bool toggle = c is CheckBox;
                //WritepublicField("Action on" + c.Name + ";");                
                WriteBoolField(c.Name, ((toggle && ((CheckBox)c).IsChecked.Value)? true : false), c.ClipToBounds);
                WriteLine("bool old" + c.Name + " = " + c.Name + ";");
                WriteLine(c.Name + " = GUI." + (toggle ? "Toggle" : "Button")+"(" + Rect(c) + (toggle ? ","+c.Name : "") + ", new GUIContent(" + GetContent(c.Content) + ",\"" + c.Tag + "\"));");
                WriteLine("if (" + c.Name + " != old" + c.Name + (toggle ? "" : " && " + c.Name) + " ) {Action" + (c.SnapsToDevicePixels ? "All" : "") + "(\"on" + c.Name + "\");onButtonClick(); }");                
                WriteLine("onMouseOver = "+Rect(c)+".Contains(Event.current.mousePosition);");
                WriteLine("if (oldMouseOver"+c.Name+" != onMouseOver && onMouseOver) onOver();");
                WriteLine("oldMouseOver"+c.Name+" = onMouseOver;");
                WritePrivateField("bool oldMouseOver"+c.Name+";");

            }
            private string GetContent(object o)
            {
                if (o is Image)
                    return GetImage((Image)o);
                else
                    return "\"" + o + "\"";
            }
            private void StackPanel(StackPanel c)
            {
                WritepublicField("Action onStackPanelDraw" + ++fieldi + ";");
                WriteLine("GUI.Box(" + Rect(c) + ", \"\");");
                WriteLine("GUILayout.BeginArea(" + Rect(c) + ");");
                WriteLine("if(onStackPanelDraw" + fieldi + " != null) onStackPanelDraw" + fieldi + "();");
                WriteLine("GUILayout.EndArea();");
            }

            private void ProgressBar(ProgressBar c)
            {
                WritepublicField("float " + c.Name + " = " + c.Value+";");
                WriteLine("GUI.HorizontalScrollbar(" + Rect(c) + ", 0, Mathf.Min(Mathf.Max(0, " + c.Name + "),"+c.Maximum+"), 0, " + c.Maximum + GetStyle(c) +");");
                WriteLine("GUI.Label(new Rect(" + (c.GetX() + Width(c)/4) + "f," + c.GetY() + "f,100,15),"+c.Name+"+\"/\"+" + c.Maximum + " );");                                
            }
            private void Line(System.Windows.Shapes.Path c)
            {
                WriteLine("GUI.Box(" + Rect(c) + ",\"\",GUI.skin.customStyles[4]);");
            }
            private void ListBox(ListBox c)
            {                                                
                pubfields += "\tpublic string[] " + c.Name + " = new string[] {";
                double h = 15;
                foreach (ListBoxItem lbi in c.Items)
                {
                    h = Height(lbi);
                    pubfields += "\"" + lbi.Content + "\",";
                }
                pubfields += "};\r\n";
                WritePrivateField("Vector2 s" + c.Name + ";");
                WritePrefsField("Int","i" + c.Name,c.SelectedIndex,c.ClipToBounds);
                //WritepublicField("int i" + c.Name + ";");
                string rect = "new Rect(0,0, " + (Width(c) - 20) + "f, " + c.Name + ".Length* "+h+"f)";
                WriteLine("GUI.Box(" + Rect(c) + ", \"\");");
                WriteLine("s"+c.Name + " = GUI.BeginScrollView(" + Rect(c) + ", s" + c.Name + ", " + rect + ");");
                WriteLine("int old" + c.Name + " = i" + c.Name + ";");
                WriteLine("i" + c.Name + " = GUI.SelectionGrid(" + rect + ", i" + c.Name + ", " + c.Name + ",1,GUI.skin.customStyles[0]);");
                WriteLine("if (i" + c.Name + " != old" + c.Name + ") Action(\"on" + c.Name + "\"," + c.Name + "[i" + c.Name + "]);");
                //WritepublicField("Action<string> on" + c.Name + ";");
                WriteLine("GUI.EndScrollView();");
            }

            
            private string eval(string s)
            {
                return "@\"" + s.Replace("\"", "\"\"") + "\"";
            }
            private void Label(TextBox c)
            {
                WriteLine("GUI.Label(" + Rect(c) + ", " + eval(c.Text) +GetStyle(c) + ");");
            }

            private void TextBox(TextBox c)
            {
                int o;
                bool b =  (int.TryParse(c.Text, out o));
                WritepublicField("bool isReadOnly" + c.Name + " = " + c.IsReadOnly.ToString().ToLower() + ";");
                WriteLine("if(isReadOnly" + c.Name + "){");
                WriteLine("GUI.Label(" + Rect(c) + ", " + c.Name+".ToString()" + GetStyle(c) + ");");
                WriteLine("} else");
                WriteLine(c.Name + " = " + (b ? "int.Parse(" : "") + 
                    "GUI.TextField(" + Rect(c) + ", " + c.Name + (b ? ".ToString()" : "") + (c.MaxLength == 0 ? "" : "," + c.MaxLength) + GetStyle(c) + (b? ")":"")+ ");");
                WritePrefsField((b ? "Int" : "String"), c.Name, (b ? c.Text : eval(c.Text)),c.ClipToBounds);
                //WritepublicField((b ? "int " : "string ") + c.Name + " = " + (b ? c.Text : eval(c.Text)) + ";");
            }

            string strstr(string s, string a)
            {
                int i = s.LastIndexOf(a);
                return s.Substring(i + a.Length, s.Length - i - 1);
            }
            private void Image(Image c)
            {
                WritePrivateField("Rect " + c.Name + ";");
                
                WriteStart(c.Name + " = " + Rect(c) + ";");
                WriteLine("GUI.DrawTexture(" + c.Name + "," + GetImage(c) + ", ScaleMode.ScaleToFit);");

            }
            string GetImage(Image a)
            {
                string path = new Uri(a.Source.ToString()).LocalPath;
                string localpath = "Skin/Images/" + Path.GetFileName(path);
                pubfields += "\t[LoadPath(\""+localpath +"\")]\r\n";
                WritepublicField("Texture2D Image" + a.Name + ";");
                File.Copy(path, output + localpath, true);
                return "Image"+a.Name;
            }
            private string Rect(FrameworkElement v)
            {
                return "new Rect(" + v.GetX() + "f, " + v.GetY() + "f, " + Width(v) + "f, " + Height(v) + "f)";
            }
            private void TabControl(TabControl c)
            {
                WritepublicField("int tab" + c.Name+";");
                WriteLine("GUI.BeginGroup(" + Rect(c) + ", \"\");");
                WriteLine("GUI.Box(new Rect(0, 0, " + Width(c) + "f, " + Height(c) + "f), \"\");");
                int tab = 0;
                if (c.Items[0] is TabItem)
                {
                    string strs = "";
                    foreach (TabItem i in c.Items)
                        strs += "\"" + i.Header + "\",";

                    WriteLine("GUILayout.BeginArea(new Rect(0f, 0, " + Width(c) + ", 18));");
                    WriteLine("tab"+c.Name+" = GUILayout.Toolbar(tab"+c.Name+", new string[] { " + strs + " }, GUI.skin.customStyles[1], GUILayout.ExpandWidth(false));");
                    WriteLine("GUILayout.EndArea();");

                    WriteLine("GUI.BeginGroup(new Rect(0, 18, " + Width(c) + ", " + (Height(c) - 18) + "), \"\");");
                    WriteLine("GUI.Box(new Rect(0, 0, " + Width(c) + ", " + (Height(c) - 18) + "), \"\");");
                    foreach (TabItem i in c.Items)
                    {
                        WriteLine("if(tab" + c.Name + "==" + tab++ + "){");
                        Draw((Canvas)i.Content);
                        WriteLine("}");
                    }
                    WriteLine("GUI.EndGroup();");
                }
                else
                {
                    foreach (Canvas i in c.Items)
                    {
                        WriteLine("if(tab"+c.Name+"==" + tab++ + "){");
                        Draw((Canvas)i);
                        WriteLine("}");
                    }
                }
                WriteLine("GUI.EndGroup();");
            }
            void WriteStart(string s)
            {
                start += "\t\t" + s + "\r\n";
            }
            enum FT { pub,pref  }

            void WriteBoolField(string name, bool value, bool save)
            {
                if (save)
                    pubfields += "\tpublic bool " + name + " { get { return PlayerPrefs.GetInt(\"" + name + "\", " + (value ? 1 : 0) + ") == 1; } set { PlayerPrefs.SetInt(\"" + name + "\", value?1:0); } }\r\n";
                else
                    pubfields += "\tpublic bool " + name + "=" + value.ToString().ToLower() + ";\r\n";
            }
            void WritePrefsField(string type,string name,object def,bool save)
            {
                if (save)
                    prefFields += "\tpublic "+type.ToLower() +" " + name + "{ get { return PlayerPrefs.Get"+type+"(\"" + name + "\", " + def + "); } set { PlayerPrefs.Set"+type+"(\""+name+"\", value); } }\r\n";                    
                else
                    pubfields += "\tpublic " + type.ToLower() + " " + name + " = " + def + ";\r\n";
                
            }
            void WritePrivateField(string s)
            {
                privfields += "\tprivate " + s + "\r\n";
            }

            void WritepublicField(string s)
            {
                pubfields += "\tpublic " + s + "\r\n";
            }
            void WriteLine(string s)
            {
                winfunc += "\t\t" + s + "\r\n";
            }

        }
    }
}
