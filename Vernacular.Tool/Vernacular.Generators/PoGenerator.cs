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
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Vernacular.Tool;
using Vernacular.Parsers;
using Vernacular.Potato;

namespace Vernacular.Generators
{
    public sealed class PoGenerator : StreamGenerator
    {
        public bool PotMode { get; set; }
        public bool ExcludeHeaderMetadata { get; set; }

        protected override void Generate ()
        {
            var document = new Document ();

            if (!ExcludeHeaderMetadata) {
                if (LocalizationMetadata != null) {
                    foreach (string header in LocalizationMetadata) {
                        document.Headers [header] = LocalizationMetadata [header];
                    }
                }

                document.Headers.PopulateWithRequiredHeaders ();
                document.Headers ["X-Generator"] = "Vernacular";
            }

            var sorted_strings = from localized_string in Strings
                                 orderby localized_string.UntranslatedSingularValue
                                 orderby localized_string.UntranslatedPluralValue
                                 select localized_string;

            foreach (var localized_string in RetainStringOrder ? Strings : sorted_strings) {
                var unit = new Unit {
                    new Comment {
                        Type = CommentType.Translator,
                        Value = localized_string.TranslatorComments
                    },

                    new Comment {
                        Type = CommentType.Extracted,
                        Value = localized_string.DeveloperComments
                    }
                };

                if (localized_string.References != null) {
                    foreach (var reference in localized_string.References) {
                        unit.Add (new Comment {
                            Type = CommentType.Reference,
                            Value = reference
                        });
                    }
                }

                unit.Add (new Comment {
                    Type = CommentType.Flag,
                    Value = localized_string.StringFormatHint
                });

                var context = localized_string.Context;
                var singular = localized_string.UntranslatedSingularValue;
                var plural = localized_string.UntranslatedPluralValue;

                if (localized_string.Gender != LanguageGender.Neutral) {
                    var add_gender_context = true;
                    if (context != null) {
                        var context_lower = context.ToLower ();
                        foreach (var gender_context in PoParser.GenderContexts) {
                            if (context_lower.Contains (gender_context.Key)) {
                                add_gender_context = false;
                                break;
                            }
                        }
                    }

                    if (add_gender_context) {
                        var gender_context = localized_string.Gender.ToString () + " form";
                        if (String.IsNullOrEmpty (context)) {
                            context = gender_context;
                        } else {
                            context += ", " + gender_context;
                        }
                    }
                }

                unit.Add (
                    new Message {
                        Type = MessageType.Context,
                        Value = context
                    },

                    new Message {
                        Type = MessageType.SingularIdentifier,
                        Value = singular
                    },

                    new Message {
                        Type = MessageType.PluralIdentifier,
                        Value = plural
                    }
                );

                var translated = localized_string.TranslatedValues;
                if (translated == null) {
                    if (plural == null) {
                        translated = new [] { singular };
                    } else {
                        translated = new [] { singular, plural };
                    }
                }

                for (int i = 0; i < translated.Length; i++) {
                    unit.Add (new Message {
                        Type = translated.Length == 1 ? MessageType.SingularString : MessageType.PluralString,
                        PluralOrder = i,
                        Value = PotMode ? String.Empty : translated [i]
                    });
                }

                document.Add (unit);
            }

            Writer.Write (document.Generate ());
        }
    }
}
