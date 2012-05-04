//
// FieldReflectionResourceCatalog.cs
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

namespace Vernacular
{
    public abstract class FieldReflectionResourceCatalog<T> : Catalog
    {
        private struct CachedString : IEquatable<CachedString>
        {
            public string Message;
            public LanguageGender Gender;
            public int PluralOrder;

            public bool Equals (CachedString other)
            {
                return Message == other.Message && Gender == other.Gender && PluralOrder == other.PluralOrder;
            }

            public override bool Equals (object obj)
            {
                if (obj is CachedString) {
                    return Equals ((CachedString)obj);
                }

                return false;
            }

            public override int GetHashCode ()
            {
                unchecked {
                    var hash = 17;
                    hash = hash * 31 + Message.GetHashCode ();
                    hash = hash * 31 + (int)Gender;
                    hash = hash * 31 + PluralOrder;
                    return hash;
                }
            }
        }

        private Type reflection_type;
        private Dictionary<CachedString, T> string_cache = new Dictionary<CachedString, T> ();

        public FieldReflectionResourceCatalog (Type reflectionType)
        {
            reflection_type = reflectionType;
        }

        protected bool GetResource (out T resource, string message,
            LanguageGender gender = LanguageGender.Neutral, int pluralCount = 1)
        {
            var cached_string = new CachedString {
                Message = message,
                Gender = gender,
                PluralOrder = PluralRules.GetOrder (CurrentIsoLanguageCode, pluralCount)
            };

            if (string_cache.TryGetValue (cached_string, out resource)) {
                return true;
            }

            var id = GetResourceId (ResourceIdType.ComprehensibleIdentifier,
                message, gender, cached_string.PluralOrder);
            var field = reflection_type.GetField (id);

            if (field == null) {
                return false;
            }

            resource = (T)field.GetValue (null);
            string_cache.Add (cached_string, resource);

            return true;
        }
    }
}
