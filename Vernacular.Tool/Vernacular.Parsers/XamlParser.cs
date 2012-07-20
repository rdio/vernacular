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
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public sealed class XamlParser : Parser
    {
        private readonly List<string> xaml_paths = new List<string> ();
        private readonly List<Tuple<string, Stream>> xaml_streams = new List<Tuple<string, Stream>> ();

        public override IEnumerable<string> SupportedFileExtensions {
            get { yield return ".xaml"; }
        }

        public override void Add (string path)
        {
            xaml_paths.Add (path);
        }

        public override void Add (Stream stream, string path)
        {
            var memory_stream = new MemoryStream ();
            int size;
            var buffer = new byte[2048];

            do {
                size = stream.Read (buffer, 0, buffer.Length);
                memory_stream.Write (buffer, 0, size);
            } while (size > 0);

            memory_stream.Seek (0, SeekOrigin.Begin);

            xaml_streams.Add (new Tuple<string, Stream> (path, memory_stream));
        }

        public override IEnumerable<ILocalizationUnit> Parse ()
        {
            var from_paths = from xaml_path in xaml_paths
                             from localization_unit in Parse (xaml_path)
                             select localization_unit;
            var from_streams = from xaml_stream in xaml_streams
                              from localization_unit in Parse (xaml_stream.Item2, xaml_stream.Item1)
                              select localization_unit;
            return from_paths.Concat (from_streams);            
        }

        private IEnumerable<LocalizedString> Parse (XmlTextReader reader, string xamlPath)
        {
            while (reader.Read ()) {
                if (reader.NodeType != XmlNodeType.Element || !reader.HasAttributes) {
                    continue;
                }

                var in_app_bar = false;

                if (reader.NamespaceURI.Contains ("clr-namespace:Microsoft.Phone.Shell")) {
                    var name = reader.Name.Substring (reader.Prefix.Length + 1);
                    in_app_bar = name == "ApplicationBarIconButton" || name == "ApplicationBarMenuItem";
                }

                var localized_string = new LocalizedString ();
                var is_binding = false;

                while (reader.MoveToNextAttribute ()) {
                    if (!in_app_bar && !reader.NamespaceURI.Contains ("clr-namespace:Vernacular.Xaml")) {
                        continue;
                    }

                    var name = String.IsNullOrEmpty (reader.Prefix)
                        ? reader.Name
                        : reader.Name.Substring (reader.Prefix.Length + 1);

                    switch (name) {
                        case "Text": // only valid when in_app_bar is true
                        case "Catalog.Message":
                            var value = reader.Value;
                            is_binding = IsBinding (value);
                            localized_string.UntranslatedSingularValue = Unescape (value);
                            AddReference (localized_string, reader, xamlPath);
                            break;
                        case "Catalog.PluralMessage":
                            localized_string.UntranslatedPluralValue = Unescape (reader.Value);
                            break;
                        case "Catalog.Comment":
                            localized_string.DeveloperComments = reader.Value;
                            break;
                        case "Catalog.ToolTip":
                            // Here we want to yield a new string directly since
                            // this could be mixed with other Catalog attributes
                            // on the element (e.g. two separate localized strings
                            // could be returned for the element)
                            yield return AddReference (new LocalizedString {
                                UntranslatedSingularValue = Unescape (reader.Value)
                            }, reader, xamlPath);
                            break;
                    }
                }

                if (localized_string.IsDefined && !is_binding) {
                    yield return localized_string;
                }

                reader.MoveToElement ();
            }
        }

        private LocalizedString AddReference (LocalizedString localizedString, XmlTextReader reader, string xamlPath)
        {
            if (reader.HasLineInfo ()) {
                localizedString.AddReference (RelativeDocumentUrl (xamlPath), reader.LineNumber);
            }

            return localizedString;
        }

        private IEnumerable<LocalizedString> Parse (Stream stream, string xamlPath)
        {
            using (var reader = new XmlTextReader (stream)) {
                return Parse (reader, xamlPath).ToList ();
            }
        }

        private IEnumerable<LocalizedString> Parse (string xamlPath)
        {
            using (var reader = new XmlTextReader (xamlPath)) {
                return Parse (reader, xamlPath);
            }
        }

        /// <summary>
        /// Detects if <param name="text">text</param> is a a Binding expression.
        /// Make sure it doesn't match the escpae sequence "{}"
        /// </summary>
        private bool IsBinding (string text)
        {
            const string regex = @"^\s*\{[^\}]";
            return Regex.Match (text, regex).Success;
        }

        /// <summary>
        /// Unescape xaml positional parameters (i.e. removes all occurences of "{}")
        /// </summary>
        private string Unescape (string value)
        {
            return value.Replace ("{}{", "{");
        }
    }
}
