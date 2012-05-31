//
// Catalog.cs
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
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace Vernacular
{
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

        public static Action<Exception, string> ErrorHandler;

        public static string Format (string format, params object [] args)
        {
            try {
                return String.Format (format, args);
            } catch (FormatException e) {
                if (ErrorHandler != null) {
                    ErrorHandler (e, "Vernacular.Catalog.Format: invalid format string");
                }
                return format;
            }
        }

        public static string GetResourceId (ResourceIdType resourceIdType,
            string context, string message, LanguageGender gender, int pluralOrder)
        {
            var builder = new StringBuilder ("Vernacular_P");

            if (!String.IsNullOrEmpty (context)) {
                message = context + "__" + message;
            }

            builder.Append (pluralOrder);
            builder.Append ('_');

            if (gender != LanguageGender.Neutral) {
                builder.Append (gender == LanguageGender.Masculine ? "M_" : "F_");
            }

            switch (resourceIdType) {
                case ResourceIdType.ComprehensibleIdentifier:
                    break;
                case ResourceIdType.Base64:
                    message = Convert.ToBase64String (Encoding.UTF8.GetBytes (message));
                    break;
                default:
                    throw new Exception ("Unknown ResourceIdType");
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

        #endregion
    }
}
