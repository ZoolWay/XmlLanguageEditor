using System;
using System.IO;

namespace Zw.XmlLanguageEditor.Parsing
{
    internal class FormatDetector
    {
        private const int BUFFER_SIZE = 64;

        public DataFormat Detect(string filename)
        {
            using (var stream = new StreamReader(filename, true))
            {
                char[] buffer = new char[BUFFER_SIZE];
                int readBytes = 0;
                do
                {
                    readBytes = stream.Read(buffer, 0, BUFFER_SIZE);
                    for (int i = 0; i < readBytes; i++)
                    {
                        if (buffer[i] == '<') return DataFormat.Xml;
                    }
                    if (readBytes < BUFFER_SIZE) return DataFormat.Unknown; // no more data
                } while (readBytes > 0);
            }
            return DataFormat.Unknown; // not reachable as stream end triggers (readBytes < BUFFER_SIZE)
        }
    }
}
