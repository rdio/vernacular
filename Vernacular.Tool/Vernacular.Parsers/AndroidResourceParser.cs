//
// AndroidResourceParser.cs
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
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public sealed class AndroidResourceParser : Parser
    {
        private List<string> xml_paths = new List<string> ();

        public override IEnumerable<string> SupportedFileExtensions {
            get {
                yield return ".xml";
                yield return ".axml";
            }
        }

        public override void Add (string path)
        {
            xml_paths.Add (path);
        }

        public override void Add (Stream stream, string path)
        {
            throw new NotSupportedException ();
        }

        private static string DecodeElement (XElement element)
        {
            StringBuilder builder = null;

            foreach (var node in element.Nodes ()) {
                if (builder == null) {
                    builder = new StringBuilder (node.ToString ());
                } else {
                    builder.Append (node.ToString ());
                }
            }

            var value = builder.ToString ();

            return Regex.Replace (value
                .Replace ("\n", " ")
                .Replace ("\r", " ")
                .Replace ("\t", " ")
                .Replace ("\\'", "'")
                .Replace ("\\\"", "\""),
                @"[ ]+", " ", RegexOptions.Multiline).Trim ();
        }

        public override IEnumerable<ILocalizationUnit> Parse ()
        {
            return from xml_path in xml_paths
                   from resource in XDocument.Load (xml_path, LoadOptions.SetLineInfo).Elements ("resources")
                   from @string in resource.Elements ("string")
                   select new LocalizedString {
                       Name = @string.Attribute ("name").Value,
                       References = new [] {
                           String.Format ("{0}:{1}", xml_path, ((IXmlLineInfo)@string).LineNumber)
                       },
                       UntranslatedSingularValue = DecodeElement (@string)
                   };
        }
    }
}
