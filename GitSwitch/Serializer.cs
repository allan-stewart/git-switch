﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GitSwitch
{
    public class Serializer : ISerializer
    {
        public T Read<T>(string filePath)
        {
            var settings = new XmlReaderSettings
            {
                CloseInput = true,
                ConformanceLevel = ConformanceLevel.Document,
                IgnoreComments = true
            };

            using (var reader = XmlReader.Create(filePath, settings))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }

        public void Write<T>(string filePath, T TInput)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, TInput);
            }
        }
    }
}
