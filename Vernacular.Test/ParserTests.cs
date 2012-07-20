//
// ParserTests.cs
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
using System.Linq;
using NUnit.Framework;

using Vernacular.Tool;
using Vernacular.Parsers;

namespace Vernacular.Test
{
    [TestFixture]
    public class ParserTests
    {
        public static List<ILocalizationUnit> ParseAssembly ()
        {
            var asm_parser = new AssemblyParser ();
            asm_parser.Add (typeof(Catalog).Assembly.Location);
            asm_parser.Add (typeof(Strings).Assembly.Location);
            return new List<ILocalizationUnit> (asm_parser.Parse ());
        }

        public static List<ILocalizationUnit> ParseXap ()
        {
            var aggregate_parser = new AggregateParser ();
            var xap_parser = new XapParser(aggregate_parser);
            xap_parser.Add ("../../Xaps/XapStrings.xap");
            return new List<ILocalizationUnit> (aggregate_parser.Parse ());
        }

        [Test]
        public void TestParseAssembly ()
        {
            AssertUnits (ParseAssembly ());
        }

        [Test]
        public void TestParsePo ()
        {
            var po_parser = new PoParser ();
            po_parser.Add ("../../Catalog/en_US.pot");
            AssertUnits (new List<ILocalizationUnit> (po_parser.Parse ()));
        }

        [Test]
        public void TestParseXap ()
        {
            AssertUnits (ParseXap ());
        }

        private void AssertUnits (List<ILocalizationUnit> units)
        {
            Assert.IsTrue (units.Any ());
            if (units [0] is LocalizationMetadata) {
                units.RemoveAt (0);
            }

            Assert.AreEqual (17, units.Count);

            int gs_m_count = 0;
            int gs_f_count = 0;
            int gs_n_count = 0;
            int gp_m_count = 0;
            int gp_f_count = 0;
            int gp_n_count = 0;

            foreach (var unit in units) {
                var str = unit as LocalizedString;
                Assert.IsNotNull (str);

                var message = str.UntranslatedSingularValue;

                Assert.AreEqual ("comment=" + message.Split(':')[0], str.DeveloperComments);

                switch (message) {
                    case "NeutralSingular":
                        Assert.AreEqual (LanguageGender.Neutral, str.Gender);
                        break;
                    case "NeutralPlural:P0":
                        Assert.AreEqual (LanguageGender.Neutral, str.Gender);
                        Assert.AreEqual ("NeutralPlural:P1", str.UntranslatedPluralValue);
                        break;
                    case "GenderSingular":
                        switch (str.Gender) {
                            case LanguageGender.Masculine: gs_m_count++; break;
                            case LanguageGender.Feminine: gs_f_count++; break;
                            case LanguageGender.Neutral: gs_n_count++; break;
                            default: Assert.Fail ("invalid LanguageGender, should never be reached"); break;
                        }
                        break;
                    case "GenderPlural:P0":
                        Assert.AreEqual ("GenderPlural:P1", str.UntranslatedPluralValue);

                        switch (str.Gender) {
                            case LanguageGender.Masculine: gp_m_count++; break;
                            case LanguageGender.Feminine: gp_f_count++; break;
                            case LanguageGender.Neutral: gp_n_count++; break;
                            default: Assert.Fail ("invalid LanguageGender, should never be reached"); break;
                        }
                        break;
                    case "$b":
                    case "$c":
                    case "$d:P0":
                    case "$e":
                    case "$g":
                    case "$Nested":
                    case "$FieldMessage":
                    case "$PropertyMessage":
                    case "$StaticCtorMessage":
                        break;
                    default:
                        Assert.Fail ("unexpected localization unit message: " + message);
                        break;
                }

            }

            Assert.AreEqual (1, gs_m_count, "gs_m_count");
            Assert.AreEqual (1, gs_f_count, "gs_f_count");
            Assert.AreEqual (1, gs_n_count, "gs_n_count");
            Assert.AreEqual (1, gp_m_count, "gp_m_count");
            Assert.AreEqual (1, gp_f_count, "gp_f_count");
            Assert.AreEqual (1, gp_n_count, "gp_n_count");
        }
    }
}