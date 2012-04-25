using System;
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

        private static string DecodeString (string value)
        {
            return Regex.Replace (value
                .Replace ("\n", " ")
                .Replace ("\r", " ")
                .Replace ("\t", " ")
                .Replace ("\\'", "'")
                .Replace ("\\\"", "\"")
                .Escape (decode: true),
                @"[ ]+", " ", RegexOptions.Multiline).Trim ();
        }

        public override IEnumerable<LocalizedString> Parse ()
        {
            return from xml_path in xml_paths
                   from resource in XDocument.Load (xml_path, LoadOptions.SetLineInfo).Elements ("resources")
                   from @string in resource.Elements ("string")
                   select new LocalizedString {
                       Name = @string.Attribute ("name").Value,
                       References = new [] {
                           String.Format ("{0}:{1}", xml_path, ((IXmlLineInfo)@string).LineNumber)
                       },
                       UntranslatedSingularValue = DecodeString (@string.Value)
                   };
        }
    }
}