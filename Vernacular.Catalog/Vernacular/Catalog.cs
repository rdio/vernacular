//
// Catalog.cs
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
        /// <summary>
        /// The current 2-letters ISO language code. <seealso cref="http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes"/>
        /// </summary>
        public virtual string CurrentIsoLanguageCode {
            get { return current_iso_language_code ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName; }
            set { current_iso_language_code = value; }
        }

        #region Implementation Abstract Methods
        /// <summary>
        /// When overridden in a derived class, gets the localized version of <param name="message"/>.
        /// </summary>
        /// <param name="message">The original message</param>
        /// <returns>The localized message</returns>
        public abstract string CoreGetString (string message);

        /// <summary>
        /// When overridden in a derived class, gets the localized singular or plural message, depending on n
        /// </summary>
        /// <param name="singularMessage">The singular message</param>
        /// <param name="pluralMessage">The plural message</param>
        /// <param name="n">The plural count</param>
        /// <returns>The localized and pluralized message</returns>
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

        /// <summary>
        /// Return the localized translation of message, based on the current <see cref="Implementation">Catalog Implementation</see>
        /// and <see cref="CurrentIsoLanguageCode">ISO language code</see>>.
        /// </summary>
        /// <param name="message">The message to be localized</param>
        /// <param name="comment">Developer's comment, only visible by translators</param>
        /// <param name="implementation">Optionally provide a catalog implementation valid for this code only </param>
        /// <returns></returns>
        public static string GetString (string message, string comment = null) {
            return Implementation.CoreGetString (message);
        }

        public static string GetString (string message, Catalog implementation) {
            if (implementation == null)
                throw new ArgumentNullException("implementation");
            return implementation.CoreGetString (message);
        }

        public static string GetString (string message, string comment, Catalog implementation) {
            if (implementation == null)
                throw new ArgumentNullException("implementation");
            return implementation.CoreGetString (message);
        }

        /// <summary>
        /// Like <see cref="GetString(string, string)"/>, but consider plural forms. If a translation is found, apply the plural 
        /// formula to <param name="n"/>, and return the resulting message (some languages have more than two plural forms).
        /// If no translation is found, return <param name="singularMessage"/> if <param name="n"> is 1; return <param name="pluralMessage"> otherwise.
        /// 
        /// The Plural formula is computed from the <see cref="CurrentIsoLanguageCode"/>
        /// </summary>
        /// <param name="singularMessage">The singular message</param>
        /// <param name="pluralMessage">The plural message</param>
        /// <param name="n">The plural count</param>
        /// <param name="comment">Developer's comment, only visible by translators</param>
        /// <param name="implementation">Optionally provide a catalog implementation valid for this code only </param>
        /// <returns>The localized message, pluralized if required</returns>
        public static string GetPluralString (string singularMessage, string pluralMessage,
            int n, string comment = null) {
            return Implementation.CoreGetPluralString (singularMessage, pluralMessage, n);
        }

        public static string GetPluralString (string singularMessage, string pluralMessage,
            int n, Catalog implementation) {
            if (implementation == null)
                throw new ArgumentNullException("implementation");
            return implementation.CoreGetPluralString (singularMessage, pluralMessage, n);
        }

        public static string GetPluralString (string singularMessage, string pluralMessage,
            int n, string comment, Catalog implementation) {
            if (implementation==null)
                throw new ArgumentNullException("implementation");
            return implementation.CoreGetPluralString (singularMessage, pluralMessage, n);
        }

        public static string GetGenderString (LanguageGender gender, string masculineMessage,
            string feminineMessage, string comment = null) {
            return Implementation.CoreGetGenderString (gender, masculineMessage, feminineMessage);
        }

        public static string GetGenderString (LanguageGender gender, string message, string comment = null) {
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
            string pluralMessage, int n, string comment = null)
        {
            return Implementation.CoreGetPluralGenderString (gender, singularMessage, pluralMessage, singularMessage, pluralMessage, n);
        }

        public static string GetGenderString (ILanguageGenderProvider provider, string masculineMessage, string feminineMessage, string comment = null)
        {
            return Implementation.CoreGetGenderString (provider.LanguageGender, masculineMessage, feminineMessage);
        }

        public static string GetGenderString (ILanguageGenderProvider provider, string message, string comment = null)
        {
            return Implementation.CoreGetGenderString (provider.LanguageGender, message, message);
        }

        public static string GetPluralGenderString (ILanguageGenderProvider provider,
            string singularMasculineMessage, string pluralMasculineMessage,
            string singularFeminineMessage, string pluralFeminineMessage,
            int n, string comment = null)
        {
            return Implementation.CoreGetPluralGenderString (provider.LanguageGender,
                singularMasculineMessage, pluralMasculineMessage,
                singularFeminineMessage, pluralFeminineMessage,
                n);
        }

        public static string GetPluralGenderString (ILanguageGenderProvider provider, string singularMessage,
            string pluralMessage, int n, string comment = null)
        {
            return Implementation.CoreGetPluralGenderString (provider.LanguageGender,
                singularMessage, pluralMessage,
                singularMessage, pluralMessage,
                n);
        }

        #endregion

        #region Utilities
        /// <summary>
        /// An error handler for the FormatExceptions caught by Catalog.Format on translated strings. 
        /// It is recommended that you log those and reports the issue to the translator.
        /// </summary>
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
            if (message == null) {
                throw new ArgumentNullException ("message");
            }

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
