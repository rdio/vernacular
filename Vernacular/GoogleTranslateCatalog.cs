//
// GoogleTranslateCatalog.cs
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

#if false

using System;
using System.Collections.Generic;
using System.Net;
using System.Json;

namespace Vernacular
{
    public class GoogleTranslateCatalog : Catalog
    {
        private Dictionary<string, string> translation_cache = new Dictionary<string, string> ();

        public string ApiKey { get; set; }

        public string TranslateString (string message)
        {
            lock (translation_cache) {
                string translation;
                if (translation_cache.TryGetValue (message, out translation)) {
                    return translation;
                }

                if (String.IsNullOrEmpty (ApiKey)) {
                    throw new ArgumentException ("the ApiKey property must be set");
                }

                var query = String.Format ("https://www.googleapis.com/language/translate/v2?key={0}&q={1}&source=en&target={2}",
                    ApiKey, Uri.EscapeDataString (message), CurrentIsoLanguageCode);

                using (var client = new WebClient ()) {
                    var json = JsonObject.Parse (client.DownloadString (query));
                    translation = (string)json["data"]["translations"][0]["translatedText"];
                    translation_cache.Add (message, translation);
                    return translation;
                }
            }
        }

        public override string CoreGetString (string message)
        {
            return TranslateString (message);
        }

        public override string CoreGetPluralString (string singularMessage, string pluralMessage, int n)
        {
            return TranslateString (singularMessage);
        }

        public override string CoreGetGenderString (LanguageGender gender, string masculineMessage, string feminineMessage)
        {
            return TranslateString (masculineMessage);
        }

        public override string CoreGetPluralGenderString (LanguageGender gender,
            string singularMasculineMessage, string pluralMasculineMessage,
            string singularFeminineMessage, string pluralFeminineMessage,
            int n)
        {
            return TranslateString (singularMasculineMessage);
        }
    }
}

#endif
