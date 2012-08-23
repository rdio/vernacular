//
// LocalizedString.cs
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

namespace Vernacular
{
    public class LocalizedString : ILocalizationUnit
    {
        public string Name { get; set; }
        public string Context { get; set; }
        public string UntranslatedSingularValue { get; set; }
        public string UntranslatedPluralValue { get; set; }
        public string [] TranslatedValues { get; set; }

        public string StringFormatHint { get; set; }
        public string [] References { get; set; }
        public string DeveloperComments { get; set; }
        public string TranslatorComments { get; set; }
        public LanguageGender Gender { get; set; }

        private List<string> warnings = new List<string> ();
        public List<string> Warnings {
            get { return warnings; }
        }

        public void AddReference (string path, int line)
        {
            if (path != null) {
                if (line >= 0) {
                    path += String.Format (":{0}", line);
                }
                if (References == null) {
                    References = new [] { path };
                } else {
                    References = new List<string> (References) { path }.ToArray ();
                }
            }
        }

        public void StripMetadata ()
        {
            StringFormatHint = null;
            References = null;
            DeveloperComments = null;
            TranslatorComments = null;
        }

        private bool IsPopulated (string [] array)
        {
            return array != null && array.Length > 0 && array [0] != null;
        }

        public bool IsDefined {
            get { return UntranslatedSingularValue != null; }
        }

        public bool HasReferences {
            get { return IsPopulated (References); }
        }

        public bool UntranslatedEquals (LocalizedString other)
        {
            return
                Context == other.Context &&
                UntranslatedSingularValue == other.UntranslatedSingularValue &&
                UntranslatedPluralValue == other.UntranslatedPluralValue &&
                Gender == other.Gender;
        }

        private static string Join (string a, string b)
        {
            var a_null = String.IsNullOrWhiteSpace (a);
            var b_null = String.IsNullOrWhiteSpace (b);

            if (a_null && b_null) {
                return null;
            } else if (!a_null && b_null) {
                return a;
            } else if (a_null && !b_null) {
                return b;
            } else {
                a = a.Trim ();
                b = b.Trim ();

                if (String.Equals(a, b, StringComparison.InvariantCultureIgnoreCase)) {
                    return a;
                } else {
                    return String.Join ("; ", a, b);
                }
            }
        }

        public static LocalizedString Merge (LocalizedString a, LocalizedString b)
        {
            if (!a.UntranslatedEquals (b)) {
                throw new Exception ("Cannot merge two strings with different untranslated values");
            }

            var merged = new LocalizedString {
                Context = a.Context,
                UntranslatedSingularValue = a.UntranslatedSingularValue,
                UntranslatedPluralValue = a.UntranslatedPluralValue,
                Gender = a.Gender,
                StringFormatHint = a.StringFormatHint ?? b.StringFormatHint,
                DeveloperComments = Join (a.DeveloperComments, b.DeveloperComments),
                TranslatorComments = Join (a.TranslatorComments, b.TranslatorComments)
            };

            var a_translated = new List<string> (a.TranslatedValues ?? new string [0]);
            var b_translated = new List<string> (b.TranslatedValues ?? new string [0]);

            if (b_translated.Count >= a_translated.Count) {
                a_translated = b_translated;
            }

            if (a_translated.Count > 0) {
                merged.TranslatedValues = a_translated.ToArray ();
            }

            var a_references = new List<string> (a.References ?? new string [0]);
            var b_references = new List<string> (b.References ?? new string [0]);

            foreach (var reference in b_references) {
                if (!a_references.Contains (reference)) {
                    a_references.Add (reference);
                }
            }

            if (a_references.Count > 0) {
                merged.References = a_references.ToArray ();
            }

            return merged;
        }

        public bool HasValidTranslations {
            get {
                if (TranslatedValues == null || TranslatedValues.Length == 0) {
                    return false;
                }

                foreach (var translated_value in TranslatedValues) {
                    if (!String.IsNullOrWhiteSpace (translated_value)) {
                        return true;
                    }
                }

                return false;
            }
        }

        private static string Escape (string str)
        {
            return str.Replace ("\"", "\\\"").Replace ("\\", "\\\\");
        }

        public override string ToString ()
        {
            var builder = new StringBuilder ();

            Action<string, string> serialize_string = (key, value) => {
                if (!String.IsNullOrWhiteSpace (value)) {
                    builder.AppendFormat ("  \"{0}\": \"{1}\",\n", key, Escape (value));
                }
            };

            Action<string, string []> serialize_array = (key, array) => {
                if (array == null || array.Length == 0) {
                    return;
                }

                builder.AppendFormat ("  \"{0}\": [\n", key);
                for (int i = 0, n = array.Length; i < n; i++) {
                    builder.AppendFormat ("    \"{0}\"", Escape (array [i].ToString ()));
                    if (i < array.Length - 1) {
                        builder.Append (',');
                    }
                    builder.Append ('\n');
                }
                builder.Append ("  ],\n");
            };

            serialize_string ("translatorComments", TranslatorComments);
            serialize_string ("developerComments", DeveloperComments);
            serialize_string ("stringFormatHint", StringFormatHint);
            if (Gender != LanguageGender.Neutral) {
                serialize_string ("gender", Gender.ToString ());
            }
            serialize_array ("references", References);
            serialize_string ("untranslatedSingularValue", UntranslatedSingularValue);
            serialize_string ("untranslatedPluralValue", UntranslatedPluralValue);
            serialize_array ("translatedValues", TranslatedValues);

            if (builder.Length > 0) {
                builder.Length--;
                if (builder [builder.Length - 1] == ',') {
                    builder.Length--;
                }

                builder.Insert (0, "{\n");
                builder.Append ('\n');
            } else {
                builder.Insert (0, "{");
            }

            builder.Append ("}\n");

            return builder.ToString ();
        }
    }
}
