//
// XamlParser.cs
//
// Author:
//   Aaron Bockover <abock@rd.io>
//
// Copyright 2012 Rdio, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public sealed class XamlParser : Parser
    {
        private List<string> xaml_paths = new List<string> ();
        List<Stream> xaml_streams = new List<Stream>(); 

        public override IEnumerable<string> SupportedFileExtensions {
            get { yield return ".xaml"; }
        }

        public override void Add (string path)
        {
            xaml_paths.Add (path);
        }

        public override void Add(Stream stream)
        {
            var newStream = new MemoryStream();
            var size = 0;
            var buffer = new byte[2048];
            do
            {
                size = stream.Read(buffer, 0, buffer.Length);
                newStream.Write(buffer,0,size);
            } while (size > 0);
            xaml_streams.Add(newStream);
        }

        public override IEnumerable<ILocalizationUnit> Parse ()
        {
            return (from xaml_path in xaml_paths
                   from localization_unit in Parse (xaml_path)
                   select localization_unit).Concat(
                       from xamlStream in xaml_streams 
                       from localization_unit in Parse(xamlStream)
                       select localization_unit);
        }

        IEnumerable<LocalizedString> Parse (Stream stream)
        {
            using (var reader = new XmlTextReader(stream))
            {
                return Parse(reader);
            }
        }

        IEnumerable<LocalizedString> Parse (string xamlPath)
        {
            using (var reader = new XmlTextReader(xamlPath))
            {
                return Parse(reader, xamlPath);
            }
        }

        IEnumerable<LocalizedString> Parse(XmlTextReader reader, string xamlPath=null)
        {
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element || !reader.HasAttributes)
                {
                    continue;
                }

                var in_app_bar = false;

                if (reader.NamespaceURI.Contains("clr-namespace:Microsoft.Phone.Shell"))
                {
                    var name = reader.Name.Substring(reader.Prefix.Length + 1);
                    in_app_bar = name == "ApplicationBarIconButton" || name == "ApplicationBarMenuItem";
                }

                var localized_string = new LocalizedString();

                while (reader.MoveToNextAttribute())
                {
                    if (!in_app_bar && !reader.NamespaceURI.Contains("clr-namespace:Vernacular.Xaml"))
                    {
                        continue;
                    }

                    var name = String.IsNullOrEmpty(reader.Prefix)
                        ? reader.Name
                        : reader.Name.Substring(reader.Prefix.Length + 1);

                    switch (name)
                    {
                        case "Text": // only valid when in_app_bar is true
                        case "Catalog.Message":
                            localized_string.UntranslatedSingularValue = reader.Value;
                            if (reader.HasLineInfo())
                            {
                                localized_string.AddReference(RelativeDocumentUrl(xamlPath), reader.LineNumber);
                            }
                            break;
                        case "Catalog.PluralMessage":
                            localized_string.UntranslatedPluralValue = reader.Value;
                            break;
                        case "Catalog.Comment":
                            localized_string.DeveloperComments = reader.Value;
                            break;
                    }
                }

                if (localized_string.IsDefined)
                {
                    yield return localized_string;
                }

                reader.MoveToElement();
            }
        }        
    }
}
