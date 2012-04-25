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