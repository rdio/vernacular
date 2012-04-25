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

        private List<string> generated_resource_string_ids = new List<string> ();

        protected TextWriter Writer { get; private set; }

        protected virtual Encoding Encoding {
            get { return Encoding.UTF8; }
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

        public void Add (LocalizedString localizedString, bool stripMetadata = false)
        {
            if (!Merge (localizedString, stripMetadata: stripMetadata)) {
                if (stripMetadata) {
                    localizedString.StripMetadata ();
                }
                strings.Add (localizedString);
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
        }

        protected IEnumerable<ResourceString> GetResourceStrings (LocalizedString localizedString)
        {
            var translated = localizedString.TranslatedValues;
            if (translated == null) {
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

                yield return new ResourceString {
                    Id = Catalog.GetResourceId (untranslated, localizedString.Gender, i),
                    Untranslated = untranslated,
                    Translated = translated [i]
                };
            }
        }

        protected void MarkResourceStringAsGenerated (ResourceString resourceString)
        {
            generated_resource_string_ids.Add (resourceString.Id);
        }

        protected bool HasResourceStringBeenGenerated (ResourceString resourceString)
        {
            return generated_resource_string_ids.Contains (resourceString.Id);
        }

        protected abstract void Generate ();

        public void Reduce (Parser masterSet, Parser subSet)
        {
            foreach (var localized_string in subSet.Parse ()) {
                Add (localized_string, stripMetadata: true);
            }

            foreach (var localized_string in masterSet.Parse ()) {
                Merge (localized_string, stripMetadata: true);
            }
        }
    }
}
