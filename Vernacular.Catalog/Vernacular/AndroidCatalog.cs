//
// AndroidCatalog.cs
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

#if ANDROID

using System;

using Java.Util;
using Android.Content.Res;

namespace Vernacular
{
    public class AndroidCatalog : FieldReflectionResourceCatalog<int>
    {
        private Resources resources;

        public AndroidCatalog (Resources resources, Type reflectionType) : base (reflectionType)
        {
            this.resources = resources;
        }

        public override string CurrentIsoLanguageCode {
            get { return Locale.Default.Language; }
        }

        protected virtual string GetString (int androidResourceId)
        {
            return CoreFilter (resources.GetString (androidResourceId));
        }

        public override string CoreGetString (string message)
        {
            int resource_id;
            if (GetResource (out resource_id, message)) {
                return GetString (resource_id);
            }

            return CoreFilter (DefaultImplementation.CoreGetString (message));
        }

        public override string CoreGetPluralString (string singularMessage, string pluralMessage, int n)
        {
            int resource_id;
            if (GetResource (out resource_id, singularMessage, pluralCount: n)) {
                return GetString (resource_id);
            }

            return CoreFilter (DefaultImplementation.CoreGetPluralString (singularMessage, pluralMessage, n));
        }

        public override string CoreGetGenderString (LanguageGender gender, string masculineMessage, string feminineMessage)
        {
            int resource_id;
            if (GetResource (out resource_id, masculineMessage, gender: gender)) {
                return GetString (resource_id);
            }

            return CoreFilter (DefaultImplementation.CoreGetGenderString (gender, masculineMessage, feminineMessage));
        }

        public override string CoreGetPluralGenderString (LanguageGender gender,
            string singularMasculineMessage, string pluralMasculineMessage,
            string singularFeminineMessage, string pluralFeminineMessage,
            int n)
        {
            int resource_id;
            if (GetResource (out resource_id, singularMasculineMessage, gender: gender, pluralCount: n)) {
                return GetString (resource_id);
            }

            return CoreFilter (DefaultImplementation.CoreGetPluralGenderString (gender,
                singularMasculineMessage, pluralMasculineMessage,
                singularFeminineMessage, singularMasculineMessage,
                n));
        }
    }
}

#endif
