//
// ResxParser.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.ComponentModel.Design;

using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public sealed class ResxParser : Parser
    {
        private List<string> resx_paths = new List<string>();

        public override IEnumerable<string> SupportedFileExtensions {
            get {
                yield return ".resx";
            }
        }

        public override void Add(string path)
        {
            resx_paths.Add(path);
        }

        public override void Add(Stream stream, string path)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<ILocalizationUnit> Parse ()
        {
            foreach (var path in resx_paths) {
                var reader = new ResXResourceReader(path) { UseResXDataNodes = true };

                foreach (DictionaryEntry item in reader) {
                    var name = (string)item.Key;
                    var node = (ResXDataNode)item.Value;
                    yield return new LocalizedString {
                        Name = name,
                        DeveloperComments = node.Comment,
                        UntranslatedSingularValue = (string)node.GetValue(null as ITypeResolutionService)
                    };
                }
            }

            yield break;
        }
    }
}