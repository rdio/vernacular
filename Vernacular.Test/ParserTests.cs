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

        [Test]
        public void TestParseAssembly ()
        {
            var localization_units = ParseAssembly ();

            Assert.AreEqual (6, localization_units.Count);

            foreach (var unit in localization_units) {
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
                    case "GenderSingular:M":
                        Assert.AreEqual (LanguageGender.Masculine, str.Gender);
                        break;
                    case "GenderSingular:F":
                        Assert.AreEqual (LanguageGender.Feminine, str.Gender);
                        break;
                    case "GenderPlural:P0:M":
                        Assert.AreEqual (LanguageGender.Masculine, str.Gender);
                        Assert.AreEqual ("GenderPlural:P1:M", str.UntranslatedPluralValue);
                        break;
                    case "GenderPlural:P0:F":
                        Assert.AreEqual (LanguageGender.Feminine, str.Gender);
                        Assert.AreEqual ("GenderPlural:P1:F", str.UntranslatedPluralValue);
                        break;
                    default:
                        Assert.Fail ("unexpected localization unit message: " + message);
                        break;
                }
            }
        }
    }
}