using System;

namespace Zw.XmlLanguageEditor.Parsing
{
    internal class XmlFormatOptions : IFormatOptions
    {
        public string RootElementName { get; private set; }

        public XmlFormatOptions(string rootElementName)
        {
            this.RootElementName = rootElementName;
        }
    }
}
