using System;

namespace Zw.XmlLanguageEditor.Parsing
{
    /// <summary>
    /// Defines common format options.
    /// Remember to have all implementations as immutables.
    /// </summary>
    public interface IFormatOptions
    {
        DataFormat Format { get; }
    }
}
