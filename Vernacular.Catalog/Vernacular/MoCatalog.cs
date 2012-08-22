using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vernacular.Tool;

namespace Vernacular
{
    public class MoCatalog : ResourceCatalog
    {        
        public MoCatalog (Stream mo_stream)
        {
            foreach (var resourceString in GetAllResourceStrings(mo_stream)) {
                resources.Add(resourceString.Id, resourceString.Translated);
            }
        }

        IEnumerable<ResourceString> GetAllResourceStrings (Stream mo_stream) {
            var resource_strings = new Dictionary<string, ResourceString> ();

            using (var mo_parser = new MoParser (mo_stream)) {
                foreach (var resource_string in from localized_string in mo_parser.Parse()
                                                from resource_string in GetResourceStrings(localized_string)
                                                where !resource_strings.ContainsKey(resource_string.Id)
                                                select resource_string) {
                    resource_strings.Add(resource_string.Id, resource_string);
                }
            }
            return from resource_string in resource_strings select resource_string.Value;
        }

        protected IEnumerable<ResourceString> GetResourceStrings (LocalizedString localizedString) {
            return ResourceString.Generate (ResourceIdType, localizedString);
        }

        Dictionary<string, string> resources = new Dictionary<string, string> (); 

        public override Func<string, string> GetResourceById {
            get { return GetResourceByIdImplementation; }
            set { throw new InvalidOperationException (); }
        }

        protected override string GetResource (string context, string message, LanguageGender gender = LanguageGender.Neutral, int pluralCount = 1) {
            return message == null ? null : base.GetResource (context, message, gender, pluralCount);
        }

        private string GetResourceByIdImplementation (string id) {
            if (id == null) {
                return null;
            }

            string resource;
            return resources.TryGetValue (id, out resource) ? resource : null;
        }

        class MoParser : IDisposable
        {
            public Stream MoStream { get; set; }

            public MoParser(Stream moStream)
            {
                MoStream = moStream;
            }

            public IEnumerable<LocalizedString> Parse ()
            {
                uint number_of_strings;
                uint original_strings_table_offset;
                uint translations_table_offset;
                uint hash_table_size;
                uint hash_table_offset;
                using (var reader = new BinaryReader (MoStream))
                {
                    if (reader.ReadUInt32 () != 0x950412de)
                        throw new Exception ("Wrong magic");
                    if (reader.ReadUInt32 () != 0x0)
                        throw new Exception ("unsupported file version");
                    number_of_strings = reader.ReadUInt32 ();
                    original_strings_table_offset = reader.ReadUInt32 ();
                    translations_table_offset = reader.ReadUInt32 ();
                    hash_table_size = reader.ReadUInt32 ();
                    hash_table_offset = reader.ReadUInt32 ();

                    for (var i=0;i<number_of_strings;i++) {
                        var localized_string = new LocalizedString();
                        reader.BaseStream.Seek (original_strings_table_offset + i*8, SeekOrigin.Begin);
                        var original_string_length = reader.ReadUInt32 ();
                        var original_string_offset = reader.ReadUInt32 ();
                        reader.BaseStream.Seek (translations_table_offset + i*8, SeekOrigin.Begin);
                        var translation_length = reader.ReadUInt32 ();
                        var translation_offset = reader.ReadUInt32 ();
                        reader.BaseStream.Seek (original_string_offset, SeekOrigin.Begin);
                        var original_string_bytes = reader.ReadBytes((int)original_string_length);
                        var original_string = Encoding.UTF8.GetString (original_string_bytes, 0, original_string_bytes.Length).Split ('\0');
                        reader.BaseStream.Seek (translation_offset, SeekOrigin.Begin);
                        var translation_bytes = reader.ReadBytes((int) translation_length);
                        var translation = Encoding.UTF8.GetString (translation_bytes, 0, translation_bytes.Length).Split('\0');

                        localized_string.UntranslatedSingularValue = original_string[0];
                        if (original_string.Count () == 2) {
                            localized_string.UntranslatedPluralValue = original_string[1];
                        }
                        localized_string.TranslatedValues = translation;

                        yield return localized_string;
                    }                
                }
            }

            public void Dispose()
            {
                if (MoStream == null) {
                    return;
                }

                MoStream.Dispose();
                MoStream = null;
            }
        }

    }

}
