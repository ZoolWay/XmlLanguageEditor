using System;
using Zw.XmlLanguageEditor.Parsing;

namespace Zw.XmlLanguageEditor.Ui.Events
{
    public class LoadedMasterEvent
    {
        public string Filename { get; private set; }
        public DataFormat Format { get; private set; }
        public IFormatOptions FormatOptions { get; private set; }

        public LoadedMasterEvent(string filename, DataFormat format, IFormatOptions formatOptions)
        {
            this.Filename = filename;
            this.Format = format;
            this.FormatOptions = formatOptions;
        }
    }
}
