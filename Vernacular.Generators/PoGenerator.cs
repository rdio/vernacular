using System;
using System.Collections.Generic;

using Vernacular.Tool;

namespace Vernacular.Generators
{
    public sealed class PoGenerator : Generator
    {
        private static string Escape (string value)
        {
            return String.IsNullOrWhiteSpace (value) ? value : value.Escape ();
        }

        private void WriteComment (char prefix, string value)
        {
            if (String.IsNullOrWhiteSpace (value)) {
                return;
            }

            foreach (var line in value.Split ('\n')) {
                Writer.WriteLine ("#{0} {1}", prefix, line);
            }
        }

        private void WriteString (string id, string value)
        {
            if (id == null || value == null) {
                return;
            }

            Writer.Write (id);
            Writer.Write (' ');

            var lines = value.Split ('\n');
            if (lines.Length > 1) {
                Writer.WriteLine ("\"\"");
                foreach (var line in lines) {
                    if (!String.IsNullOrEmpty (line)) {
                        Writer.WriteLine ("\"{0}\\n\"", Escape (line));
                    }
                }
            } else {
                Writer.WriteLine ("\"{0}\"", Escape (lines [0]));
            }
        }

        protected override void Generate ()
        {
            foreach (var localized_string in Strings) {
                WriteComment (' ', localized_string.TranslatorComments);
                WriteComment ('.', localized_string.DeveloperComments);
                if (localized_string.References != null) {
                    foreach (var reference in localized_string.References) {
                        WriteComment (':', reference);
                    }
                }
                WriteComment (',', localized_string.StringFormatHint);

                var singular = localized_string.UntranslatedSingularValue;
                var plural = localized_string.UntranslatedPluralValue;

                WriteString ("msgid", singular);
                if (plural != null) {
                    WriteString ("msgid_plural", plural);
                }

                var translated = localized_string.TranslatedValues;
                if (translated == null) {
                    if (plural == null) {
                        translated = new [] { singular };
                    } else {
                        translated = new [] { singular, plural };
                    }
                }

                if (translated.Length == 1) {
                    WriteString ("msgstr", translated [0]);
                } else {
                    for (int i = 0; i < translated.Length; i++) {
                        WriteString (String.Format ("msgstr[{0}]", i), translated [i]);
                    }
                }

                Writer.WriteLine ();
            }
        }
    }
}