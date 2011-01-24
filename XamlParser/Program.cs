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
        string modPath = "../../data.txt";
        public Program()
        {
            
            IEnumerable<string> l = File.ReadAllLines(modPath);
            var ar = l.Select(a => a.Split('\t')).ToList();
            foreach (string file in Directory.GetFiles(path, "*.xaml"))
            {
                bool notModified=false;
                var wrt = File.GetLastWriteTime(file).ToString();
                for (int i = 0; i < ar.Count(); i++)
                {
                    if (ar[i][0] == file)
                    {                        
                        if (ar[i][1] == wrt)
                            notModified = true;
                        ar.Remove(ar[i]);
                    }                                            
                }
                ar.Add(new[] { file, wrt });
                if (notModified) continue;
                filename = Path.GetFileNameWithoutExtension(file);
                if (filename.Contains("Window"))
                {
                    string xaml = File.ReadAllText(file);
                    object o = (Window)XamlReader.Parse(xaml);
                    if (o is Window)
                    {
                        Window w = (Window)o;
                        w.Left = -1200;
                        w.Top = -1200;
                        w.Show();

                        new Parser() { templ = template, w = w }.Start(w);
                    }
                }
            }
            File.WriteAllLines(modPath, ar.Select(a => a[0] + "\t" + a[1]).ToArray());
        }
        public class Parser
        {
            public Window w;
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
                    ongui += "\t\tGUI.Window(wndid" + fieldi + "," + "new Rect(" + x1 + "," + y1 + "," + Width(c) + "f," + Height(c) + "f)" + ", Wnd" + fieldi + ",\"\"" + GetStyle(c) + ");\r\n";
                    winfunc += "\tvoid Wnd" + fieldi + "(int id){\r\n";
                    winfunc += "\t\tif (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}\r\n";
                    winfunc += "\t\tfocusWindow = false;\r\n";
                    winfunc += "\t\tbool onMouseOver;\r\n";
                    Draw(c);
                    if (!w.Topmost)
                        WriteLine("if (GUI.Button(new Rect(" + Width(c) + "f - 25, 5, 20, 15), \"X\")) { enabled = false;onButtonClick();Action(\"Close\"); }");
                    //WriteLine(@"if(GUI.tooltip!="""") GUI.Label(new Rect(Input.mousePosition.x+10-rect.x,Screen.height -Input.mousePosition.y+10-rect.y, 100, 200), GUI.tooltip);");
                    winfunc += "\t}\r\n";
                }
                templ = templ.Replace("_fields_", prefFields + pubfields + privfields, "_funcs_", winfunc, "_start_", start, "_ongui_", ongui);
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
                        fieldi++;
                        c.Name = c.GetType().Name + fieldi;
                    }
                    else hasname = true;
                    if (hasname)
                    {
                        if (hasname)
                        {
                            WriteinternalField("bool v" + c.Name + " = " + (c.Visibility == 0 ? "true" : "false") + ";");
                            WriteLine("if(v" + c.Name + "){");
                        }

                        c.Name = char.ToUpper(c.Name[0]) + c.Name.Substring(1);
                        WriteinternalField("bool focus" + c.Name + ";");
                        WriteLine("if(focus" + c.Name + ") { focus" + c.Name + " = false; GUI.FocusControl(\"" + c.Name + "\");}");
                        WriteLine("GUI.SetNextControlName(\"" + c.Name + "\");");
                    }
                    if (c is TextBox)
                        if (hasname)
                            TextBox((TextBox)c);
                        else
                            Label((TextBox)c);
                    if (c is Canvas)
                        Canvas((Canvas)c);
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
                    if (hasname)
                        WriteLine("}");
                }
            }
            float Height(FrameworkElement f)
            {
                if (f.Height + "" == "NaN")
                {
                    return (float)f.ActualHeight;
                }
                return (float)f.Height;
            }
            float Width(FrameworkElement f)
            {
                if (f.Width + "" == "NaN")
                {
                    if (f.ActualWidth == 0) Debugger.Break();
                    return (float)f.ActualWidth;
                }
                return (float)f.Width;
            }
            private void Canvas(Canvas c)
            {
                WriteLine("GUI.BeginGroup(new Rect(" + c.GetX() + "f, " + c.GetY() + "f, " + Width(c) + "f, " + Height(c) + "f), \"\");");
                WriteLine("GUI.Box(new Rect(0, 0, " + Width(c) + "f, " + Height(c) + "f), \"\");");
                Draw(c);
                WriteLine("GUI.EndGroup();");

            }
            void Slider(Slider c)
            {
                WritePrefsField("Float", c.Name, c.Value + "f", c.ClipToBounds);
                //WritepublicField("float " + c.Name + " = " + c.Value+"f;");
                WriteLine(c.Name + " = GUI.HorizontalSlider(" + Rect(c) + ", " + c.Name + ", " + c.Minimum + "f, " + c.Maximum + "f);");
                WriteLine("GUI.Label(new Rect(" + (c.GetX() + Width(c)) + "f," + c.GetY() + "f,40,15),System.Math.Round(" + c.Name + ",1).ToString());");
            }
            private void Button(System.Windows.Controls.Primitives.ButtonBase c)
            {
                bool toggle = c is CheckBox;
                var n = c.Name;
                //WritepublicField("Action on" + c.Name + ";");                
                WriteBoolField(n, ((toggle && ((CheckBox)c).IsChecked.Value) ? true : false), c.ClipToBounds);
                WriteLine("bool old" + n + " = " + n + ";");
                WriteLine(n + " = GUI." + (toggle ? "Toggle" : "Button") + "(" + Rect(c) + (toggle ? "," + n : "") + ", new GUIContent(" + GetContent(c.Content) + ",\"" + c.Tag + "\"));");
                WriteLine("if (" + n + " != old" + n + (toggle ? "" : " && " + n) + " ) {Action(\"" + n + "\");onButtonClick(); }");
                WriteLine("onMouseOver = " + Rect(c) + ".Contains(Event.current.mousePosition);");
                WriteLine("if (oldMouseOver" + n + " != onMouseOver && onMouseOver) onOver();");
                WriteLine("oldMouseOver" + n + " = onMouseOver;");
                WritePrivateField("bool oldMouseOver" + n + ";");

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
                fieldi++;
                WriteinternalField("Action onStackPanelDraw" + fieldi + ";");
                WriteLine("GUI.Box(" + Rect(c) + ", \"\");");
                WriteLine("GUILayout.BeginArea(" + Rect(c) + ");");
                WriteLine("if(onStackPanelDraw" + fieldi + " != null) onStackPanelDraw" + fieldi + "();");
                WriteLine("GUILayout.EndArea();");
            }
            private void ProgressBar(ProgressBar c)
            {
                WriteinternalField("float " + c.Name + " = " + c.Value + ";");
                WriteLine("GUI.HorizontalScrollbar(" + Rect(c) + ", 0, Mathf.Min(Mathf.Max(0, " + c.Name + ")," + c.Maximum + "), 0, " + c.Maximum + GetStyle(c) + ");");
                WriteLine("GUI.Label(new Rect(" + (c.GetX() + Width(c) / 4) + "f," + c.GetY() + "f,100,15)," + c.Name + "+\"/\"+" + c.Maximum + " );");
            }
            private void Line(System.Windows.Shapes.Path c)
            {
                WriteLine("GUI.Box(" + Rect(c) + ",\"\",GUI.skin.customStyles[4]);//line");
            }
            private void ListBox(ListBox c)
            {
                var n = c.Name;
                pubfields += "\tpublic string[] l" + n + ";\r\n";
                double h = 15;
                if (c.Items.Count > 0)
                    h = Height((ListBoxItem)c.Items[0]);
                WritePrivateField("Vector2 s" + n + ";");
                WritePrefsField("Int", "i" + n, c.SelectedIndex, c.ClipToBounds);
                WritePublicField("string " + n + " { get { if(l" + n + ".Length==0) return \"\"; return l" + n + "[i" + n + "]; } set { i" + n + " = l" + n + ".SelectIndex(value); }}");
                string rect = "new Rect(0,0, " + (Width(c) - 20) + "f, l" + n + ".Length* " + h + "f)";
                WriteLine("GUI.Box(" + Rect(c) + ", \"\");");
                WriteLine("s" + n + " = GUI.BeginScrollView(" + Rect(c) + ", s" + n + ", " + rect + ");");
                WriteLine("int old" + n + " = i" + n + ";");
                WriteLine("i" + n + " = GUI.SelectionGrid(" + rect + ", i" + n + ", l" + n + ",1,GUI.skin.customStyles[0]);");
                WriteLine("if (i" + n + " != old" + n + ") Action(\"" + n + "\");");
                //WritepublicField("Action<string> on" + n + ";");
                WriteLine("GUI.EndScrollView();");
            }
            private string eval(string s)
            {
                return "@\"" + s.Replace("\"", "\"\"") + "\"";
            }
            private void Label(TextBox c)
            {
                WriteLine("GUI.Label(" + Rect(c) + ", " + eval(c.Text) + GetStyle(c) + ");");
            }
            private void TextBox(TextBox c)
            {
                int o;
                var n = c.Name;
                bool b = (int.TryParse(c.Text, out o));
                WriteinternalField("bool r" + n + " = " + c.IsReadOnly.ToString().ToLower() + ";");
                WriteLine("if(r" + n + "){");
                WriteLine("GUI.Label(" + Rect(c) + ", " + n + ".ToString()" + GetStyle(c) + ");");
                WriteLine("} else");
                WriteLine(n + " = " + (b ? "int.Parse(" : "") +
                    "GUI.TextField(" + Rect(c) + ", " + n + (b ? ".ToString()" : "") + (c.MaxLength == 0 ? ",100" : "," + c.MaxLength) + GetStyle(c) + (b ? ")" : "") + ");");
                WritePrefsField((b ? "Int" : "String"), n, (b ? c.Text : eval(c.Text)), c.ClipToBounds);
                //WritepublicField((b ? "int " : "string ") + n + " = " + (b ? c.Text : eval(c.Text)) + ";");
            }
            string strstr(string s, string a)
            {
                int i = s.LastIndexOf(a);
                return s.Substring(i + a.Length, s.Length - i - 1);
            }
            private void Image(Image a)
            {
                var n = a.Name;
                WritePrivateField("Rect " + n + ";");
                WriteStart(n + " = " + Rect(a) + ";");
                var f = GetImage(a);
                WriteLine("if(" + f + "!=null)");
                WriteLine("\tGUI.DrawTexture(" + n + "," + f + ", ScaleMode.ScaleToFit);");
            }
            string GetImage(Image a)
            {
                if (a.Tag != null)
                    pubfields += "\t[FindAsset(\"" + a.Tag + "\")]\r\n";
                WritePublicField("Texture img" + a.Name + ";");
                return "img" + a.Name;
            }
            private string Rect(FrameworkElement v)
            {
                return "new Rect(" + v.GetX() + "f, " + v.GetY() + "f, " + Width(v) + "f, " + Height(v) + "f)";
            }
            private void TabControl(TabControl c)
            {
                WriteinternalField("int tab" + c.Name + ";");
                WriteLine("GUI.BeginGroup(" + Rect(c) + ", \"\");");
                WriteLine("GUI.Box(new Rect(0, 0, " + Width(c) + "f, " + Height(c) + "f), \"\");");
                int tab = 0;
                string strs = "";
                foreach (TabItem i in c.Items)
                    strs += "\"" + i.Header + "\",";

                WriteLine("GUILayout.BeginArea(new Rect(0f, 0, " + Width(c) + ", 18));");
                WriteLine("tab" + c.Name + " = GUILayout.Toolbar(tab" + c.Name + ", new string[] { " + strs + " }, GUI.skin.customStyles[1], GUILayout.ExpandWidth(false));");
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
                WriteLine("GUI.EndGroup();");
            }
            void WriteStart(string s)
            {
                start += "\t\t" + s + "\r\n";
            }
            enum FT { pub, pref }
            void WriteBoolField(string name, bool value, bool save)
            {
                if (save)
                    pubfields += "\tinternal bool " + name + " { get { return PlayerPrefs.GetInt(\"" + name + "\", " + (value ? 1 : 0) + ") == 1; } set { PlayerPrefs.SetInt(\"" + name + "\", value?1:0); } }\r\n";
                else
                    pubfields += "\tinternal bool " + name + "=" + value.ToString().ToLower() + ";\r\n";
            }
            void WritePrefsField(string type, string name, object def, bool save)
            {
                if (save)
                    prefFields += "\tinternal " + type.ToLower() + " " + name + "{ get { return PlayerPrefs.Get" + type + "(\"" + name + "\", " + def + "); } set { PlayerPrefs.Set" + type + "(\"" + name + "\", value); } }\r\n";
                else
                    pubfields += "\tinternal " + type.ToLower() + " " + name + " = " + def + ";\r\n";

            }
            void WritePrivateField(string s)
            {
                privfields += "\tprivate " + s + "\r\n";
            }
            void WriteinternalField(string s)
            {
                pubfields += "\tinternal " + s + "\r\n";
            }

            void WritePublicField(string s)
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
