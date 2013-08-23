//
// ResxGenerator.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vernacular.Generators
{
    public abstract class StreamGenerator : Generator
    {
        protected TextWriter Writer { get; private set; }

        protected virtual Encoding Encoding {
            get { return new UTF8Encoding(false); }
        }

        public override void Generate (string path) 
        {
            if (String.IsNullOrEmpty (path) || path == "-") {
                Writer = Console.Out;
            } else {
                Writer = new GeneratorWriter (File.Create (path), Encoding);
            }

            try {
                Generate ();
            }
            finally {
                Writer.Close ();
                Writer = null;
            }
        }

        protected sealed class GeneratorWriter : StreamWriter
        {
            public GeneratorWriter (Stream stream, Encoding encoding)
                : base (stream, encoding) {
                NewLine = "\n";
            }
        }
    }
}
