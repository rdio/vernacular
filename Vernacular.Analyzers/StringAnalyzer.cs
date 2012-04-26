//
// StringAnalyzer.cs
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
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Vernacular.Tool;

namespace Vernacular.Analyzers
{
    public class StringAnalyzer
    {
        private List<LocalizedString> localized_strings = new List<LocalizedString> ();
        private List<string> illegal_words = new List<string> ();

        public StringAnalyzer (string illegalWordsPath = null)
        {
            if (illegalWordsPath != null) {
                using (var reader = new StreamReader (illegalWordsPath)) {
                    string line;
                    while ((line = reader.ReadLine ()) != null) {
                        illegal_words.Add (line.Trim ().ToLower ());
                    }
                }
            }
        }

        public void Add (LocalizedString localizedString)
        {
            localized_strings.Add (localizedString);
        }

        public void Analyze ()
        {
            var count = 0;
            foreach (var localized_string in localized_strings) {
                if (Analyze (localized_string)) {
                    count++;
                }
            }

            if (count > 0) {
                Console.WriteLine ();
                Console.WriteLine ();
                Console.WriteLine ("{0} possibly problematic strings.", count);
            }
        }

        private bool Analyze (LocalizedString localizedString)
        {
            var warnings = new List<string> ();

            if (Check (CheckFormatArguments, localizedString) &&
                String.IsNullOrWhiteSpace (localizedString.DeveloperComments)) {
                warnings.Add ("String.Format without developer comment");
            } else if (Check (CheckEmpty, localizedString)) {
                warnings.Add ("Empty or whitespace");
            } else if (Check (CheckIllegalWords, localizedString)) {
                warnings.Add ("Possibly illegal words");
            }

            if (warnings.Count == 0) {
                return false;
            }

            if (localizedString.HasReferences) {
                foreach (var reference in localizedString.References) {
                    Console.WriteLine ("Warning: @{0}", reference);
                }
            } else {
                Console.WriteLine ("Warning: @<unknown source location>");
            }

            foreach (var warning in warnings) {
                Console.WriteLine ("  {0}", warning);
            }

            if (localizedString.DeveloperComments != null) {
                Console.WriteLine ("  Developer Comments  = {0}", localizedString.DeveloperComments);
            }

            if (localizedString.UntranslatedSingularValue != null) {
                Console.WriteLine ("  Singular = {0}", localizedString.UntranslatedSingularValue);
            }

            if (localizedString.UntranslatedPluralValue != null) {
                Console.WriteLine ("  Plural   = {0}", localizedString.UntranslatedPluralValue);
            }

            return true;
        }

        private bool Check (Func<string, bool> func, LocalizedString localizedString)
        {
            return func (localizedString.UntranslatedSingularValue) || func (localizedString.UntranslatedPluralValue);
        }

        public static bool CheckFormatArguments (string value)
        {
            if (String.IsNullOrEmpty (value)) {
                return false;
            }

            return Regex.IsMatch (value, @"\{\d+");
        }

        private bool CheckEmpty (string value)
        {
            return String.IsNullOrWhiteSpace (value) && value != null;
        }

        private bool CheckIllegalWords (string value)
        {
            if (String.IsNullOrEmpty (value)) {
                return false;
            }

            value = value.ToLower ();

            foreach (var word in illegal_words) {
                if (value.Contains (word)) {
                    return true;
                }
            }

            return false;
        }
    }
}
