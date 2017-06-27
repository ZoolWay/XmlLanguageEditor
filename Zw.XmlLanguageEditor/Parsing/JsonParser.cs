using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zw.XmlLanguageEditor.Parsing
{
    internal class JsonParser : IParser
    {
        public string Name => "JsonParser";

        public ParseResult ReadRecords(string filename)
        {
            var result = new ParseResult();
            var records = new List<Entry>();
            JObject rootObject;
            using (TextReader stream = new StreamReader(filename, true))
            {
                using (JsonReader reader = new JsonTextReader(stream))
                {
                    rootObject = JObject.Load(reader);
                }
            }
            ReadLevel(rootObject, records);
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

        private void ReadLevel(JObject obj, List<Entry> records)
        {
            foreach (JProperty property in obj.Properties())
            {
                if (property.Type == JTokenType.Property)
                {
                    if (property.Value is JObject)
                    {
                        ReadLevel((JObject)property.Value, records);
                    }
                    else if (property.Value is JValue)
                    {
                        Entry record = new Entry();
                        record.Id = property.Path;
                        record.Value = ((JValue)property.Value).Value<string>();
                        records.Add(record);
                    }
                }
            }
        }

    }
}
