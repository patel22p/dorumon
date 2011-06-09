//for RedLynx by Igor Levochkin
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
using System.Xml;

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
            var xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText("../../map.xml"));
            var prs = xml.DocumentElement;
            //var list = xml.DocumentElement.SelectNodes("/node");
            //Debugger.Break();
            //var prs = XElement.Parse(File.ReadAllText("../../map.xml"));
            //var lms = prs.Elements("node").ToArray();

            var b = prs.SelectSingleNode("bounds");
            map.minlat = float.Parse(b.Attributes["minlat"].Value);
            map.minlon = float.Parse(b.Attributes["minlon"].Value);

            map.maxlat = float.Parse(b.Attributes["maxlat"].Value);
            map.maxlon = float.Parse(b.Attributes["maxlon"].Value);

            foreach (XmlNode xmlnode in prs.SelectNodes("node"))
            {
                Node n = new Node();
                n.id = int.Parse(xmlnode.Attributes["id"].Value);
                n.lat = float.Parse(xmlnode.Attributes["lat"].Value);
                n.lon = float.Parse(xmlnode.Attributes["lon"].Value);
                n.x = (n.lon - map.minlon) / (map.maxlon - map.minlon);
                n.y = (n.lat - map.minlat) / (map.maxlat - map.minlat);
                
                foreach (XmlNode tg in xmlnode.SelectNodes("tag"))
                {
                    n.tags.Add(new Tag { k = tg.Attributes["k"].Value });
                    n.tags.Add(new Tag { v = tg.Attributes["v"].Value });
                }
                map.nodes.Add(n);
            }
            foreach (XmlNode xmlnode in prs.SelectNodes("way"))
            {
                Way way = new Way();
                foreach (XmlNode nd in xmlnode.SelectNodes("nd"))
                {
                    var node = map.nodes.First(a => a.id == int.Parse(nd.Attributes["ref"].Value));
                    way.nodes.Add(node);
                }
                map.ways.Add(way);
            }
        }
    }
    
}
