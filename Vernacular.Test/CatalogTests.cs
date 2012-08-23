//
// CatalogTests.cs
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

using NUnit.Framework;

using Vernacular.Parsers;

namespace Vernacular.Test
{
    [TestFixture]
    public class CatalogTests
    {
        private List<Tuple<string, ResourceCatalog>> catalogs = new List<Tuple<string, ResourceCatalog>> ();

        [TestFixtureSetUp]
        public void SetUp ()
        {
            foreach (var path in Directory.GetFiles ("../../Catalog", "*.po")) {
                var parser = new PoParser ();
                parser.Add (path);
                var lang = Path.GetFileNameWithoutExtension (path);
                catalogs.Add (new Tuple<string, ResourceCatalog> (lang, new ParserResourceCatalog (parser) { CurrentIsoLanguageCode = lang }));
            }

            (new GeneratorTests ()).TestMoGenerator ();
            foreach (var path in Directory.GetFiles ("../../Catalog", "*.mo")) {
                var lang = Path.GetFileNameWithoutExtension (path);
                var catalog = new MoCatalog (File.Open (path, FileMode.Open)) { CurrentIsoLanguageCode = lang };
                catalogs.Add(new Tuple<string, ResourceCatalog> (lang, catalog));                
            }
        }

        private string GenderTag (LanguageGender gender)
        {
            switch (gender) {
                case LanguageGender.Masculine:
                    return ":M";
                case LanguageGender.Feminine:
                    return ":F";
                default:
                    return String.Empty;
            }
        }

        private void ForEachCatalog (Action<string> action)
        {
            foreach (var catalog in catalogs) {
                Catalog.Implementation = catalog.Item2;
                action (catalog.Item1);
            }
        }

        private void AssertTranslations (string orig, LanguageGender gender, Func<string> messageGetter)
        {
            ForEachCatalog (lang => Assert.AreEqual (lang + "|" + orig + GenderTag (gender), messageGetter ()));
        }

        private void AssertTranslations (string orig, LanguageGender gender, Func<int, string> messageGetter)
        {
            ForEachCatalog (lang => {
                for (int i = 0; i < 10000; i++) {
                    var order = PluralRules.GetOrder (Catalog.Implementation.CurrentIsoLanguageCode, i);
                    Assert.AreEqual (lang + ",P" + order + "|" + orig + GenderTag (gender), messageGetter (i));
                }
            });
        }

        [Test]
        public void TestNeutralSingular ()
        {
            AssertTranslations ("NeutralSingular", LanguageGender.Neutral, () => Strings.NeutralSingular ());
        }

        [Test]
        public void TestNeutralPlural ()
        {
            AssertTranslations ("NeutralPlural", LanguageGender.Neutral, i => Strings.NeutralPlural (i));
        }

        [Test]
        public void TestGenderSingularMasculine ()
        {
            AssertTranslations ("GenderSingular", LanguageGender.Masculine, () => Strings.GenderSingular (LanguageGender.Masculine));
        }

        [Test]
        public void TestGenderSingularFeminine ()
        {
            AssertTranslations ("GenderSingular", LanguageGender.Feminine, () => Strings.GenderSingular (LanguageGender.Feminine));
        }

        [Test]
        public void TestGenderSingularNeutral ()
        {
            AssertTranslations ("GenderSingular", LanguageGender.Neutral, () => Strings.GenderSingular (LanguageGender.Neutral));
        }

        [Test]
        public void TestGenderPluralMasculine ()
        {
            AssertTranslations ("GenderPlural", LanguageGender.Masculine, i => Strings.GenderPlural (LanguageGender.Masculine, i));
        }

        [Test]
        public void TestGenderPluralFeminine ()
        {
            AssertTranslations ("GenderPlural", LanguageGender.Feminine, i => Strings.GenderPlural (LanguageGender.Feminine, i));
        }

        [Test]
        public void TestGenderPluralNeutral ()
        {
            AssertTranslations ("GenderPlural", LanguageGender.Neutral, i => Strings.GenderPlural (LanguageGender.Neutral, i));
        }

        [Test]
        public void TestNested1 ()
        {
            ForEachCatalog (lang => Assert.AreEqual ("$_Nested", Strings.Nested1 ()));
        }

        [Test]
        public void TestNested2 ()
        {
            ForEachCatalog (lang => {
                for (int i = 0; i < 10000; i++) {
                    Assert.AreEqual (String.Format ("a$_B$_C$_D:P{0}$_Ef$_Ghi",
                        PluralRules.GetOrder (lang, i)), Strings.Nested2 (i));
                }
            });
        }

        [Test]
        public void TestStaticCtorMessage ()
        {
            ForEachCatalog (lang => Assert.AreEqual ("$_StaticCtorMessage", Strings.StaticCtorMessage));
        }

        [Test]
        public void TestFieldMessage ()
        {
            ForEachCatalog (lang => Assert.AreEqual ("$_FieldMessage", Strings.FieldMessage));
        }

        [Test]
        public void TestPropertyMessage ()
        {
            ForEachCatalog (lang => Assert.AreEqual ("$_PropertyMessage", Strings.PropertyMessage));
        }
    }
}