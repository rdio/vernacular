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