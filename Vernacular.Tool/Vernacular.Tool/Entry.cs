//
// Entry.cs
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
using System.Collections.Generic;

using Mono.Options;

using Vernacular.Parsers;
using Vernacular.Analyzers;
using Vernacular.Generators;

namespace Vernacular.Tool
{
    public static class Entry
    {
        public static int Main (string[] args)
        {
            var input_paths = new List<string> ();
            string output_path = "-";
            string generator_name = String.Empty;
            string source_root_path = null;
            string reduce_master_path = null;
            string reduce_retain_path = null;
            string android_input_strings_xml = null;
            string android_output_strings_xml = null;
            string analyer_config_path = null;
            string initWithLocale = null;
            LocalizationMetadata metadata = null;
            bool generate_pot = false;
            bool exclude_po_header = false;
            bool analyze = false;
            bool analyzer_warn_as_error = false;
            bool log = false;
            bool verbose = false;
            bool retain_order = false;
            bool show_help = false;

            Generator generator = null;

            var options = new OptionSet {
                { "i|input=", "Input directory, search pattern, or file to parse (non-recursive)", v => input_paths.Add (v) },
                { "o|output=", "Output file for extracted string resources", v => output_path = v },
                { "r|source-root=", "Root directory of source code", v => source_root_path = v },
                { "g|generator=", String.Format ("Generator to use ({0})",
                    String.Join ("|", Generator.GeneratorNames)), v => generator_name = v },
                { "retain-order", "Retain the original input string order when generating. " +
                    "Default behavior is to sort strings for better diff support.", v => retain_order = v != null },
                { "a|analyze", "Run the string analyzer after generation", v => analyze = v != null },
                { "analyzer-config=", "Path to a configuration file for the analyzer; use with --analyze", v => analyer_config_path = v },
                { "analyzer-warnaserror", "Treat analyzer warnings as errors", v => analyzer_warn_as_error = v != null },
                { "reduce-master=", "Reduce a master localized PO file, " +
                    "keeping only strings defined by another unlocalized PO[T] file", v => reduce_master_path = v },
                { "reduce-retain=", "An unlocalized PO[T] file used to " +
                    "determine which strings from reduce-master should be retained", v => reduce_retain_path = v },
                { "android-input-strings-xml=", "Input file of unlocalized Android Strings.xml " +
                    "for preserving hand-maintained string resources", v => android_input_strings_xml = v },
                { "android-output-strings-xml=", "Output file of localized Android Strings.xml " +
                    "for preserving hand-maintained string resources", v => android_output_strings_xml = v },
                { "init-with-locale=", "Init a .po file for the locale", v => initWithLocale = v},
                { "pot", v => generate_pot = v != null },
                { "exclude-po-header", v => exclude_po_header = v != null },
                { "l|log", "Display logging", v => log = v != null },
                { "m|meta=", "Add localization metadata (key=value)", v => {
                    var parts = v.Split (new [] { '=' }, 2);
                    if (parts != null && parts.Length == 2) {
                        if (metadata == null) {
                            metadata = new LocalizationMetadata ();
                        }

                        metadata.Add (parts[0].Trim (), parts[1].Trim ());
                    }
                } },
                { "v|verbose", "Verbose logging", v => verbose = v != null },
                { "h|help", "Show this help message and exit", v => show_help = v != null }
            };

            try {
                options.Parse (args);

                if (show_help) {
                    Console.WriteLine ("Usage: vernacular [OPTIONS]+");
                    Console.WriteLine ();
                    Console.WriteLine ("Options:");
                    options.WriteOptionDescriptions (Console.Out);
                    return 1;
                }

                if (source_root_path != null) {
                    if (!Directory.Exists (source_root_path)) {
                        throw new OptionException ("invalid source-root", "source-root");
                    }

                    source_root_path = new DirectoryInfo (source_root_path).FullName;
                }

                generator = Generator.GetGeneratorForName (generator_name.ToLower ());
                if (generator == null) {
                    throw new OptionException ("invalid generator", "generator");
                }

                generator.RetainStringOrder = retain_order;

                if (generator is PoGenerator) {
                    ((PoGenerator)generator).PotMode = generate_pot;
                    ((PoGenerator)generator).ExcludeHeaderMetadata = exclude_po_header;
                }

                if (initWithLocale != null && !(generator is PoGenerator)) {
                    throw new OptionException ("locale option only valid with po generator", "locale");
                }

                if (initWithLocale != null && generate_pot) {
                    throw new OptionException ("you can not use -locale with -pot", "locale");
                }

                if (initWithLocale != null) {
                    (generator as PoGenerator).InitWithLocale = initWithLocale;
                }

                if (reduce_master_path != null && reduce_retain_path == null) {
                    throw new OptionException ("reduce-retain must be specified if reduce-master is", "reduce-retain");
                } else if (reduce_master_path == null && reduce_retain_path != null) {
                    throw new OptionException ("reduce-master must be specified if reduce-retain is", "reduce-master");
                } else if (reduce_master_path != null && reduce_retain_path != null) {
                    var reduce_master = new PoParser { SourceRootPath = source_root_path };
                    var reduce_retain = new PoParser { SourceRootPath = source_root_path };

                    reduce_master.Add (reduce_master_path);
                    reduce_retain.Add (reduce_retain_path);

                    generator.Reduce (reduce_master, reduce_retain);

                    generator.Generate (output_path);

                    return 0;
                }
            } catch (OptionException e) {
                Console.WriteLine ("vernacular: {0}", e.Message);
                Console.WriteLine ("Try `vernacular --help` for more information.");
                return 1;
            }

            var parser = new AggregateParser {
                SourceRootPath = source_root_path
            };

            if (verbose) {
                parser.LogLevel = 2;
            } else if (log) {
                parser.LogLevel = 1;
            }

            StringAnalyzer analyzer = null;

            if (analyze) {
                try {
                    analyzer = new StringAnalyzer (analyer_config_path, analyzer_warn_as_error);
                } catch {
                    return 1;
                }
            }

            foreach (var input_path in input_paths) {
                if (File.Exists (input_path)) {
                    parser.Add (input_path);
                    continue;
                }

                var search_pattern = "*";
                var dir = input_path;

                if (!Directory.Exists (dir)) {
                    search_pattern = Path.GetFileName (dir);
                    dir = Path.GetDirectoryName (dir);
                    if (!Directory.Exists (dir)) {
                        continue;
                    }
                }

                foreach (var path in Directory.EnumerateFiles (dir, search_pattern, SearchOption.TopDirectoryOnly)) {
                    parser.Add (path);
                }
            }

            if (metadata != null) {
                generator.Add (metadata);
            }

            foreach (var localization_unit in parser.Parse ()) {
                generator.Add (localization_unit);

                var localized_string = localization_unit as LocalizedString;
                if (analyzer != null) {
                    analyzer.Add (localized_string);
                }
            }

            if (analyzer != null) {
                var error_count = analyzer.Analyze ();
                if (error_count > 0) {
                    Console.WriteLine ("The analyzer reported {0} errors. Generation skipped.", error_count);
                    return 1;
                }
            }

            generator.Generate (output_path);

            if (generator is AndroidGenerator && android_input_strings_xml != null && android_output_strings_xml != null) {
                ((AndroidGenerator)generator).LocalizeManualStringsXml (android_input_strings_xml, android_output_strings_xml);
            }

            return 0;
        }
    }
}
