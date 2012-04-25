using System;
using System.Collections.Generic;

using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public abstract class Parser
    {
        public virtual int LogLevel { get; set; }
        public virtual string SourceRootPath { get; set; }

        public abstract IEnumerable<string> SupportedFileExtensions { get; }
        public abstract void Add (string path);
        public abstract IEnumerable<LocalizedString> Parse ();

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