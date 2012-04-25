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

        public override IEnumerable<string> SupportedFileExtensions {
            get { yield return ".xaml"; }
        }

        public override void Add (string path)
        {
            xaml_paths.Add (path);
        }

        public override IEnumerable<LocalizedString> Parse ()
        {
            return from xaml_path in xaml_paths
                   from localized_string in Parse (xaml_path)
                   select localized_string;
        }

        private IEnumerable<LocalizedString> Parse (string xamlPath)
        {
            using (var reader = new XmlTextReader (xamlPath)) {
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
                                localized_string.UntranslatedSingularValue = reader.Value;
                                if (reader.HasLineInfo ()) {
                                    localized_string.AddReference (RelativeDocumentUrl (xamlPath), reader.LineNumber);
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

                    if (localized_string.IsDefined) {
                        yield return localized_string;
                    }

                    reader.MoveToElement ();
                }
            }
        }
    }
}