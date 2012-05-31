//
// PoLexer.cs
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

namespace Vernacular.Parsers
{
    public sealed class PoLexer
    {
        public enum CommentType
        {
            Translator,
            Extracted,
            Reference,
            Flag,
            PreviousContext,
            ObsoleteMessage
        }

        public abstract class Token
        {
            public int Line { get; set; }
            public int Column { get; set; }
            public string Value { get; set; }

            public static explicit operator string (Token token)
            {
                return token.Value;
            }

            public override string ToString ()
            {
                return string.Format ("[{0},{1}] {2} = \"{3}\"", Line, Column, GetType ().Name, Value);
            }

            public sealed class String : Token { }
            public sealed class Identifier : Token { }

            public sealed class Comment : Token
            {
                public CommentType Type { get; set; }
                public char TypeChar { get; set; }

                public override string ToString ()
                {
                    return string.Format ("{0} ({1})", base.ToString (), Type);
                }
            }
        }

        private class SyntaxException :Exception
        {
            public SyntaxException (PoLexer lexer, string message)
                : base (String.Format ("Syntax error: \"{0}\": {1}:{2},{3} ('{4}')",
                    message, lexer.Path, lexer.Line, lexer.Column, lexer.Peek ()))
            {
            }
        }

        public string Path { get; set; }
        public StreamReader Reader { get; set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        private StringBuilder builder = new StringBuilder ();

        public PoLexer ()
        {
            Line = 1;
            Column = 1;
        }

        private void PushString (char c)
        {
            builder.Append (c);
        }

        private string PopString ()
        {
            var s = builder.ToString ();
            builder.Clear ();
            return s;
        }

        private char Peek ()
        {
            return (char)Reader.Peek ();
        }

        private char Read ()
        {
            var c = (char)Reader.Read ();

            if (c == '\n') {
                Line++;
                Column = 1;
            } else {
                Column++;
            }

            return c;
        }

        private void EatWhitespace ()
        {
            while (!Reader.EndOfStream && Char.IsWhiteSpace (Peek ())) {
                Read ();
            }
        }

        private bool IsIdentifier (char c)
        {
            return Char.IsLetterOrDigit (c) || c == '_' || c == '[' || c == ']';
        }

        private Token Annotate (int line, int column, Token token)
        {
            token.Line = line;
            token.Column = column;
            return token;
        }

        public IEnumerable<Token> Lex ()
        {
            while (!Reader.EndOfStream) {
                EatWhitespace ();
                var c = Peek ();
                if (c == '#') {
                    yield return Annotate (Line, Column, LexComment ());
                } else if (c == '"') {
                    yield return Annotate (Line, Column, LexMessage ());
                } else if (IsIdentifier (c)) {
                    yield return Annotate (Line, Column, LexIdentifier ());
                } else if (c == Char.MaxValue) {
                    break;
                } else {
                    throw new SyntaxException (this, "Expected whitespace, comment, identifier, or string");
                }
            }
        }

        private Token.Comment LexComment ()
        {
            var comment = new Token.Comment ();

            // Eat the '#'
            Read ();

            // Determine the comment type
            var c = Peek ();
            switch (c) {
                case '.': comment.Type = CommentType.Extracted; break;
                case ':': comment.Type = CommentType.Reference; break;
                case ',': comment.Type = CommentType.Flag; break;
                case '|': comment.Type = CommentType.PreviousContext; break;
                case '~': comment.Type = CommentType.ObsoleteMessage; break;
                default: comment.Type = CommentType.Translator; break;
            }

            comment.TypeChar = c;

            // Eat the one of four comment type characters from above
            if (comment.Type != CommentType.Translator) {
                Read ();
            }

            // Eat whitespace and then start accumulating the comment value
            EatWhitespace ();

            while ((c = Read ()) != '\n') {
                PushString (c);
            }

            comment.Value = PopString ();
            return comment;
        }

        private Token.String LexMessage ()
        {
            // Eat the '"'
            Read ();

            while (true) {
                var c = Read ();
                switch (c) {
                    case '\n':
                    case '\r':
                        throw new SyntaxException (this, "New lines are not allowed in strings");
                    case '\\':
                        switch (Read ()) {
                            case '\\': PushString ('\\'); break;
                            case '"': PushString ('"'); break;
                            case 'a': PushString ('\a'); break;
                            case 'f': PushString ('\f'); break;
                            case 'n': PushString ('\n'); break;
                            case 'r': PushString ('\r'); break;
                            case 'v': PushString ('\v'); break;
                            case '0': PushString ('\0'); break;
                            default: throw new SyntaxException (this, "Invalid string escape sequence");
                        }
                        break;
                    case '"':
                        return new Token.String { Value = PopString () };
                    default:
                        PushString (c);
                        break;
                }
            }
        }

        private Token.Identifier LexIdentifier ()
        {
            while (true) {
                var c = Read ();
                if (IsIdentifier (c)) {
                    PushString (c);
                } else if (Char.IsWhiteSpace (c)) {
                    return new Token.Identifier { Value = PopString () };
                } else {
                    throw new SyntaxException (this, "Invalid identifier syntax");
                }
            }
        }
    }
}
