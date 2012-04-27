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
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Vernacular.Tool;

namespace Vernacular.Analyzers
{
    public class StringAnalyzer
    {
        private List<LocalizedString> localized_strings = new List<LocalizedString> ();
        private AnalyzerConfiguration configuration;

        public StringAnalyzer (string configurationPath = null)
        {
            configuration = new AnalyzerConfiguration (configurationPath);
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
            } else if (Check (CheckTags, localizedString)) {
                warnings.Add ("Possibly invalid tags");
            }

            warnings.AddRange (localizedString.Warnings);

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

            return Regex.IsMatch (value, @"\{\d+", RegexOptions.Multiline);
        }

        private bool CheckEmpty (string value)
        {
            return String.IsNullOrWhiteSpace (value) && value != null;
        }

        private bool CheckIllegalWords (string value)
        {
            if (String.IsNullOrWhiteSpace (value)) {
                return false;
            }

            value = value.ToLower ();

            foreach (var word in configuration.IllegalWords) {
                if (value.Contains (word.Key)) {
                    if (word.Value != null) {
                        foreach (var except in word.Value) {
                            if (value == except) {
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private bool CheckTags (string value)
        {
            if (String.IsNullOrWhiteSpace (value) ||
                configuration.SupportedTags == null || configuration.SupportedTags.Count == 0) {
                return false;
            }

            foreach (Match match in new Regex (@"\[(\w+)\]", RegexOptions.Multiline).Matches (value)) {
                if (match != null && match.Groups.Count > 1 &&
                    !configuration.SupportedTags.Contains (match.Groups [1].Value)) {
                    return true;
                }
            }

            return false;
        }
    }
}
