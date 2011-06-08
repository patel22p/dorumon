//for redlynx by Igor Levochkin
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;

namespace OpenStreetMapWPF
{
    
    public class Map
    {
        public float minlat;
        public float minlon;
        public float maxlat;
        public float maxlon;
        public List<Node> nodels = new List<Node>();
    }
    public class Node
    {
        public int id;
        public float lat;
        public float lon;
        public float x;
        public float y;
        public List<Tag> tags = new List<Tag>();
    }
    public class Tag
    {
        public string k;
        public string v;
    }
    public partial class Window1 : Window
    {
        public Map map = new Map();
        public Window1()
        {
            
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            
            var prs = XElement.Parse(File.ReadAllText("../../map.xml"));
            //var lms = prs.Elements("node").ToArray();
            
            var b = prs.Element("bounds");
            map.minlat = float.Parse(b.Attribute("minlat").Value);
            map.minlon = float.Parse(b.Attribute("minlon").Value);
            
            map.maxlat = float.Parse(b.Attribute("maxlat").Value);
            map.maxlon = float.Parse(b.Attribute("maxlon").Value);
            
            foreach (var xmlnode in prs.Elements("node"))
            {
                Node n = new Node();
                n.lat = float.Parse(xmlnode.Attribute("lat").Value);
                n.lon = float.Parse(xmlnode.Attribute("lon").Value);
                n.x = (n.lat - map.minlat) / (map.maxlat - map.minlat);
                n.y = (n.lon - map.minlon) / (map.maxlon - map.minlon);
                foreach (var tg in xmlnode.Elements("tag"))
                {
                    n.tags.Add(new Tag { k = tg.Attribute("k").Value });
                    n.tags.Add(new Tag { v = tg.Attribute("v").Value });
                }
            }
            

            Debugger.Break();

            //XmlSerializer xml = new XmlSerializer(typeof(List<object>), new[] { typeof(Map), typeof(Node) });
            //var ms = new MemoryStream(File.ReadAllBytes("../../map.xml"));
            //var ms = new MemoryStream();
            //xml.Deserialize(ms);
            //xml.Serialize(ms,items);
            //ms.Position = 0;
            //string s = new StreamReader(ms).ReadToEnd();
            //Regex
            //Debugger.Break();
        }
    }
}
