//
// ResourceString.cs
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Vernacular
{
    public struct ResourceString
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

        public static IEnumerable<ResourceString> Generate (ResourceIdType resourceIdType, LocalizedString localizedString)
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
                if (translated [i] == null ||
                    (i == 0 && String.IsNullOrWhiteSpace (localizedString.UntranslatedSingularValue))) {
                    continue;
                }

                yield return new ResourceString {
                    Id = Catalog.GetResourceId (resourceIdType,
                        localizedString.Context, localizedString.UntranslatedSingularValue,
                        localizedString.Gender, i),
                    Untranslated = localizedString.UntranslatedSingularValue,
                    Translated = translated [i]
                };
            }
        }
    }
}