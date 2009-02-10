using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using doru;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using botva_online.Properties;

namespace botva_online
{
    class Program
    {
        static void Main(string[] args)
        {
            Logging.Setup();
            new Program();
        }

        intA id = new intA("i.txt");
         Settings Settings {get{return Properties.Settings.Default;}}
        public Program()
        {
            StringBuilder sb = new StringBuilder();
            Socket _Socket = Helper.Connect("www.botva-online.ru", 80);
            for (;id.i< Settings._Max; id.i++)
			{
                id.Trace("loading");
                _Socket.Send(String.Format(Res.get, id.i).Save("sended"));
                string s=Http.ReadHttp(_Socket).ToStr().Save("received");                
                MatchCollection ms = Regex.Matches(s, @"text_main_4"">(.+?)<");
                string clan = Regex.Match(s, @"text_main_5"">(.+?)<").Groups[1].Value;
                sb.Append(id.i + ";");
                sb.Append(clan + ";");
                string name = Regex.Match(s, @"text_main_1"">(.+?)<").Groups[1].Value;
                sb.Append(name+ ";");
                foreach (Match m in ms)
                {
                    sb.Append(m.Groups[1].Value + ";");
                }
                sb.AppendLine(); sb.AppendLine();
                File.WriteAllText(Settings._outputfile, sb.ToString());
			}            
        }
    }
}
//for (int i = 0; i < ms.Count; i++)
//{
//    sb.Append(ms[i].Groups[1].Value +":");
//    i++;
//    if (i < ms.Count)
//        sb.Append(ms[i].Groups[1].Value);
//    sb.AppendLine();
//}