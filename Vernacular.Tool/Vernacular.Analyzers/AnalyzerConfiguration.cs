//
// AnalyzerConfiguration.cs
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
using System.Xml;
using System.Xml.Linq;

namespace Vernacular.Analyzers
{
    internal class AnalyzerConfiguration
    {
        private Dictionary<string, List<string>> illegal_words = new Dictionary<string, List<string>> ();
        private List<string> supported_tags = new List<string> ();
        private List<string> spellcheck_dictionaries = new List<string> ();

        public Dictionary<string, List<string>> IllegalWords {
            get { return illegal_words; }
        }

        public List<string> SupportedTags {
            get { return supported_tags; }
        }

        public string HunspellDictionaryPath { get; set; }
        public string HunspellAffixPath { get; set; }

        public bool WarningsAsErrors { get; set; }

        public List<string> SpellcheckDictionaries {
            get { return spellcheck_dictionaries; }
        }

        private string configuration_path;

        public AnalyzerConfiguration (string configurationPath, bool warningsAsErrors = false)
        {
            WarningsAsErrors = warningsAsErrors;

            if (configurationPath != null) {
                configuration_path = configurationPath;
                ParseConfiguration (XDocument.Load (configurationPath, LoadOptions.SetLineInfo));
            }
        }

        private void ConfigError (IXmlLineInfo lineInfo, string message, params object [] args)
        {
            Console.Error.WriteLine ("Analyzer Configuration Error: {0} ({1}:{2},{3})",
                String.Format (message, args), configuration_path,
                lineInfo.LineNumber, lineInfo.LinePosition);
        }

        private void ParseConfiguration (XDocument doc)
        {
            var root = doc.Element ("vernacular-analyzer");
            if (root == null) {
                ConfigError (doc, "invalid root element name (should be 'vernacular-analyzer')");
                return;
            }

            foreach (var element in root.Elements ()) {
                switch (element.Name.ToString ()) {
                    case "warnings-as-errors":
                        bool warnings_as_errors;
                        if (!Boolean.TryParse (element.Value, out warnings_as_errors)) {
                            ConfigError (element, "invalid boolean value for warnings-as-errors");
                            return;
                        }

                        WarningsAsErrors = warnings_as_errors;
                        break;
                    case "spellcheck":
                        var hunspell_affix = element.Attribute ("hunspell-affix");
                        var hunspell_dictionary = element.Attribute ("hunspell-dictionary");

                        if (hunspell_affix == null || hunspell_dictionary == null) {
                            ConfigError (element, "spellcheck element must have hunspell-affix and hunspell-dictionary attributes");
                            return;
                        }

                        HunspellAffixPath = hunspell_affix.Value;
                        HunspellDictionaryPath = hunspell_dictionary.Value;

                        foreach (var spellcheck_element in element.Elements ()) {
                            if (spellcheck_element.Name != "dictionary-path") {
                                ConfigError (spellcheck_element, "invalid spellcheck element: {0}", spellcheck_element.Name);
                                return;
                            }

                            var path = spellcheck_element.Attribute ("value");
                            if (path == null) {
                                ConfigError (spellcheck_element, "dictionary-path element without value");
                                return;
                            } else if (spellcheck_dictionaries.Contains (path.Value)) {
                                ConfigError (spellcheck_element, "dictionary-path already specified");
                                return;
                            }

                            spellcheck_dictionaries.Add (path.Value);
                        }
                        break;
                    case "supported-tags":
                        foreach (var supported_tags_element in element.Elements ()) {
                            if (supported_tags_element.Name != "tag") {
                                ConfigError (supported_tags_element,
                                    "invalid supported-tags element: {0}", supported_tags_element.Name);
                                return;
                            }

                            var tag = supported_tags_element.Attribute ("value");
                            if (tag == null) {
                                ConfigError (supported_tags_element, "tag element without value");
                                return;
                            } else if (supported_tags.Contains (tag.Value)) {
                                ConfigError (supported_tags_element, "tag already specified");
                                return;
                            }

                            supported_tags.Add (tag.Value);
                        }
                        break;
                    case "illegal-words":
                        foreach (var illegal_words_element in element.Elements ()) {
                            if (illegal_words_element.Name != "word") {
                                ConfigError (illegal_words_element,
                                    "invalid illegal-words element: {0}", illegal_words_element.Name);
                                return;
                            }
                            
                            var word = illegal_words_element.Attribute ("value");
                            if (word == null) {
                                ConfigError (illegal_words_element, "word element without value");
                                return;
                            } else if (illegal_words.ContainsKey (word.Value)) {
                                ConfigError (illegal_words_element, "word already specified");
                                return;
                            }

                            List<string> exceptions = null;

                            foreach (var except_element in illegal_words_element.Elements ()) {
                                if (except_element.Name != "except-in") {
                                    ConfigError (except_element, "invalid word element: {0}", except_element.Name);
                                    return;
                                }

                                if (exceptions == null) {
                                    exceptions = new List<string> ();
                                }

                                var except = except_element.Attribute ("value");
                                if (except == null) {
                                    ConfigError (except_element, "except-in without value");
                                    return;
                                } else if (exceptions.Contains (except.Value.ToLower ())) {
                                    ConfigError (except, "except-in already specified for word");
                                    return;
                                }

                                exceptions.Add (except.Value.ToLower ());
                            }

                            illegal_words.Add (word.Value.ToLower (), exceptions);
                        }
                        break;
                    default:
                        ConfigError (element, "Invalid element: {0}", element.Name);
                        return;
                }
            }
        }
    }
}
