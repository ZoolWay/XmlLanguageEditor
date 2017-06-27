using System;
using System.Collections.Generic;

namespace Zw.XmlLanguageEditor.Parsing
{
    internal class ParseResult
    {
        public IEnumerable<Entry> Records { get; set; }
        public IFormatOptions FormatOptions { get; set; }
    }
}
