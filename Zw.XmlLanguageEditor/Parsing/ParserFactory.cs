using System;

namespace Zw.XmlLanguageEditor.Parsing
{
    internal class ParserFactory
    {
        public IParser CreateParser(DataFormat format)
        {
            if (format == DataFormat.Xml) return new XmlParser();
            throw new Exception($"The given langauge data format '{format}' is not supported!");
        }
    }
}
