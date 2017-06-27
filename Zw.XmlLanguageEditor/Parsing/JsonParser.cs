using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zw.XmlLanguageEditor.Parsing
{
    internal class JsonParser : IParser
    {
        private static readonly log4net.ILog log = global::log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            log.InfoFormat("Loaded {0} records from JSON: {1}", records.Count, filename);
            result.Records = records;
            result.FormatOptions = new JsonFormatOptions();
            return result;
        }

        public void InjectEntries(string filename, IEnumerable<Entry> entries)
        {
            JObject rootObject;
            using (TextReader stream = new StreamReader(filename, true))
            {
                using (JsonReader reader = new JsonTextReader(stream))
                {
                    rootObject = JObject.Load(reader);
                }
            }
            foreach (var entry in entries)
            {
                var path = entry.Id.Split('.');
                InjectEntry(rootObject, path, entry.Value);
            }
            using (TextWriter stream = new StreamWriter(filename, false))
            {
                using (JsonWriter writer = new JsonTextWriter(stream))
                {
                    writer.Formatting = Formatting.Indented;
                    rootObject.WriteTo(writer);
                }
            }
            log.InfoFormat("Wrote {0} records to JSON: {1}", entries.Count(), filename);
        }

        public void CreateEmpty(IFormatOptions formatOptions, string fileName)
        {
            File.WriteAllText(fileName, "{\n}", Encoding.UTF8);
        }

        public bool IsSupporting(DataFormat format)
        {
            return (format == DataFormat.Json);
        }

        private void InjectEntry(JObject obj, string[] path, string entryValue)
        {
            string currentPropName = path[0];
            bool isLeaf = path.Length <= 1;

            if (obj[currentPropName] == null) // create
            {
                if (isLeaf)
                {
                    obj[currentPropName] = new JValue(entryValue);
                }
                else
                {
                    JObject branch = new JObject();
                    obj[currentPropName] = branch;
                    var childPath = path.ToList().Skip(1).ToArray();
                    InjectEntry(branch, childPath, entryValue);
                }
            }
            else // update
            {
                if (isLeaf)
                {
                    obj[currentPropName] = new JValue(entryValue);
                }
                else
                {
                    var childobj = obj[currentPropName] as JObject;
                    if (childobj != null)
                    {
                        var childPath = path.ToList().Skip(1).ToArray();
                        InjectEntry(childobj, childPath, entryValue);
                    }
                    else
                    {
                        throw new NotImplementedException("Changing JSON structure to fit the master path is not yet implemented!");
                    }
                }
            }
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
