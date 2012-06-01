// 
// Comment.cs
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

using Vernacular.PO.Internal;

namespace Vernacular.PO
{
    public class Comment : IDocumentPart
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public CommentType Type { get; set; }
        public string Value { get; set; }

        public bool HasValue {
            get { return !String.IsNullOrWhiteSpace (Value); }
        }

        public Comment ()
        {
        }

        internal Comment (Token.Comment comment)
        {
            Line = comment.Line;
            Column = comment.Column;
            Type = GetCommentType (comment.TypeChar);
            Value = comment.Value;
        }

        public string Generate ()
        {
            if (!HasValue) {
                return null;
            }

            var builder = new StringBuilder ();

            foreach (var line in Value.Split ('\n')) {
                builder.Append ('#');
                builder.Append (GetCommentChar (Type));
                builder.Append (' ');
                builder.Append (line);
                builder.Append ('\n');
            }

            builder.Length--;

            return builder.ToString ();
        }

        internal static char GetCommentChar (CommentType type)
        {
            switch (type) {
                case CommentType.Extracted:
                    return '.';
                case CommentType.Reference:
                    return ':';
                case CommentType.Flag:
                    return ',';
                case CommentType.PreviousContext:
                    return '|';
                case CommentType.ObsoleteMessage:
                    return '~';
                case CommentType.Translator:
                    return ' ';
                default:
                    throw new Exception ("Invalid CommentType");
            }
        }

        internal static CommentType GetCommentType (char type)
        {
            switch (type) {
                case '.':
                    return CommentType.Extracted;
                case ':':
                    return CommentType.Reference;
                case ',':
                    return CommentType.Flag;
                case '|':
                    return CommentType.PreviousContext;
                case '~':
                    return CommentType.ObsoleteMessage;
                case ' ':
                default:
                    return CommentType.Translator;
            }
        }
    }
}