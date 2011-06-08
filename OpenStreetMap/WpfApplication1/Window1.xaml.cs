//for redlynx by Igor Levochkin
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace OpenStreetMapWPF
{
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
           ParseMap();
           foreach (var w in map.ways)
           {
               var p = new Polygon();
               foreach (var n in w.nodes)
               {
                   var width = canvas.ActualWidth;
                   var height = canvas.ActualHeight ;
                   p.Points.Add(new Point(n.x * width, n.y * height));

               }
               p.StrokeThickness = 4;
               //p.Fill = new SolidColorBrush(Colors.Black);
               p.Stroke = new SolidColorBrush(Colors.Black);
               canvas.Children.Add(p);
                   
           }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            
            base.OnKeyDown(e);
        }
        void ParseMap()
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
                n.id = int.Parse(xmlnode.Attribute("id").Value);
                n.lat = float.Parse(xmlnode.Attribute("lat").Value);
                n.lon = float.Parse(xmlnode.Attribute("lon").Value);
                n.x = (n.lat - map.minlat) / (map.maxlat - map.minlat);
                n.y = (n.lon - map.minlon) / (map.maxlon - map.minlon);
                foreach (var tg in xmlnode.Elements("tag"))
                {
                    n.tags.Add(new Tag { k = tg.Attribute("k").Value });
                    n.tags.Add(new Tag { v = tg.Attribute("v").Value });
                }
                map.nodes.Add(n);
            }
            foreach (var xmlnode in prs.Elements("way"))
            {
                Way way = new Way();
                foreach (var nd in xmlnode.Elements("nd"))
                {
                    var node = map.nodes.First(a => a.id == int.Parse(nd.Attribute("ref").Value));
                    way.nodes.Add(node);
                }
                map.ways.Add(way);
            }
        }
    }
    public class Map
    {
        public float minlat;
        public float minlon;
        public float maxlat;
        public float maxlon;
        public List<Node> nodes = new List<Node>();
        public List<Way> ways = new List<Way>();
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
    public class Way
    {
        public List<Node> nodes = new List<Node>();
    }
    public class Tag
    {
        public string k;
        public string v;
    }
}
