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
using System.Text;
using System.Collections.Generic;

using Vernacular.Potato.Internal;

namespace Vernacular.Potato
{
    public sealed class Message : UnitChild
    {
        private MessageType type;
        public MessageType Type {
            get { return type; }
            set {
                if (type != value) {
                    type = value;
                    NotifyPropertyChanged ("Type");
                }
            }
        }

        private int plural_order;
        public int PluralOrder {
            get { return plural_order; }
            set {
                if (plural_order != value) {
                    plural_order = value;
                    NotifyPropertyChanged ("PluralOrder");
                }
            }
        }

        public override bool HasValue {
            get { return Value != null; }
        }

        public Message ()
        {
        }

        internal Message (Token token, MessageType type, int pluralOrder)
        {
            Line = token.Line;
            Column = token.Column;
            Type = type;
            PluralOrder = pluralOrder;
        }

        private static Dictionary<string, string> escapes = new Dictionary<string, string> {
            {"\\", @"\\"},
            {"\"", @"\"""},
            {"\a", @"\a"},
            {"\b", @"\b"},
            {"\f", @"\f"},
            {"\n", @"\n"},
            {"\r", @"\r"},
            {"\t", @"\t"},
            {"\v", @"\v"},
            {"\0", @"\0"}
        };

        private static string Escape (string value)
        {
            if (String.IsNullOrWhiteSpace (value)) {
                return value;
            }

            foreach (var escape in escapes) {
                value = value.Replace (escape.Key, escape.Value);
            }

            return value;
        }

        public override string Generate ()
        {
            if (!HasValue) {
                return null;
            }

            var builder = new StringBuilder ();

            switch (Type) {
                case MessageType.Context:
                    builder.Append ("msgctxt");
                    break;
                case MessageType.SingularIdentifier:
                    builder.Append ("msgid");
                    break;
                case MessageType.PluralIdentifier:
                    builder.Append ("msgid_plural");
                    break;
                case MessageType.SingularString:
                    builder.Append ("msgstr");
                    break;
                case MessageType.PluralString:
                    builder.Append ("msgstr[");
                    builder.Append (PluralOrder);
                    builder.Append (']');
                    break;
                default:
                    throw new Exception ("Invalid MessageType");
            }

            builder.Append (' ');

            var lines = Value.Split ('\n');
            if (lines.Length > 1) {
                builder.Append ("\"\"\n");
                foreach (var line in lines) {
                    if (!String.IsNullOrEmpty (line)) {
                        builder.Append ('"');
                        builder.Append (Escape (line));
                        builder.Append ("\\n\"");
                        builder.Append ('\n');
                    }
                }
                builder.Length--;
            } else {
                builder.Append ('"');
                builder.Append (Escape (lines [0]));
                builder.Append ('"');
            }

            return builder.ToString ();
        }
    }
}