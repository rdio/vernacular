using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Vernacular.Tool;

namespace Vernacular.Analyzers
{
    public class StringAnalyzer
    {
        private List<LocalizedString> localized_strings = new List<LocalizedString> ();

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

            return value.ToLower ().Contains ("rdio");
        }
    }
}
