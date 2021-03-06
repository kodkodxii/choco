﻿// Copyright © 2011 - Present RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.infrastructure.services
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using configuration;
    using filesystem;

    /// <summary>
    ///   XML interaction
    /// </summary>
    public sealed class XmlService : IXmlService
    {
        private readonly IFileSystem _fileSystem;

        public XmlService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public XmlType deserialize<XmlType>(string xmlFilePath)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof (XmlType));
                var xmlReader = XmlReader.Create(new StringReader(_fileSystem.read_file(xmlFilePath)));
                if (!xmlSerializer.CanDeserialize(xmlReader))
                {
                    this.Log().Warn("Cannot deserialize response of type {0}", typeof (XmlType));
                    return default(XmlType);
                }

                return (XmlType) xmlSerializer.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                this.Log().Error("Error deserializing response of type {0}:{1}{2}", typeof (XmlType), Environment.NewLine, ex.ToString());
                throw;
            }
        }

        public void serialize<XmlType>(XmlType xmlType, string xmlFilePath)
        {
            _fileSystem.create_directory_if_not_exists(_fileSystem.get_directory_name(xmlFilePath));
            try
            {
                if (_fileSystem.file_exists(xmlFilePath))
                {
                    _fileSystem.delete_file(xmlFilePath);
                }

                var xmlSerializer = new XmlSerializer(typeof (XmlType));
                var textWriter = new StreamWriter(xmlFilePath, append: false, encoding: Encoding.UTF8)
                    {
                        AutoFlush = true
                    };

                xmlSerializer.Serialize(textWriter, xmlType);
                textWriter.Flush();

                textWriter.Close();
                textWriter.Dispose();
            }
            catch (Exception ex)
            {
                this.Log().Error("Error serializing type {0}:{1}{2}", typeof (XmlType), Environment.NewLine, Config.get_configuration_settings().Debug ? ex.ToString() : ex.Message);
                throw;
            }
        }
    }
}