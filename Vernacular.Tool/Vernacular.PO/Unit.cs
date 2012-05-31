// 
// Unit.cs
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

namespace Vernacular.PO
{
    public sealed class Unit
    {
        private List<Token.Comment> comments = new List<Token.Comment> ();
        private List<Message> messages = new List<Message> ();

        public IList<Token.Comment> Comments {
            get { return comments; }
        }

        public IList<Message> Messages {
            get { return messages; }
        }

        internal Unit ()
        {
        }

        public override string ToString ()
        {
            var builder = new StringBuilder ();

            foreach (var comment in Comments) {
                builder.AppendFormat ("#{0} {1}\n", comment.TypeChar, comment.Value);
            }

            foreach (var message in Messages) {
                builder.Append (message);
                builder.Append ("\n");
            }

            return builder.ToString ();
        }
    }
}