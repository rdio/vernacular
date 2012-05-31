// 
// Message.cs
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
using System.Text.RegularExpressions;

namespace Vernacular.PO
{
    public sealed class Message : IDocumentPart
    {
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Value { get; private set; }
        public MessageType Type { get; private set; }
        public int PluralOrder { get; private set; }

        internal Message (Token token, MessageType type, int pluralOrder)
        {
            Line = token.Line;
            Column = token.Column;
            Type = type;
            PluralOrder = pluralOrder;
        }

        public void AppendValue (Token token)
        {
            Value += (string)token;
        }

        public override string ToString ()
        {
            string id = null;

            switch (Type) {
                case MessageType.Context:
                    id = "msgctxt";
                    break;
                case MessageType.SingularIdentifier:
                    id = "msgid";
                    break;
                case MessageType.PluralIdentifier:
                    id = "msgid_plural";
                    break;
                case MessageType.SingularString:
                    id = "msgstr";
                    break;
                case MessageType.PluralString:
                    id = "msgstr[" + PluralOrder.ToString () + "]";
                    break;
            }

            return String.Format ("{0} \"{1}\"", id, Value.Replace ("\"", "\\\""));
        }
    }
}