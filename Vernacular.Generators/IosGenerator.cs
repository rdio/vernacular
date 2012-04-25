using System;

namespace Vernacular.Generators
{
    public sealed class IosGenerator : Generator
    {
        private static string Escape (string @string)
        {
            return @string.Replace ("\"", "\\\"");
        }

        protected override System.Text.Encoding Encoding {
            get { return System.Text.Encoding.Unicode; }
        }

        protected override void Generate ()
        {
            foreach (var localized_string in Strings) {
                foreach (var resource_string in GetResourceStrings (localized_string)) {
                    if (!HasResourceStringBeenGenerated (resource_string)) {
                        MarkResourceStringAsGenerated (resource_string);
                        Writer.WriteLine ("/* Untranslated: {0} */", resource_string.Untranslated);
                        Writer.WriteLine ("\"{0}\" = \"{1}\";", resource_string.Id, Escape (resource_string.Translated));
                        Writer.WriteLine ();
                    }
                }
            }
        }
    }
}