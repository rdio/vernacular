//
// XapParser.cs
//
// Author:
//   Stephane Delcroix <stephane@delcroix.org>
//
// Copyright 2012 Stephane Delcroix
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
using ICSharpCode.SharpZipLib.Zip;
using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public sealed class XapParser : Parser
    {
        private readonly Parser parser_for_parts;

        public XapParser (Parser parserForParts)
        {
            parser_for_parts = parserForParts;
        }

        public override IEnumerable<string> SupportedFileExtensions
        {
            get { yield return ".xap"; }
        }

        public override void Add (string path)
        {
            using (var file_stream = new FileStream (path, FileMode.Open, FileAccess.Read))
            using (var zip_stream = new ZipInputStream (file_stream)) {                
                ZipEntry entry;
                while ((entry = zip_stream.GetNextEntry ()) != null) {
                    if (!entry.IsFile) {
                        continue;
                    }
                    var extension = Path.GetExtension (entry.Name);
                    if (!parser_for_parts.SupportedFileExtensions.Contains (extension)) {
                        continue;
                    }

                    using (var out_stream = new MemoryStream ()) {
                        int size;
                        var buffer = new byte[2048];
                        do {
                            size = zip_stream.Read (buffer, 0, buffer.Length);
                            out_stream.Write (buffer, 0, size);
                        } while (size > 0);

                        out_stream.Seek (0, SeekOrigin.Begin);
                        try {
                            parser_for_parts.Add (out_stream, entry.Name);
                        } catch (NotSupportedException) {
                            
                        }
                    }
                }                
            }
        }

        public override void Add (Stream stream, string path)
        {
            throw new NotSupportedException ();
        }

        public override IEnumerable<ILocalizationUnit> Parse ()
        {
           yield break;
        }
    }
}
