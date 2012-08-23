using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vernacular.Parsers;
using Vernacular.Tool;

namespace Vernacular.Generators
{
    public sealed class MoGenerator : BinaryGenerator
    {                
        protected override void Generate ()
        {
            StringList = Strings.OrderBy (ls=>ls.UntranslatedSingularValue).ToList ();
            NumberOfStrings = (uint) StringList.Count ();
            OriginalStringsTableOffset = 28;
            TranslationsTableOffset = OriginalStringsTableOffset + NumberOfStrings * 8;
            HashTableSize = 0; //we don't implement (optional) hash table here
            HashTableOffset = TranslationsTableOffset + NumberOfStrings * 8;

            WriteHeaderTo (Writer);
            WriteLengthsAndOffsetsTo (Writer);
            WriteHashTableTo (Writer);
            WriteMessagesTo (Writer);
        }

        List<LocalizedString> StringList { get; set; }
        uint NumberOfStrings { get; set; }
        uint OriginalStringsTableOffset { get; set; }
        uint TranslationsTableOffset { get; set; }
        uint HashTableSize { get; set; }
        uint HashTableOffset { get; set; }

        private void WriteHeaderTo (BinaryWriter writer)
        {
            //                    byte
            //             +------------------------------------------+
            //          0  | magic number = 0x950412de                |
            //             |                                          |
            //          4  | file format revision = 0                 |
            //             |                                          |
            //          8  | number of strings                        |  == N
            //             |                                          |
            //         12  | offset of table with original strings    |  == O
            //             |                                          |
            //         16  | offset of table with translation strings |  == T
            //             |                                          |
            //         20  | size of hashing table                    |  == S
            //             |                                          |
            //         24  | offset of hashing table                  |  == H
            //             |                                          |
            //             .                                          .
            //             .    (possibly more entries later)         .
            //             .                                          .
            //             |                                          |                                          

            writer.Write (0x950412de);
            writer.Write ((uint)0x0);
            writer.Write (NumberOfStrings);
            writer.Write (OriginalStringsTableOffset);
            writer.Write (TranslationsTableOffset);
            writer.Write (HashTableSize);
            writer.Write (HashTableOffset);
        }

        private void WriteLengthsAndOffsetsTo (BinaryWriter writer) 
        {
            //          O  | length & offset 0th string  ----------------.
            //      O + 8  | length & offset 1st string  ------------------.
            //              ...                                    ...   | |
            //O + ((N-1)*8)| length & offset (N-1)th string           |  | |
            //             |                                          |  | |
            //          T  | length & offset 0th translation  ---------------.
            //      T + 8  | length & offset 1st translation  -----------------.
            //              ...                                    ...   | | | |
            //T + ((N-1)*8)| length & offset (N-1)th translation      |  | | | |
            //             |                                          |  | | | |

            var offset = HashTableOffset + HashTableSize*4;
            foreach (var localized_string in StringList) {
                var length = Encoding.UTF8.GetByteCount (localized_string.UntranslatedSingularValue);
                if (!string.IsNullOrEmpty (localized_string.UntranslatedPluralValue)) {
                    length += 1 + Encoding.UTF8.GetByteCount (localized_string.UntranslatedPluralValue);
                }
                var context = GetContextAndGender (localized_string);
                if (!string.IsNullOrEmpty (context)) {
                    length += 1 + Encoding.UTF8.GetByteCount (context);
                }

                Writer.Write (length);
                writer.Write (offset);
                offset = (uint)(offset + length + 1);
            }

            foreach (var localized_string in StringList) {
                var length = localized_string.TranslatedValues.Sum (tv => Encoding.UTF8.GetByteCount(tv)) +
                             localized_string.TranslatedValues.Count () - 1;

                Writer.Write (length);
                writer.Write (offset);
                offset = (uint)(offset + length + 1);
            }
        }

        void WriteHashTableTo (BinaryWriter writer) 
        {
            //          H  | start hash table                         |  | | | |
            //              ...                                    ...   | | | |
            //  H + S * 4  | end hash table                           |  | | | |
            //             |                                          |  | | | |
        }

        void WriteMessagesTo (BinaryWriter writer) 
        {
            //             | NUL terminated 0th string  <----------------' | | |
            //             |                                          |    | | |
            //             | NUL terminated 1st string  <------------------' | |
            //             |                                          |      | |
            //              ...                                    ...       | |
            //             |                                          |      | |
            //             | NUL terminated 0th translation  <---------------' |
            //             |                                          |        |
            //             | NUL terminated 1st translation  <-----------------'
            //             |                                          |
            //              ...                                    ...
            //             |                                          |
            //             +------------------------------------------+
            foreach (var localized_string in StringList) {
                var context = GetContextAndGender (localized_string);
                if (!string.IsNullOrEmpty (context)) {
                    Writer.Write (Encoding.UTF8.GetBytes (context));
                    writer.Write ((byte)0x4);    
                }

                Writer.Write (Encoding.UTF8.GetBytes (localized_string.UntranslatedSingularValue));
                writer.Write ((byte)0x0);

                if (!string.IsNullOrEmpty (localized_string.UntranslatedPluralValue)) {
                    Writer.Write (Encoding.UTF8.GetBytes (localized_string.UntranslatedPluralValue));
                    writer.Write ((byte)0x0);
                }
            }

            foreach (var localized_string in StringList) {
                foreach (var translatedValue in localized_string.TranslatedValues)
                {
                    Writer.Write (Encoding.UTF8.GetBytes (translatedValue));
                    writer.Write ((byte)0x0);
                }
            }
        }

        string GetContextAndGender (LocalizedString localized_string)
        {
            var context = localized_string.Context;
            if (localized_string.Gender != LanguageGender.Neutral) {
                var add_gender_context = true;
                if (context != null) {
                    var context_lower = context.ToLower ();
                    foreach (var gender_context in PoParser.GenderContexts) {
                        if (context_lower.Contains (gender_context.Key)) {
                            add_gender_context = false;
                            break;
                        }
                    }
                }

                if (add_gender_context) {
                    var gender_context = localized_string.Gender.ToString () + " form";
                    if (String.IsNullOrEmpty (context)) {
                        context = gender_context;
                    } else {
                        context += ", " + gender_context;
                    }
                }
            }
            return context;
        }
    }
}
