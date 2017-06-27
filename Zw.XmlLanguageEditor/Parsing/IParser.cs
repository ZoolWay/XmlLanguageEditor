using System;
using System.Collections.Generic;

namespace Zw.XmlLanguageEditor.Parsing
{
    /// <summary>
    /// Defines a parser for language constants.
    /// </summary>
    internal interface IParser
    {
        /// <summary>
        /// Reads langauge records from the given file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        ParseResult ReadRecords(string filename);

        /// <summary>
        /// Writes records to the given file, merging with existing content.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="entries"></param>
        void InjectEntries(string filename, IEnumerable<Entry> entries);

        /// <summary>
        /// Creates an empty file for language constants.
        /// </summary>
        /// <param name="formatOptions"></param>
        /// <param name="fileName"></param>
        void CreateEmpty(IFormatOptions formatOptions, string fileName);
    }
}
