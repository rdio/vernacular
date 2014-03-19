//
// PoParser.cs
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
using System.Text;
using System.Collections.Generic;

using Vernacular.Tool;
using Vernacular.Potato;

namespace Vernacular.Parsers
{
    public sealed class PoParser : Parser
    {
        public static readonly Dictionary<string, LanguageGender> GenderContexts = new Dictionary<string, LanguageGender> {
            { "masculine form", LanguageGender.Masculine },
            { "feminine form", LanguageGender.Feminine },
            { "gender-masculine", LanguageGender.Masculine },
            { "gender-feminine", LanguageGender.Feminine }
        };

        private List<string> po_paths = new List<string> ();

        public override IEnumerable<string> SupportedFileExtensions {
            get {
                yield return ".po";
                yield return ".pot";
            }
        }

        public override void Add (string path)
        {
            po_paths.Add (path);
        }

        public override void Add (Stream stream, string path)
        {
            throw new NotSupportedException ();
        }

        private ILocalizationUnit Parse (IDocumentPart part)
        {
            var headers = part as HeaderCollection;
            var unit = part as Unit;

            if (headers != null) {
                var metadata = new LocalizationMetadata ();
                foreach (var header in headers) {
                    metadata.Add (header.Name, header.Value);
                }
                return metadata;
            } else if (unit != null) {
                return ParsePoMessageUnit (unit);
            } else {
                return null;
            }
        }

        private LocalizedString ParsePoMessageUnit (Unit unit)
        {
            var developer_comments_builder = new StringBuilder ();
            var translator_comments_builder = new StringBuilder ();
            var references_builder = new StringBuilder ();
            var flags_builder = new StringBuilder ();
            var translated_values = new List<string> ();
            string untranslated_singular_value = null;
            string untranslated_plural_value = null;
            string context = null;

            foreach (var message in unit.Messages) {
                switch (message.Type) {
                    case MessageType.SingularIdentifier:
                        untranslated_singular_value = message.Value;
                        break;
                    case MessageType.PluralIdentifier:
                        untranslated_plural_value = message.Value;
                        break;
                    case MessageType.SingularString:
                    case MessageType.PluralString:
                        translated_values.Insert (message.PluralOrder, message.Value);
                        break;
                    case MessageType.Context:
                        context = message.Value.Trim ();
                        break;
                }
            }

            foreach (var comment in unit.Comments) {
                switch (comment.Type) {
                    case CommentType.Extracted:
                        developer_comments_builder.Append (comment.Value.Trim ());
                        developer_comments_builder.Append ('\n');
                        break;
                    case CommentType.Translator:
                        translator_comments_builder.Append (comment.Value.Trim ());
                        translator_comments_builder.Append ('\n');
                        break;
                    case CommentType.Reference:
                        references_builder.Append (comment.Value.Trim ());
                        references_builder.Append (' ');
                        break;
                    case CommentType.Flag:
                        flags_builder.Append (comment.Value.Trim ());
                        flags_builder.Append (',');
                        break;
                }
            }

            var developer_comments = developer_comments_builder.ToString ().Trim ();
            var translator_comments = translator_comments_builder.ToString ().Trim ();
            var references = references_builder.ToString ().Trim ();
            var flags = flags_builder.ToString ().Trim ();

            var localized_string = new LocalizedString ();

            if (!String.IsNullOrWhiteSpace (developer_comments)) {
                localized_string.DeveloperComments = developer_comments;
            }

            if (!String.IsNullOrWhiteSpace (translator_comments)) {
                localized_string.TranslatorComments = translator_comments;
            }

            if (!String.IsNullOrWhiteSpace (references)) {
                localized_string.References = references.Split (' ');
            }

            if (!String.IsNullOrWhiteSpace (flags)) {
                foreach (var flag in flags.Split (',')) {
                    if (flag.EndsWith ("-format")) {
                        localized_string.StringFormatHint = flag;
                    }
                }
            }

            if (context != null) {
                var context_lower = context.ToLower ();
                foreach (var gender_context in GenderContexts) {
                    if (context_lower == gender_context.Key) {
                        localized_string.Gender = gender_context.Value;
                        context = null;
                        break;
                    } else if (context.Contains (gender_context.Key)) {
                        localized_string.Gender = gender_context.Value;
                        break;
                    }
                }
            }

            localized_string.Context = context;
            localized_string.UntranslatedSingularValue = untranslated_singular_value;
            localized_string.UntranslatedPluralValue = untranslated_plural_value;

            if (translated_values.Count > 0) {
                localized_string.TranslatedValues = translated_values.ToArray ();
            }

            return localized_string;
        }

        public override IEnumerable<ILocalizationUnit> Parse ()
        {
            foreach (var path in po_paths) {
                var parser = new Vernacular.Potato.Internal.Parser ();
                foreach (var part in parser.Parse (path)) {
                    var unit = Parse (part);
                    if (unit is LocalizationMetadata && Path.GetExtension (path) == ".pot") {
                        continue;
                    } else if (unit != null) {
                        yield return unit;
                    }
                }
            }
        }
    }
}
