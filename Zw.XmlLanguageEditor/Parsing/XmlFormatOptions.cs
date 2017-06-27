using System;

namespace Zw.XmlLanguageEditor.Parsing
{
    internal class XmlFormatOptions : IFormatOptions
    {
        public DataFormat Format => DataFormat.Xml;
        public string RootElementName { get; private set; }

        public XmlFormatOptions(string rootElementName)
        {
            this.RootElementName = rootElementName;
        }
    }
}
