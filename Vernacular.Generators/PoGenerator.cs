//
// PoGenerator.cs
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
using System.Text;
using System.Collections.Generic;

using Vernacular.Tool;

namespace Vernacular.Generators
{
    public sealed class PoGenerator : Generator
    {
        public bool PotMode { get; set; }

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

        private void GenerateMetadata ()
        {
            if (LocalizationMetadata == null) {
                Add (new LocalizationMetadata ());
            }

            if (!LocalizationMetadata.ContainsKey ("Project-Id-Version")) {
                LocalizationMetadata.Add ("Project-Id-Version", "PACKAGE VERSION");
            }

            if (!LocalizationMetadata.ContainsKey ("PO-Revision-Date")) {
                LocalizationMetadata.Add ("PO-Revision-Date", DateTime.Now.ToString (@"yyyy\-MM\-dd HH\:mmzz",
                    System.Globalization.CultureInfo.InvariantCulture));
            }

            if (!LocalizationMetadata.ContainsKey ("Last-Translator")) {
                LocalizationMetadata.Add ("Last-Translator", String.Empty);
            }

            if (!LocalizationMetadata.ContainsKey ("Language-Team")) {
                LocalizationMetadata.Add ("Language-Team", String.Empty);
            }

            if (!LocalizationMetadata.ContainsKey ("MIME-Version")) {
                LocalizationMetadata.Add ("MIME-Version", "1.0");
            }

            if (!LocalizationMetadata.ContainsKey ("Content-Type")) {
                LocalizationMetadata.Add ("Content-Type", "text/plain; charset=UTF-8");
            }

            if (!LocalizationMetadata.ContainsKey ("Content-Transfer-Encoding")) {
                LocalizationMetadata.Add ("Content-Transfer-Encoding", "8bit");
            }

            if (!LocalizationMetadata.ContainsKey ("Plural-Forms")) {
                LocalizationMetadata.Add ("Plural-Forms", "nplurals=2; plural=(n != 1);");
            }

            LocalizationMetadata.Add ("X-Generator", "Vernacular");
        }

        protected override void Generate ()
        {
            GenerateMetadata ();

            WriteString ("msgid", String.Empty);
            var builder = new StringBuilder ();
            foreach (string header in LocalizationMetadata) {
                builder.AppendFormat ("{0}: {1}\n", header, LocalizationMetadata [header]);
            }
            WriteString ("msgstr", builder.ToString ());
            Writer.WriteLine ();

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
                    WriteString ("msgstr", PotMode ? String.Empty : translated [0]);
                } else {
                    for (int i = 0; i < translated.Length; i++) {
                        WriteString (String.Format ("msgstr[{0}]", i), PotMode ? String.Empty : translated [i]);
                    }
                }

                Writer.WriteLine ();
            }
        }
    }
}
