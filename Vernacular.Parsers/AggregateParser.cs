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
