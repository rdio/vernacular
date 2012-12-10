﻿//
// PluralRules.cs
//
// Author:
//   Aaron Bockover <abock@rd.io>
//   Stephane Delcroix <stephane@delcroix.org>
//
// Copyright 2012 Rdio, Inc.
// Copyright 2012 S. Delcroix
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
    public static class PluralRules
    {
        struct PluralInfo
        {
            public int nforms;
            public string header;
            public Func<int, int> getOrder;
        }
        
        public static int GetOrder (string isoLanguageCode, int n)
        {
            return GetPluralInfo(isoLanguageCode).getOrder(n);
        }

        public static int GetNumberOfPlurals (string isoLanguageCode)
        {
            return GetPluralInfo(isoLanguageCode).nforms;
        }

        public static string GetPluralHeader (string isoLanguageCode)
        {
            return GetPluralInfo(isoLanguageCode).header;
        }

        static PluralInfo GetPluralInfo (string isoLanguageCode)
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
                    return new PluralInfo
                               {
                                   nforms = 1,
                                   header = "Plural-Forms: nplurals=1; plural=0;", 
                                   getOrder = n => 0
                               };
                case "is":  // Icelandic
                    // 2 forms
                    return new PluralInfo {
                        nforms = 2,
                        header = "Plural-Forms: nplurals=2; plural=(n % 10 != 1 || n % 100 == 11) ? 1 : 0;",
                        getOrder = n => (n % 10 != 1 || n % 100 == 11) ? 1 : 0
                    };                    
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
                    return new PluralInfo {
                        nforms = 2,
                        header = "Plural-Forms: nplurals=2; plural=n != 1 ? 1 : 0;",
                        getOrder = n => n != 1 ? 1 : 0
                    };
                case "mk":  // Macedonian
                    // 2 forms
                    return new PluralInfo {
                        nforms = 2,
                        header = "Plural-Forms: nplurals=2; plural=n == 1 || n % 10 == 1 ? 0 : 1;",
                        getOrder = n => n == 1 || n % 10 == 1 ? 0 : 1
                    };
                case "jv":  // Javanese
                    // 2 forms
                    return new PluralInfo {
                        nforms = 2,
                        header = "Plural-Forms: nplurals=2; plural=n != 0 ? 1 : 0;",
                        getOrder = n => n != 0 ? 1 : 0
                    };
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
                    return new PluralInfo {
                        nforms = 2,
                        header = "Plural-Forms: nplurals=2; plural=n > 1 ? 1 : 0;",
                        getOrder = n => n > 1 ? 1 : 0
                    };
                case "lt":  // Lithuanian
                    // 3 forms
                    return new PluralInfo {
                        nforms = 3,
                        header = "Plural-Forms: nplurals=3; plural=n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2;",
                        getOrder = n => n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2
                    };
                case "cs":  // Czech
                case "sk":  // Slovak
                    // 3 forms
                    return new PluralInfo {
                        nforms = 3,
                        header = "Plural-Forms: nplurals=3; plural=n == 1 ? 0 : (n >= 2 && n <= 4) ? 1 : 2;",
                        getOrder = n => n == 1 ? 0 : (n >= 2 && n <= 4) ? 1 : 2
                    };
                case "mnk": // Mandinka
                    // 3 forms
                    return new PluralInfo {
                        nforms = 3,
                        header = "Plural-Forms: nplurals=3; plural=n == 0 ? 0 : n == 1 ? 1 : 2;",
                        getOrder = n => n == 0 ? 0 : n == 1 ? 1 : 2
                    };
                case "lv":  // Latvian
                    // 3 forms
                    return new PluralInfo {
                        nforms = 3,
                        header = "Plural-Forms: nplurals=3; plural=n % 10 == 1 && n % 100 != 11 ? 0 : n != 0 ? 1 : 2;",
                        getOrder = n => n % 10 == 1 && n % 100 != 11 ? 0 : n != 0 ? 1 : 2
                    };                    
                case "ro":  // Romanian
                    // 3 forms
                    return new PluralInfo {
                        nforms = 3,
                        header = "Plural-Forms: nplurals=3; plural=n == 1 ? 0 : (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? 1 : 2;",
                        getOrder = n => n == 1 ? 0 : (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? 1 : 2
                    };  
                case "be":  // Belarusian
                case "bs":  // Bosnian
                case "hr":  // Croatian
                case "ru":  // Russian
                case "sr":  // Serbian
                case "uk":  // Ukrainian
                    // 3 forms
                    return new PluralInfo {
                        nforms = 3,
                        header = "Plural-Forms: nplurals=3; plural=n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2;",
                        getOrder = n => n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2
                    };
                case "pl":  // Polish
                    // 3 forms
                    return new PluralInfo {
                        nforms = 3,
                        header = "Plural-Forms: nplurals=3; plural=n == 1 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2;",
                        getOrder = n => n == 1 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2
                    };
                case "kw":  // Cornish
                    // 4 forms
                    return new PluralInfo {
                        nforms = 4,
                        header = "Plural-Forms: nplurals=4; plural=n == 1 ? 0 : (n == 2) ? 1 : (n == 3) ? 2 : 3;",
                        getOrder = n => n == 1 ? 0 : (n == 2) ? 1 : (n == 3) ? 2 : 3
                    };
                case "gd":  // Scottish Gaelic
                    // 4 forms
                    return new PluralInfo {
                        nforms = 4,
                        header = "Plural-Forms: nplurals=4; plural=(n == 1 || n == 11) ? 0 : (n == 2 || n == 12) ? 1 : (n > 2 && n < 20) ? 2 : 3;",
                        getOrder = n => (n == 1 || n == 11) ? 0 : (n == 2 || n == 12) ? 1 : (n > 2 && n < 20) ? 2 : 3
                    };
                case "mt":  // Maltese
                    // 4 forms
                    return new PluralInfo {
                        nforms = 4,
                        header = "Plural-Forms: nplurals=4; plural=n == 1 ? 0 : n == 0 || (n % 100 > 1 && n % 100 < 11) ? 1 : (n % 100 > 10 && n % 100 < 20) ? 2 : 3;",
                        getOrder = n => n == 1 ? 0 : n == 0 || (n % 100 > 1 && n % 100 < 11) ? 1 : (n % 100 > 10 && n % 100 < 20) ? 2 : 3
                    };
                case "cy":  // Welsh
                    // 4 forms
                    return new PluralInfo {
                        nforms = 4,
                        header = "Plural-Forms: nplurals=4; plural=n == 1 ? 0 : (n == 2) ? 1 : (n != 8 && n != 11) ? 2 : 3;",
                        getOrder = n => n == 1 ? 0 : (n == 2) ? 1 : (n != 8 && n != 11) ? 2 : 3
                    };
                case "sl":  // Slovenian
                    // 4 forms
                    return new PluralInfo {
                        nforms = 4,
                        header = "Plural-Forms: nplurals=4; plural=n % 100 == 1 ? 1 : n % 100 == 2 ? 2 : n % 100 == 3 || n % 100 == 4 ? 3 : 0;",
                        getOrder = n => n % 100 == 1 ? 1 : n % 100 == 2 ? 2 : n % 100 == 3 || n % 100 == 4 ? 3 : 0
                    };
                case "ga":  // Irish
                    // 5 forms
                    return new PluralInfo {
                        nforms = 5,
                        header = "Plural-Forms: nplurals=5; plural=n == 1 ? 0 : n == 2 ? 1 : n < 7 ? 2 : n < 11 ? 3 : 4;",
                        getOrder = n => n == 1 ? 0 : n == 2 ? 1 : n < 7 ? 2 : n < 11 ? 3 : 4
                    };
                default:
                    return new PluralInfo {
                        nforms = 2,
                        header = "Plural-Forms: nplurals=2; plural=n != 1 ? 1 : 0;",
                        getOrder = n => n != 1 ? 1 : 0
                    };
            }
        }
    }
}