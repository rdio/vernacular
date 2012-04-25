//
// AndroidGenerator.cs
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
using System.Xml;
using System.Text;

using Vernacular.Parsers;

namespace Vernacular.Generators
{
    public sealed class AndroidGenerator : Generator
    {
        protected override void Generate ()
        {
            using (var xml = new XmlTextWriter (Writer)) {
                WriteDocument (xml, () => {
                    foreach (var localized_string in Strings) {
                        foreach (var resource_string in GetResourceStrings (localized_string)) {
                            if (!HasResourceStringBeenGenerated (resource_string)) {
                                xml.WriteComment (String.Format (" Untranslated: {0} ",
                                    resource_string.Untranslated.Replace ("--", "\\-\\-")));
                                WriteString (xml, resource_string.Id, resource_string.Translated);
                                MarkResourceStringAsGenerated (resource_string);
                            }
                        }
                    }
                });
            }
        }

        private void WriteDocument (XmlTextWriter xml, Action bodyBuilder)
        {
            xml.Formatting = Formatting.Indented;
            xml.Indentation = 2;

            xml.WriteStartDocument ();
            xml.WriteStartElement ("resources");

            bodyBuilder ();

            xml.WriteEndElement ();
            xml.WriteEndDocument ();
        }

        private void WriteString (XmlTextWriter xml, string name, string value)
        {
            xml.WriteStartElement ("string");
            xml.WriteAttributeString ("name", name);
            xml.WriteValue (value.Replace ("'", "\\'"));
            xml.WriteEndElement ();
        }

        public void LocalizeManualStringsXml (string unlocalizedPath, string localizedPath)
        {
            var parser = new AndroidResourceParser ();
            parser.Add (unlocalizedPath);

            using (var xml = new XmlTextWriter (localizedPath, Encoding.UTF8)) {
                WriteDocument (xml, () => {
                    foreach (var @string in parser.Parse ()) {
                        foreach (var localized_string in Strings) {
                            if (localized_string.UntranslatedSingularValue == @string.UntranslatedSingularValue) {
                                WriteString (xml, @string.Name, @localized_string.TranslatedValues[0]);
                            }
                        }
                    }
                });
            }
        }
    }
}
