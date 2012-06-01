// 
// Token.cs
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

namespace Vernacular.PO
{
    public abstract class Token
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Value { get; set; }

        internal Token ()
        {
        }

        public static explicit operator string (Token token)
        {
            return token.Value;
        }

        public override string ToString ()
        {
            return string.Format ("[{0},{1}] {2} = \"{3}\"", Line, Column,
                GetType ().Name, Value.Replace("\"", "\\\""));
        }

        public sealed class String : Token { }
        public sealed class Identifier : Token { }

        public sealed class Comment : Token
        {
            public char TypeChar { get; set; }

            public override string ToString ()
            {
                return string.Format ("{0} [TypeChar='{1}']", base.ToString (), TypeChar);
            }
        }
    }
}