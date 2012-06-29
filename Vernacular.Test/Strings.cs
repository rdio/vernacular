//
// Strings.cs
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

using Vernacular;

namespace Vernacular.Test
{
    public static class Strings
    {
        public static string NeutralSingular ()
        {
            return Catalog.GetString ("NeutralSingular",
                comment: "comment=NeutralSingular");
        }

        public static string NeutralPlural (int n)
        {
            return Catalog.GetPluralString ("NeutralPlural:P0", "NeutralPlural:P1",
                n, comment: "comment=NeutralPlural");
        }

        public static string GenderSingular (LanguageGender gender)
        {
            return Catalog.GetGenderString (gender, "GenderSingular",
                comment: "comment=GenderSingular");
        }

        public static string GenderPlural (LanguageGender gender, int n)
        {
            return Catalog.GetPluralGenderString (gender, "GenderPlural:P0", "GenderPlural:P1",
                n, comment: "comment=GenderPlural");
        }

        public static readonly string FieldMessage = Catalog.GetString ("$FieldMessage", comment: "comment=$FieldMessage");

        public static string PropertyMessage {
            get { return Catalog.GetString ("$PropertyMessage", comment: "comment=$PropertyMessage"); }
        }

        public static readonly string StaticCtorMessage;

        static Strings ()
        {
            StaticCtorMessage = Catalog.GetString ("$StaticCtorMessage", comment: "comment=$StaticCtorMessage");
        }

        public static string Nested1 ()
        {
            Func<string, string> n = s => s;
            return n (n (n (n (n (n (n (n (Catalog.GetString ("$Nested", comment: "comment=$Nested")))))))));
        }

        public static string Nested2 (int i)
        {
            Func<string, string, string, string> n = (s1, s2, s3) => s1 + s2 + s3;

            return n (
                "a",
                Catalog.GetString ("$b", comment: "comment=$b"),
                n (
                    Catalog.GetString ("$c", comment: "comment=$c"),
                    n (
                        Catalog.GetPluralString ("$d:P0", "$d:P1", i, comment: "comment=$d"),
                        Catalog.GetString ("$e", comment: "comment=$e"),
                        n (
                            "f",
                            Catalog.GetString ("$g", comment: "comment=$g"),
                            "h"
                        )
                    ),
                    "i"
                )
            );
        }
    }
}