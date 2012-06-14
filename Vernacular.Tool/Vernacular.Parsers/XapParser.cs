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
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public sealed class XapParser : Parser
    {
        IList<Parser> subParsers;

        public XapParser(IList<Parser> subParsers )
        {
            this.subParsers = subParsers;
        }

        public override IEnumerable<string> SupportedFileExtensions
        {
            get { yield return ".xap"; }
        }

        public override void Add(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var zipStream = new ZipInputStream(fileStream);
            ZipEntry entry;
            while ((entry = zipStream.GetNextEntry()) != null)
            {
                if (!entry.IsFile)
                    continue;
                foreach (var parser_for_path in
                    from parser in subParsers
                    from ext in parser.SupportedFileExtensions
                    where ext == Path.GetExtension(entry.Name)
                    select parser)
                {
                    using (var outStream = new MemoryStream())
                    {

                        int size;
                        var buffer = new byte[2048];
                        do
                        {
                            size = zipStream.Read(buffer, 0, buffer.Length);
                            outStream.Write(buffer, 0, size);
                        } while (size > 0);

                        outStream.Seek(0, SeekOrigin.Begin);
                        parser_for_path.Add(outStream);
                    }
                }
            }
            zipStream.Close();
            fileStream.Close();
        }

        public override void Add(Stream stream)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<ILocalizationUnit> Parse()
        {
           yield break;
        }
    }
}
