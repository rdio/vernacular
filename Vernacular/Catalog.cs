using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace Vernacular
{
    public class ResourceCatalog : Catalog
    {
        public Func<string, string> GetResourceById { get; set; }

        protected virtual string GetResource (string message,
            LanguageGender gender = LanguageGender.Neutral, int pluralCount = 1)
        {
            if (GetResourceById == null) {
                return null;
            }

            var plural_order = GetPluralOrder (CurrentIsoLanguageCode, pluralCount);
            var resource_id = GetResourceId (message, gender, plural_order);

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

    public abstract class Catalog
    {
        private sealed class DefaultCatalog : Catalog
        {
            public override string CoreGetString (string message)
            {
                return message;
            }

            public override string CoreGetPluralString (string singularMessage, string pluralMessage, int n)
            {
                return n == 1 ? singularMessage : pluralMessage;
            }

            public override string CoreGetGenderString (LanguageGender gender, string masculineMessage, string feminineMessage)
            {
                return gender == LanguageGender.Feminine ? feminineMessage : masculineMessage;
            }

            public override string CoreGetPluralGenderString (LanguageGender gender,
                string singularMasculineMessage, string pluralMasculineMessage,
                string singularFeminineMessage, string pluralFeminineMessage,
                int n)
            {
                return gender == LanguageGender.Feminine
                    ? (n == 1 ? singularFeminineMessage : pluralFeminineMessage)
                    : (n == 1 ? singularMasculineMessage : pluralMasculineMessage);
            }
        }

        private static Catalog default_implementation;
        public static Catalog DefaultImplementation {
            get { return default_implementation ?? (default_implementation = new DefaultCatalog ()); }
        }

        private static Catalog active_implementation;
        public static Catalog Implementation {
            get { return active_implementation ?? DefaultImplementation; }
            set { active_implementation = value; }
        }

        private string current_iso_language_code;
        public virtual string CurrentIsoLanguageCode {
            get { return current_iso_language_code ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName; }
            set { current_iso_language_code = value; }
        }

        #region Implementation Abstract Methods

        public abstract string CoreGetString (string message);

        public abstract string CoreGetPluralString (string singularMessage, string pluralMessage, int n);

        public abstract string CoreGetGenderString (LanguageGender gender, string masculineMessage, string feminineMessage);

        public abstract string CoreGetPluralGenderString (LanguageGender gender,
            string singularMasculineMessage, string pluralMasculineMessage,
            string singularFeminineMessage, string pluralFeminineMessage,
            int n);

        #endregion

        public Func<string, string> MessageFilter { get; set; }

        protected virtual string CoreFilter (string message)
        {
            return MessageFilter == null ? message : MessageFilter (message);
        }

        #region Public GetString Methods

        public static string GetString (string message, string comment = null)
        {
            if (Implementation == null) {
                return message;
            }

            return Implementation.CoreGetString (message);
        }

        public static string GetPluralString (string singularMessage, string pluralMessage,
            int n, string comment = null)
        {
            if (Implementation == null) {
                return n == 1 ? singularMessage : pluralMessage;
            }

            return Implementation.CoreGetPluralString (singularMessage, pluralMessage, n);
        }

        public static string GetGenderString (LanguageGender gender, string masculineMessage,
            string feminineMessage, string comment)
        {
            return Implementation.CoreGetGenderString (gender, masculineMessage, feminineMessage);
        }

        public static string GetGenderString (LanguageGender gender, string message, string comment)
        {
            return Implementation.CoreGetGenderString (gender, message, message);
        }

        public static string GetPluralGenderString (LanguageGender gender,
            string singularMasculineMessage, string pluralMasculineMessage,
            string singularFeminineMessage, string pluralFeminineMessage,
            int n, string comment = null)
        {
            return Implementation.CoreGetPluralGenderString (gender,
                singularMasculineMessage, pluralMasculineMessage,
                singularFeminineMessage, pluralFeminineMessage,
                n);
        }

        public static string GetPluralGenderString (LanguageGender gender, string singularMessage,
            string pluralMessage, int n, string comment)
        {
            return Implementation.CoreGetPluralGenderString (gender, singularMessage, pluralMessage, singularMessage, pluralMessage, n);
        }

        public static string GetGenderString (ILanguageGenderProvider provider, string masculineMessage, string feminineMessage, string comment)
        {
            return Implementation.CoreGetGenderString (provider.LanguageGender, masculineMessage, feminineMessage);
        }

        public static string GetGenderString (ILanguageGenderProvider provider, string message, string comment)
        {
            return Implementation.CoreGetGenderString (provider.LanguageGender, message, message);
        }

        public static string GetPluralGenderString (ILanguageGenderProvider provider,
            string singularMasculineMessage, string pluralMasculineMessage,
            string singularFeminineMessage, string pluralFeminineMessage,
            int n, string comment)
        {
            return Implementation.CoreGetPluralGenderString (provider.LanguageGender,
                singularMasculineMessage, pluralMasculineMessage,
                singularFeminineMessage, pluralFeminineMessage,
                n);
        }

        public static string GetPluralGenderString (ILanguageGenderProvider provider, string singularMessage,
            string pluralMessage, int n, string comment)
        {
            return Implementation.CoreGetPluralGenderString (provider.LanguageGender,
                singularMessage, pluralMessage,
                singularMessage, pluralMessage,
                n);
        }

        #endregion

        #region Utilities

        public static string GetResourceId (string message, LanguageGender gender, int pluralOrder)
        {
            var builder = new StringBuilder ("Vernacular_P");

            builder.Append (pluralOrder);
            builder.Append ('_');

            if (gender != LanguageGender.Neutral) {
                builder.Append (gender == LanguageGender.Masculine ? "M_" : "F_");
            }

            foreach (var c in message) {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_') {
                    builder.Append (c);
                } else if (c != ' ') {
                    builder.Append ((int)c);
                }
            }

            return builder.ToString ();
        }

        public static int GetPluralOrder (string isoLanguageCode, int n)
        {
            switch (isoLanguageCode) {
                case "ay":  // Aymará
                case "bo":  // Tibetan
                case "cgg": // Chiga
                case "dz":  // Dzongkha
                case "fa":  // Persian
                case "id":  // Indonesian
                case "ja":  // Japanese
                case "jbo": // Lojban
                case "ka":  // Georgian
                case "kk":  // Kazakh
                case "km":  // Khmer
                case "ko":  // Korean
                case "ky":  // Kyrgyz
                case "lo":  // Lao
                case "ms":  // Malay
                case "sah": // Yakut
                case "su":  // Sundanese
                case "th":  // Thai
                case "tt":  // Tatar
                case "ug":  // Uyghur
                case "vi":  // Vietnamese
                case "wo":  // Wolof
                    // 1 form
                    return 0;
                case "is":  // Icelandic
                    // 2 forms
                    return (n%10!=1 || n%100==11) ? 1 : 0;
                case "af":  // Afrikaans
                case "an":  // Aragonese
                case "ast": // Asturian
                case "az":  // Azerbaijani
                case "bg":  // Bulgarian
                case "bn":  // Bengali
                case "ca":  // Catalan
                case "da":  // Danish
                case "de":  // German
                case "el":  // Greek
                case "en":  // English
                case "eo":  // Esperanto
                case "es":  // Spanish
                case "et":  // Estonian
                case "eu":  // Basque
                case "fi":  // Finnish
                case "fo":  // Faroese
                case "fur": // Friulian
                case "fy":  // Frisian
                case "gl":  // Galician
                case "gu":  // Gujarati
                case "ha":  // Hausa
                case "he":  // Hebrew
                case "hi":  // Hindi
                case "hy":  // Armenian
                case "hu":  // Hungarian
                case "ia":  // Interlingua
                case "it":  // Italian
                case "kn":  // Kannada
                case "ku":  // Kurdish
                case "lb":  // Letzeburgesch
                case "mai": // Maithili
                case "ml":  // Malayalam
                case "mn":  // Mongolian
                case "mr":  // Marathi
                case "nah": // Nahuatl
                case "nap": // Neapolitan
                case "nb":  // Norwegian Bokmal
                case "ne":  // Nepali
                case "nl":  // Dutch
                case "se":  // Northern Sami
                case "nn":  // Norwegian Nynorsk
                case "no":  // Norwegian (old code)
                case "nso": // Northern Sotho
                case "or":  // Oriya
                case "ps":  // Pashto
                case "pa":  // Punjabi
                case "pap": // Papiamento
                case "pms": // Piemontese
                case "pt":  // Portuguese
                case "rm":  // Romansh
                case "sco": // Scots
                case "si":  // Sinhala
                case "so":  // Somali
                case "son": // Songhay
                case "sq":  // Albanian
                case "sw":  // Swahili
                case "sv":  // Swedish
                case "ta":  // Tamil
                case "te":  // Telugu
                case "tk":  // Turkmen
                case "ur":  // Urdu
                case "yo":  // Yoruba
                    // 2 forms
                    return n!=1 ? 1 : 0;
                case "mk":  // Macedonian
                    // 2 forms
                    return n==1 || n%10==1 ? 0 : 1;
                case "jv":  // Javanese
                    // 2 forms
                    return n!=0 ? 1 : 0;
                case "ach": // Acholi (maybe)
                case "ak":  // Akan
                case "am":  // Amharic
                case "br":  // Breton
                case "fil": // Filipino
                case "fr":  // French
                case "gun": // Gun
                case "ln":  // Lingala
                case "mfe": // Mauritian Creole
                case "mg":  // Malagasy
                case "mi":  // Maori
                case "oc":  // Occitan
                case "tg":  // Tajik
                case "ti":  // Tigrinya
                case "tr":  // Turkish
                case "uz":  // Uzbek
                case "wa":  // Walloon
                case "zh":  // Chinese
                    // 2 forms
                    return n>1 ? 1 : 0;
                case "lt":  // Lithuanian
                    // 3 forms
                    return (n%10==1 && n%100!=11 ? 0 : n%10>=2 && (n%100<10 || n%100>=20) ? 1 : 2);
                case "cs":  // Czech
                case "sk":  // Slovak
                    // 3 forms
                    return (n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2;
                case "mnk": // Mandinka
                    // 3 forms
                    return n==0 ? 0 : n==1 ? 1 : 2;
                case "lv":  // Latvian
                    // 3 forms
                    return (n%10==1 && n%100!=11 ? 0 : n != 0 ? 1 : 2);
                case "ro":  // Romanian
                    // 3 forms
                    return (n==1 ? 0 : (n==0 || (n%100 > 0 && n%100 < 20)) ? 1 : 2);
                case "be":  // Belarusian
                case "bs":  // Bosnian
                case "hr":  // Croatian
                case "ru":  // Russian
                case "sr":  // Serbian
                case "uk":  // Ukrainian
                    // 3 forms
                    return (n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2);
                case "pl":  // Polish
                    // 3 forms
                    return (n==1 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2);
                case "kw":  // Cornish
                    // 4 forms
                    return (n==1) ? 0 : (n==2) ? 1 : (n == 3) ? 2 : 3;
                case "gd":  // Scottish Gaelic
                    // 4 forms
                    return (n==1 || n==11) ? 0 : (n==2 || n==12) ? 1 : (n > 2 && n < 20) ? 2 : 3;
                case "mt":  // Maltese
                    // 4 forms
                    return (n==1 ? 0 : n==0 || ( n%100>1 && n%100<11) ? 1 : (n%100>10 && n%100<20 ) ? 2 : 3);
                case "cy":  // Welsh
                    // 4 forms
                    return (n==1) ? 0 : (n==2) ? 1 : (n != 8 && n != 11) ? 2 : 3;
                case "sl":  // Slovenian
                    // 4 forms
                    return (n%100==1 ? 1 : n%100==2 ? 2 : n%100==3 || n%100==4 ? 3 : 0);
                case "ga":  // Irish
                    // 5 forms
                    return n==1 ? 0 : n==2 ? 1 : n<7 ? 2 : n<11 ? 3 : 4;
                default:
                    return n != 1 ? 1 : 0;
            }
        }

        #endregion
    }
}