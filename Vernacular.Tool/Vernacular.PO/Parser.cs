//
// Parser.cs
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

namespace Vernacular.PO
{
    public sealed class Parser
    {
        private class SyntaxException :Exception
        {
            public SyntaxException (Lexer lexer, string message)
                : base (String.Format ("Syntax error: \"{0}\": {1}:{2},{3}",
                    message, lexer.Path, lexer.Line, lexer.Column))
            {
            }
        }

        private bool IsStartOfUnitToken (Token token)
        {
            return token is Token.Comment ||
                (token is Token.Identifier &&
                    (token.Value == "msgctxt" || token.Value == "msgid"));
        }

        private bool IsMsgstrToken (Token token)
        {
            return token is Token.Identifier && token.Value.StartsWith ("msgstr");
        }

        public IEnumerable<Unit> Parse (string path)
        {
            using (var reader = new StreamReader(path)) {
                foreach (var unit in Parse (reader, path)) {
                    yield return unit;
                }
            }
        }

        public IEnumerable<Unit> Parse (StreamReader reader, string pathHint = null)
        {
            return Parse (new Lexer { Path = pathHint, Reader = reader });
        }

        public IEnumerable<Unit> Parse (Lexer lexer)
        {
            var unit = new Unit ();
            Token last_msgstr_token = null;
            Message message = null;

            foreach (var token in lexer.Lex ()) {
                if (IsMsgstrToken (token)) {
                    last_msgstr_token = token;
                } else if (IsStartOfUnitToken (token) && last_msgstr_token != null) {
                    last_msgstr_token = null;
                    yield return unit;
                    unit = new Unit ();
                }

                if (token is Token.Comment) {
                    unit.Comments.Add ((Token.Comment)token);
                } else if (token is Token.Identifier) {
                    message = ParseIdentifier (lexer, (Token.Identifier)token);
                    unit.Messages.Add (message);
                } else if (token is Token.String) {
                    message.AppendValue (token);
                }
            }

            yield return unit;
        }

        private Message ParseIdentifier (Lexer lexer, Token.Identifier identifier)
        {
            var match = Regex.Match ((string)identifier, @"^msg(id|id_plural|str|str\[(\d+)\]|ctxt)$");
            if (!match.Success) {
                throw new SyntaxException (lexer, "invalid identifier: " + (string)identifier);
            }

            int plural_order;
            MessageType type;

            switch (match.Groups [1].Value) {
                case "id":
                    type = MessageType.SingularIdentifier;
                    plural_order = -1;
                    break;
                case "id_plural":
                    type = MessageType.PluralIdentifier;
                    plural_order = -1;
                    break;
                case "str":
                    type = MessageType.SingularString;
                    plural_order = 0;
                    break;
                case "ctxt":
                    type = MessageType.Context;
                    plural_order = -1;
                    break;
                default:
                    if (match.Groups.Count == 3) {
                        type = MessageType.PluralString;
                        plural_order = Int32.Parse (match.Groups [2].Value);
                    } else {
                        throw new SyntaxException(lexer, "invalid identifier: " + (string)identifier);
                    }
                    break;
            }

            return new Message (identifier, type, plural_order);
        }
    }
}