using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace ConsoleApplication20
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.Beep();
        }
        public class Database
        {
            public string _Test;

        }
        public Program()
        {
            XmlReflectionImporter _XmlReflectionImporter = new XmlReflectionImporter();
            XmlSchemas _XmlSchemas = new XmlSchemas();
            XmlSchemaExporter _XmlSchemaExporter = new XmlSchemaExporter(_XmlSchemas);

            XmlTypeMapping map = _XmlReflectionImporter.ImportTypeMapping(typeof(Database));
            _XmlSchemaExporter.ExportTypeMapping(map);

            TextWriter _TextWriter = new StreamWriter("asd.xsd");
            _XmlSchemas[0].Write(_TextWriter);
            _TextWriter.Close();
        }
    }
}
