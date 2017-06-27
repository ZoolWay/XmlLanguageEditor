using System;
using System.Collections.Generic;
using System.Xml;

namespace Zw.XmlLanguageEditor.Parsing
{
    /// <summary>
    /// Parser for simple XML language files where the XML element name is the text id. No nesting supported.
    /// </summary>
    internal class XmlParser : IParser
    {
        private static readonly log4net.ILog log = global::log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void CreateEmpty(IFormatOptions options, string fileName)
        {
            var xmlOptions = options as XmlFormatOptions;
            if (xmlOptions == null) throw new ArgumentException("XmlParser requires XmlFormatOptions!", nameof(options));
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement(xmlOptions.RootElementName));
            doc.Save(fileName);
        }

        public ParseResult ReadRecords(string filename)
        {
            var result = new ParseResult();
            var xmlOptions = new XmlFormatOptions();
            var records = new List<Entry>();
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            log.InfoFormat("Loaded '{0}' with {1} child nodes for reading", filename, doc.DocumentElement?.ChildNodes?.Count ?? -1);
            xmlOptions.RootElementName = doc.DocumentElement?.Name;
            result.FormatOptions = xmlOptions;
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element) continue;
                var record = new Entry() { Id = node.Name, Value = node.InnerText };
                records.Add(record);
            }
            result.Records = records;
            return result;
        }

        public void InjectEntries(string fileName, IEnumerable<Entry> entries)
        {
            int countModified = 0;
            int countCreated = 0;
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            log.InfoFormat("Loaded '{0}' with {1} child nodes for merging", fileName, doc.DocumentElement?.ChildNodes?.Count ?? -1);
            foreach (var entry in entries)
            {
                var node = doc.DocumentElement.SelectSingleNode(GetXPath(entry)) as XmlElement;
                if (node == null)
                {
                    node = doc.CreateElement(entry.Id);
                    doc.DocumentElement.AppendChild(node);
                    countCreated++;
                }
                if (node.InnerText != entry.Value)
                {
                    node.InnerText = entry.Value;
                    countModified++;
                }
            }
            doc.Save(fileName);
            log.InfoFormat("Saved file with {0} changes applied ({1} nodes created)", countModified, countCreated);
        }

        private string GetXPath(Entry entry)
        {
            return String.Format("{0}", entry.Id);
        }

    }
}
