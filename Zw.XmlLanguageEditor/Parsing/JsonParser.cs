using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Zw.XmlLanguageEditor.Parsing
{
    internal class JsonParser : IParser
    {
        public string Name => "JsonParser";

        public ParseResult ReadRecords(string filename)
        {
            var result = new ParseResult();
            var records = new List<Entry>();
            var warnings = new List<string>();
            using (TextReader stream = new StreamReader(filename, true))
            {
                using (JsonReader reader = new JsonTextReader(stream))
                {
                    ReadLevel(reader, records, new string[0], warnings);
                }
            }
            result.Records = records;
            result.FormatOptions = new JsonFormatOptions();
            return result;
        }

        public void InjectEntries(string filename, IEnumerable<Entry> entries)
        {
            throw new NotImplementedException();
        }

        public void CreateEmpty(IFormatOptions formatOptions, string fileName)
        {
            throw new NotImplementedException();
        }

        public bool IsSupporting(DataFormat format)
        {
            return (format == DataFormat.Json);
        }

        private void ReadLevel(JsonReader reader, List<Entry> records, string[] parents, List<string> warnings)
        {
            string currentPropertyName = null;
            string levelPrefix = String.Join(".", parents);
            if (parents.Length > 0) levelPrefix += ".";
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    currentPropertyName = reader.Value as string;
                    continue; // skip resetting currentPropertyName below
                }
                else if (reader.TokenType == JsonToken.String)
                {
                    var record = new Entry();
                    record.Id = $"{levelPrefix}{currentPropertyName}";
                    record.Value = reader.Value as string; // property value (must be string)
                    records.Add(record);
                }
                else if ((reader.TokenType == JsonToken.StartObject) && (currentPropertyName != null))
                {
                    var newParents = parents.ToList();
                    newParents.Add(currentPropertyName);
                    ReadLevel(reader, records, newParents.ToArray(), warnings);
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    warnings.Add("JSON Arrays are not supported in language structures!");
                    reader.Skip();
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    return; // go hierachy up, level completed
                }
                // ignore others
                currentPropertyName = null;
            }
        }

    }
}
