//
// Parser.cs
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
using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public abstract class Parser
    {
        public virtual int LogLevel { get; set; }
        public virtual string SourceRootPath { get; set; }

        public abstract IEnumerable<string> SupportedFileExtensions { get; }
        public abstract void Add (string path);
        public abstract void Add (Stream stream, string path);
        public abstract IEnumerable<ILocalizationUnit> Parse ();

        public LocalizationMetadata LocalizationMetadata { get; private set; }

        protected Parser ()
        {
            LocalizationMetadata = new LocalizationMetadata ();
        }

        protected void Log (object message = null)
        {
            Log (false, "{0}", message);
        }

        protected void Log (string messageFormat, params object [] args)
        {
            Log (false, messageFormat, args);
        }

        protected void Log (bool verbose, string messageFormat, params object [] args)
        {
            if (LogLevel == 2 || (LogLevel == 1 && !verbose)) {
                Console.WriteLine (messageFormat, args);
            }
        }

        protected string RelativeDocumentUrl (string url)
        {
            if (SourceRootPath != null && url.StartsWith (SourceRootPath,
                StringComparison.InvariantCultureIgnoreCase)) {
                url = url.Substring (SourceRootPath.Length);
            }

            url = url.Replace ('\\', '/');

            if (url.StartsWith ("/")) {
                url = url.Substring (1);
            }

            return url;
        }
    }
}
