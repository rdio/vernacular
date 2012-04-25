#if !VERNACULAR_TOOL

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