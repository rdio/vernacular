//
// ResourceCatalog.cs
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

namespace Vernacular
{
    public class ResourceCatalog : Catalog
    {
        public Func<string, string> GetResourceById { get; set; }
        public ResourceIdType ResourceIdType { get; set; }

        public ResourceCatalog ()
        {
            ResourceIdType = ResourceIdType.ComprehensibleIdentifier;
        }

        protected virtual string GetResource (string message,
            LanguageGender gender = LanguageGender.Neutral, int pluralCount = 1)
        {
            if (GetResourceById == null) {
                return null;
            }

            var plural_order = PluralRules.GetOrder (CurrentIsoLanguageCode, pluralCount);
            var resource_id = GetResourceId (ResourceIdType, message, gender, plural_order);

            if (resource_id == null) {
                return null;
            }

            return GetResourceById (resource_id);
        }

        public override string CoreGetString (string message)
        {
            return CoreFilter (GetResource (message) ?? DefaultImplementation.CoreGetString (message));
        }

        public override string CoreGetPluralString (string singularMessage, string pluralMessage, int n)
        {
            return CoreFilter (GetResource (singularMessage, pluralCount: n)
                ?? DefaultImplementation.CoreGetPluralString (singularMessage, pluralMessage, n));
        }

        public override string CoreGetGenderString (LanguageGender gender,
            string masculineMessage, string feminineMessage)
        {
            return CoreFilter (GetResource (masculineMessage, gender: gender)
                ?? DefaultImplementation.CoreGetGenderString (gender, masculineMessage, feminineMessage));
        }

        public override string CoreGetPluralGenderString (LanguageGender gender,
            string singularMasculineMessage, string pluralMasculineMessage,
            string singularFeminineMessage, string pluralFeminineMessage,
            int n)
        {
            return CoreFilter (GetResource (singularMasculineMessage, gender: gender, pluralCount: n)
                ?? DefaultImplementation.CoreGetPluralGenderString (gender,
                    singularMasculineMessage, pluralMasculineMessage,
                    singularFeminineMessage, pluralFeminineMessage,
                    n));
        }
    }
}