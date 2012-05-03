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
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public sealed class PoParser : Parser
    {
        private class PoMessage
        {
            public string Identifier;
            public string Value;
        }

        private class PoUnit
        {
            public List<PoLexer.Token.Comment> Comments = new List<PoLexer.Token.Comment> ();
            public List<PoMessage> Messages = new List<PoMessage> ();
        }

        private List<string> po_paths = new List<string> ();

        private LocalizationMetadata header;

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

        private IEnumerable<PoUnit> ParseAllTokenStreams ()
        {
            foreach (var path in po_paths) {
                var unit = new PoUnit ();
                PoMessage message = null;

                using (var reader = new StreamReader (path)) {
                    var lexer = new PoLexer {
                        Path = path,
                        Reader = reader
                    };

                    foreach (var token in lexer.Lex ()) {
                        if (token is PoLexer.Token.Comment) {
                            unit.Comments.Add ((PoLexer.Token.Comment)token);
                        } else if (token is PoLexer.Token.Identifier) {
                            message = new PoMessage { Identifier = (string)token };
                            unit.Messages.Add (message);
                        } else if (token is PoLexer.Token.String) {
                            message.Value += (string)token;
                        } else if (token is PoLexer.Token.EndOfUnit) {
                            yield return unit;
                            unit = new PoUnit ();
                        }
                    }
                }

                yield return unit;
            }
        }

        private ILocalizationUnit ParsePoUnit (PoUnit unit)
        {
            if (header == null) {
                header = ParsePoHeaderUnit (unit);
                if (header != null) {
                    return header;
                }
            }

            return ParsePoMessageUnit (unit);
        }

        private LocalizationMetadata ParsePoHeaderUnit (PoUnit unit)
        {
            var keys = new [] {
                "project-id-version:",
                "language:",
                "content-type:"
            };

            if (unit == null || unit.Messages == null || unit.Messages.Count != 2 ||
                unit.Messages[0].Identifier != "msgid" ||
                !String.IsNullOrEmpty (unit.Messages[0].Value) ||
                unit.Messages[1].Identifier != "msgstr") {
                return null;
            }

            var header_lower = unit.Messages[1].Value.ToLower ();

            foreach (var key in keys) {
                if (!header_lower.Contains (key)) {
                    return null;
                }
            }

            LocalizationMetadata metadata = null;

            foreach (var line in unit.Messages[1].Value.Split ('\n')) {
                if (String.IsNullOrWhiteSpace (line)) {
                    continue;
                }

                var pairs = line.Split (new [] { ':' }, 2);
                if (pairs == null || pairs.Length != 2) {
                    return null;
                }

                if (metadata == null) {
                    metadata = new LocalizationMetadata ();
                }

                metadata.Add (pairs[0].Trim (), pairs[1].Trim ());
            }

            return metadata;
        }

        private LocalizedString ParsePoMessageUnit (PoUnit unit)
        {
            var developer_comments_builder = new StringBuilder ();
            var translator_comments_builder = new StringBuilder ();
            var references_builder = new StringBuilder ();
            var flags_builder = new StringBuilder ();
            var translated_values = new List<string> ();
            string untranslated_singular_value = null;
            string untranslated_plural_value = null;

            foreach (var message in unit.Messages) {
                var match = Regex.Match (message.Identifier, @"^msg(id|id_plural|str|str\[(\d+)\]|ctx)$");
                if (!match.Success) {
                    continue;
                }

                switch (match.Groups[1].Value) {
                    case "id": untranslated_singular_value = message.Value; break;
                    case "id_plural": untranslated_plural_value = message.Value; break;
                    case "str": translated_values.Insert (0, message.Value); break;
                    case "ctx": break;
                    default:
                        if (match.Groups.Count == 3) {
                            translated_values.Insert (Int32.Parse (match.Groups[2].Value), message.Value);
                        }
                        break;
                }
            }

            foreach (var comment in unit.Comments) {
                switch (comment.Type) {
                    case PoLexer.CommentType.Extracted:
                        developer_comments_builder.Append (comment.Value.Trim ());
                        developer_comments_builder.Append ('\n');
                        break;
                    case PoLexer.CommentType.Translator:
                        translator_comments_builder.Append (comment.Value.Trim ());
                        translator_comments_builder.Append ('\n');
                        break;
                    case PoLexer.CommentType.Reference:
                        references_builder.Append (comment.Value.Trim ());
                        references_builder.Append (' ');
                        break;
                    case PoLexer.CommentType.Flag:
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
                localized_string.DeveloperComments = translator_comments;
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

            localized_string.UntranslatedSingularValue = untranslated_singular_value;
            localized_string.UntranslatedPluralValue = untranslated_plural_value;

            if (translated_values.Count > 0) {
                localized_string.TranslatedValues = translated_values.ToArray ();
            }

            return localized_string;
        }

        public override IEnumerable<ILocalizationUnit> Parse ()
        {
            header = null;

            foreach (var unit in ParseAllTokenStreams ()) {
                yield return ParsePoUnit (unit);
            }
        }
    }
}
