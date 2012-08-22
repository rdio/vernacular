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
            get { return Encoding.UTF8; }
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
            private Encoding encoding;
            public override Encoding Encoding {
                get { return encoding; }
            }

            public GeneratorWriter (Stream stream, Encoding encoding)
                : base (stream) {
                this.encoding = encoding;
                NewLine = "\n";
            }
        }
    }
}
