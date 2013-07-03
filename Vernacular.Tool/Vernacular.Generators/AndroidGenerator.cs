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
using System.Xml.Linq;
using System.Text;

using Vernacular.Tool;
using Vernacular.Parsers;

namespace Vernacular.Generators
{
    public sealed class AndroidGenerator : StreamGenerator
    {
        protected override void Generate ()
        {
            using (var writer = new XmlTextWriter (Writer)) {
                WriteDocument (writer, parent => {
                    foreach (var resource_string in GetAllResourceStrings ()) {
                        WriteString (parent, resource_string.Id, resource_string.Translated);
                    }
                });
            }
        }

        private void WriteDocument (XmlTextWriter xml, Action<XElement> bodyBuilder)
        {
            xml.Formatting = Formatting.Indented;
            xml.Indentation = 2;

            var resources = new XElement ("resources");
            bodyBuilder (resources);

            new XDocument (resources).WriteTo (xml);
        }

        private void WriteString (XElement parent, string name, string value, bool formatted = false)
        {
            var string_element = XElement.Parse (String.Format ("<string{0}>{1}</string>",
                formatted ? String.Empty : " formatted=\"false\"",
                value.Replace ("'", "\\'")));
            string_element.SetAttributeValue ("name", name);
            parent.Add (string_element);
        }

        public void LocalizeManualStringsXml (string unlocalizedPath, string localizedPath)
        {
            var parser = new AndroidResourceParser ();
            parser.Add (unlocalizedPath);

            using (var xml = new XmlTextWriter (localizedPath, Encoding.UTF8)) {
                WriteDocument (xml, parent => {
                    foreach (LocalizedString @string in parser.Parse ()) {
                        var found_localization = false;

                        foreach (var localized_string in Strings) {
                            if (localized_string.UntranslatedSingularValue == @string.UntranslatedSingularValue) {
                                var translated = String.IsNullOrWhiteSpace (@localized_string.TranslatedValues [0])
                                    ? localized_string.UntranslatedSingularValue
                                    : localized_string.TranslatedValues [0];

                                WriteString (parent, @string.Name, translated, formatted: true);

                                found_localization = true;
                                break;
                            }
                        }

                        if (!found_localization) {
                            WriteString (parent, @string.Name, @string.UntranslatedSingularValue, formatted: true);
                        }
                    }
                });
            }
        }
    }
}
