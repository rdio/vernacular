using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vernacular.Generators
{
    public abstract class BinaryGenerator : Generator
    {
        protected BinaryWriter Writer { get; private set; }

        protected virtual Encoding Encoding {
            get { return Encoding.ASCII; }
        }

        public override void Generate (string path) {
            Writer = new BinaryWriter(File.Create (path), Encoding);

            try {
                Generate ();
            }
            finally {
                Writer.Close ();
                Writer = null;
            }
        }
    }
}
