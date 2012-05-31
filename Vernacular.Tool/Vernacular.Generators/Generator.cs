//
// Generator.cs
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;

using Vernacular.Tool;
using Vernacular.Parsers;

namespace Vernacular.Generators
{
    public abstract class Generator
    {
        private static readonly Dictionary<string, Type> generators = new Dictionary<string, Type> {
            { "po", typeof (PoGenerator) },
            { "ios", typeof (IosGenerator) },
            { "android", typeof (AndroidGenerator) },
            { "resx", typeof (ResxGenerator) }
        };

        public static IEnumerable<string> GeneratorNames {
            get { return from generator in generators select generator.Key; }
        }

        public static Generator GetGeneratorForName (string name)
        {
            Type type;
            return generators.TryGetValue (name, out type) ? (Generator)Activator.CreateInstance (type) : null;
        }

        private List<LocalizedString> strings = new List<LocalizedString> ();
        public IEnumerable<LocalizedString> Strings {
            get { return from @string in strings select @string; }
        }

        public LocalizationMetadata LocalizationMetadata { get; private set; }

        protected TextWriter Writer { get; private set; }

        protected virtual Encoding Encoding {
            get { return Encoding.UTF8; }
        }

        protected virtual ResourceIdType ResourceIdType {
            get { return ResourceIdType.ComprehensibleIdentifier; }
        }

        protected sealed class GeneratorWriter : StreamWriter
        {
            private Encoding encoding;
            public override Encoding Encoding {
                get { return encoding; }
            }

            public GeneratorWriter (Stream stream, Encoding encoding)
                : base (stream)
            {
                this.encoding = encoding;
                NewLine = "\n";
            }
        }

        public void Generate (string path)
        {
            if (String.IsNullOrEmpty (path) || path == "-") {
                Writer = Console.Out;
            } else {
                Writer = new GeneratorWriter (File.Create (path), Encoding);
            }

            try {
                Generate ();
            } finally {
                Writer.Close ();
                Writer = null;
            }
        }

        public void Add (ILocalizationUnit localizationUnit, bool stripMetadata = false)
        {
            var localized_string = localizationUnit as LocalizedString;
            var localization_metadata =  localizationUnit as LocalizationMetadata;

            if (localization_metadata != null) {
                if (LocalizationMetadata == null) {
                    LocalizationMetadata = new LocalizationMetadata ();
                }
                LocalizationMetadata.Add (localization_metadata);
            } else if (localized_string != null) {
                if (!Merge (localized_string, stripMetadata: stripMetadata)) {
                    if (stripMetadata) {
                        localized_string.StripMetadata ();
                    }
                    strings.Add (localized_string);
                }
            } else {
                throw new ArgumentException ("Unsupported ILocalizationUnit");
            }
        }

        private bool Merge (LocalizedString localizedString, bool stripMetadata = false)
        {
            for (int i = 0; i < strings.Count; i++) {
                if (strings [i].UntranslatedEquals (localizedString)) {
                    var merged = LocalizedString.Merge (strings [i], localizedString);
                    if (stripMetadata) {
                        merged.StripMetadata ();
                    }
                    strings [i] = merged;
                    return true;
                }
            }

            return false;
        }

        protected struct ResourceString
        {
            public string Id { get; set; }
            public string Untranslated { get; set; }
            public string Translated { get; set; }

            public string SortKey {
                get {
                    // We want to chop off the Vernacular_P0_M_ style
                    // prefixes to keep plurals grouped together.
                    return Regex.Replace (Id, @"^Vernacular_P\d+(_[FM]{1})?_", "");
                }
            }
        }

        protected IEnumerable<ResourceString> GetAllResourceStrings ()
        {
            var resource_strings = new Dictionary<string, ResourceString> ();

            foreach (var resource_string in from localized_string in Strings
                                            from resource_string in GetResourceStrings (localized_string)
                                            where !resource_strings.ContainsKey (resource_string.Id)
                                            select resource_string) {
                resource_strings.Add (resource_string.Id, resource_string);
            }

            return from resource_string in resource_strings
                   orderby resource_string.Value.SortKey
                   select resource_string.Value;
        }

        protected IEnumerable<ResourceString> GetResourceStrings (LocalizedString localizedString)
        {
            string [] translated;

            if (localizedString.HasValidTranslations) {
                translated = localizedString.TranslatedValues;
            } else {
                translated = new [] {
                    localizedString.UntranslatedSingularValue,
                    localizedString.UntranslatedPluralValue
                };
            }

            for (int i = 0; i < translated.Length; i++) {
                if (translated [i] == null) {
                    continue;
                }

                var untranslated = i == 0
                    ? localizedString.UntranslatedSingularValue
                    : localizedString.UntranslatedPluralValue;

                if (i == 0 && String.IsNullOrWhiteSpace (untranslated)) {
                    continue;
                }

                yield return new ResourceString {
                    Id = Catalog.GetResourceId (ResourceIdType,
                        localizedString.Context, untranslated,
                        localizedString.Gender, i),
                    Untranslated = untranslated,
                    Translated = translated [i]
                };
            }
        }

        protected abstract void Generate ();

        public void Reduce (Parser masterSet, Parser subSet)
        {
            foreach (var localization_unit in subSet.Parse ()) {
                var localized_string = localization_unit as LocalizedString;
                if (localized_string != null) {
                    Add (localized_string, stripMetadata: true);
                }
            }

            foreach (var localization_unit in masterSet.Parse ()) {
                var localized_string = localization_unit as LocalizedString;
                var localization_metadata = localization_unit as LocalizationMetadata;

                if (localized_string != null) {
                    Merge (localized_string, stripMetadata: true);
                } else if (localization_metadata != null) {
                    Add (localization_metadata);
                }
            }
        }
    }
}
