//
// AggregateParser.cs
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
using System.Linq;
using System.Collections.Generic;

using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public sealed class AggregateParser : Parser
    {
        private List<Parser> parsers = new List<Parser> {
            new AssemblyParser (),
            new XamlParser (),
            new PoParser (),
            new AndroidResourceParser ()
        };

        public override IEnumerable<string> SupportedFileExtensions {
            get {
                return from parser in parsers
                       from ext in parser.SupportedFileExtensions
                       select ext;
            }
        }

        public override int LogLevel {
            get { return parsers.First ().LogLevel; }
            set {
                foreach (var parser in parsers) {
                    parser.LogLevel = value;
                }
            }
        }

        public override string SourceRootPath {
            get { return parsers.First ().SourceRootPath; }
            set {
                foreach (var parser in parsers) {
                    parser.SourceRootPath = value;
                }
            }
        }

        public override void Add (string path)
        {
            foreach (var parser_for_path in
                from parser in parsers
                from ext in parser.SupportedFileExtensions
                where ext == Path.GetExtension (path)
                select parser) {
                parser_for_path.Add (path);
            }
        }

        public override IEnumerable<LocalizedString> Parse ()
        {
            return from parser in parsers
                   from localized_string in parser.Parse ()
                   select localized_string;
        }
    }
}
