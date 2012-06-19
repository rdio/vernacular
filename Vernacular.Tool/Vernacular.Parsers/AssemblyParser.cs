//
// AssemblyParser.cs
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
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Vernacular.Analyzers;
using Vernacular.Tool;

namespace Vernacular.Parsers
{
    public sealed class AssemblyParser : Parser
    {
        private static readonly List<string> LOCALIZATION_TYPES = new List<string> {
            "Vernacular.Catalog",
            "Vernacular.Xaml.Catalog"
        };

        private static readonly List<string> LOCALIZATION_METHODS = new List<string> {
            "GetString",
            "GetPluralString",
            "GetGenderString",
            "GetPluralGenderString"
        };

        private List<ModuleDefinition> modules = new List<ModuleDefinition> ();
        private List<MethodDefinition> localization_methods = new List<MethodDefinition> ();

        public override IEnumerable<string> SupportedFileExtensions {
            get {
                yield return ".exe";
                yield return ".dll";
            }
        }
        
        private void Add (ModuleDefinition module)
        {
            try
            {
                module.ReadSymbols();
            }
            catch (FileNotFoundException)
            {
            }
            catch (InvalidOperationException)
            {                
            }
            modules.Add (module);
            LocateLocalizationMethods (module);
        }

        public override void Add (string path)
        {
            var module = ModuleDefinition.ReadModule (path);
            Add (module);
        }

        public override void Add (Stream stream, string path)
        {
            var module = ModuleDefinition.ReadModule (stream);
            Add (module);
        }

        public override IEnumerable<ILocalizationUnit> Parse ()
        {
            return
                from module in modules
                from localization_unit in Parse (module)
                select localization_unit;
        }

        private IEnumerable<LocalizedString> Parse (ModuleDefinition module)
        {
            return
                from type in module.Types
                where !LOCALIZATION_TYPES.Contains (type.FullName)
                from method in type.Methods
                where method.HasBody && method.Body.Instructions.Count > 0
                from invocation in ParseLocalizedStringInvocations (method.Body.Instructions [0])
                from localized_string in ParseLocalizedStringInvocation (invocation)
                select localized_string;
        }

        private void LocateLocalizationMethods (ModuleDefinition module)
        {
            localization_methods.AddRange (
                from type in module.Types
                where LOCALIZATION_TYPES.Contains (type.FullName)
                from method in type.Methods
                where method.IsStatic && method.IsPublic && method.HasBody &&
                    LOCALIZATION_METHODS.Contains (method.Name)
                select method
            );
        }

        private MethodDefinition ResolveLocalizationMethodReference (MethodReference methodReference)
        {
            if (methodReference == null) {
                return null;
            }

            return localization_methods.Find (method_definition => {
                if (method_definition.FullName != methodReference.FullName ||
                    method_definition.ReturnType.FullName != methodReference.ReturnType.FullName ||
                    !method_definition.HasParameters || !methodReference.HasParameters ||
                    method_definition.Parameters.Count != methodReference.Parameters.Count) {
                    return false;
                }

                for (int i = 0; i < method_definition.Parameters.Count; i++) {
                    if (method_definition.Parameters [i].ParameterType.FullName !=
                        methodReference.Parameters [i].ParameterType.FullName) {
                        return false;
                    }
                }

                return true;
            });
        }

        private bool RetainInstruction (Instruction instruction, Dictionary<string, Instruction> memory)
        {
            return instruction.OpCode == OpCodes.Ldstr || instruction.OpCode == OpCodes.Ldnull;
        }

        private class LocalizedStringInvocation
        {
            public List<Instruction> Instructions;
            public List<Instruction> AllInstructions;
            public SequencePoint SequencePoint;
            public MethodDefinition Method;
        }

        private IEnumerable<LocalizedStringInvocation> ParseLocalizedStringInvocations (Instruction startInstruction)
        {
            SequencePoint last_sequence_point = null;
            var memory = new Dictionary<string, Instruction> ();
            var retained_instructions = new List<Instruction> ();
            var all_instructions = new List<Instruction> ();
            var instruction = startInstruction;

            while (instruction != null) {
                all_instructions.Add (instruction);

                if (instruction.SequencePoint != null) {
                    last_sequence_point = instruction.SequencePoint;
                }

                string location;
                if (instruction.OpCode != OpCodes.Call &&
                    instruction.Next != null &&
                    instruction.Next.IsStoreInstruction (out location)) {
                    memory [location] = instruction;
                    instruction = instruction.Next.Next;
                    continue;
                } else if (instruction.IsLoadInstruction (out location)) {
                    Instruction stored_instruction;
                    if (memory.TryGetValue (location, out stored_instruction)) {
                        memory.Remove (location);
                        if (RetainInstruction (stored_instruction, memory)) {
                            retained_instructions.Add (stored_instruction);
                        }
                    }
                } else if (RetainInstruction (instruction, memory)) {
                    retained_instructions.Add (instruction);
                } else if (instruction.OpCode == OpCodes.Call) {
                    var localization_call = ResolveLocalizationMethodReference (instruction.Operand as MethodReference);
                    if (localization_call != null) {
                        yield return new LocalizedStringInvocation {
                            Instructions = new List<Instruction> (retained_instructions),
                            AllInstructions = new List<Instruction> (all_instructions),
                            SequencePoint = last_sequence_point,
                            Method = localization_call
                        };

                        memory.Clear ();
                        retained_instructions.Clear ();
                        all_instructions.Clear ();
                        last_sequence_point = null;
                    }
                }

                instruction = instruction.Next;
            }
        }

        private IEnumerable<LocalizedString> ParseLocalizedStringInvocation (LocalizedStringInvocation invocation)
        {
            var strings = new Stack<KeyValuePair<string, string>> ();

            Log ("+ {0}", invocation.Method);
            if (invocation.SequencePoint != null) {
                Log ("  @ {0}:{1}", RelativeDocumentUrl (invocation.SequencePoint.Document.Url),
                    invocation.SequencePoint.StartLine);
            }

            foreach (var instruction in invocation.AllInstructions) {
                Log (true, "  | {0}", instruction);
            }

            Log (true, "  |".PadRight (70, '-'));

            int i = invocation.Instructions.Count;
            foreach (var param in invocation.Method.Parameters.Reverse ()) {
                if (param.ParameterType.FullName == "System.String" && i > 0) {
                    var instruction = invocation.Instructions [--i];
                    if (instruction.OpCode == OpCodes.Ldnull || instruction.OpCode == OpCodes.Ldstr) {
                        strings.Push (new KeyValuePair<string, string> (
                            param.Name,
                            instruction.Operand as string
                        ));
                    }

                    Log ("  | [{0}]: {1}", param.Name, instruction);
                }
            }

            // Detect if the GetString call is nested (an argument to) in a
            // String.Format call, which is a warning (Catalog.Format should be
            // used since it will never throw an exception).
            var call_instruction = invocation.AllInstructions [invocation.AllInstructions.Count - 1];
            var is_string_format = false;

            if (call_instruction.Next != null &&
                call_instruction.Next.Next != null &&
                call_instruction.Next.OpCode == OpCodes.Ldarg_0 &&
                call_instruction.Next.Next.OpCode == OpCodes.Call) {
                var string_format_call = call_instruction.Next.Next.Operand as MethodReference;
                if (string_format_call != null &&
                    string_format_call.DeclaringType.FullName == "System.String" &&
                    string_format_call.Name == "Format") {
                    is_string_format = true;
                }
            }

            Log (true, "  |".PadRight (70, '-'));

            foreach (var @string in GenerateLocalizedStrings (invocation.SequencePoint, strings,
                invocation.Method.Name.Contains("Gender"))) {
                Log ("  | {0}", @string);
                if (is_string_format) {
                    @string.Warnings.Add ("String.Format is unsafe - use Catalog.Format instead");
                }
                yield return @string;
            }

            Log ();
        }

        private IEnumerable<LocalizedString> GenerateLocalizedStrings (SequencePoint sequencePoint,
            Stack<KeyValuePair<string, string>> parameters, bool gendered)
        {
            var neutral = CreateLocalizedString (LanguageGender.Neutral, sequencePoint);
            var masculine = CreateLocalizedString (LanguageGender.Masculine, sequencePoint);
            var feminine = CreateLocalizedString (LanguageGender.Feminine, sequencePoint);

            while (parameters.Count > 0) {
                var param = parameters.Pop ();
                switch (param.Key) {
                    case "comment":
                        neutral.DeveloperComments = param.Value;
                        masculine.DeveloperComments = param.Value;
                        feminine.DeveloperComments = param.Value;
                        break;
                    case "message":
                    case "singularMessage":
                        if (gendered) {
                            masculine.UntranslatedSingularValue = param.Value;
                            feminine.UntranslatedSingularValue = param.Value;
                        } else {
                            neutral.UntranslatedSingularValue = param.Value;
                        }
                        break;
                    case "pluralMessage":
                        if (gendered) {
                            masculine.UntranslatedPluralValue = param.Value;
                            feminine.UntranslatedPluralValue = param.Value;
                        } else {
                            neutral.UntranslatedPluralValue = param.Value;
                        }
                        break;
                    case "masculineMessage":
                    case "singularMasculineMessage":
                        masculine.UntranslatedSingularValue = param.Value;
                        break;
                    case "pluralMasculineMessage":
                        masculine.UntranslatedPluralValue = param.Value;
                        break;
                    case "feminineMessage":
                    case "singularFeminineMessage":
                        feminine.UntranslatedSingularValue = param.Value;
                        break;
                    case "pluralFeminineMessage":
                        feminine.UntranslatedPluralValue = param.Value;
                        break;
                }
            }

            if (neutral.IsDefined) {
                yield return neutral;
            }

            if (masculine.IsDefined) {
                yield return masculine;
            }

            if (feminine.IsDefined) {
                yield return feminine;
            }
        }

        private LocalizedString CreateLocalizedString (LanguageGender gender, SequencePoint sequencePoint)
        {
            var localized_string = new LocalizedString { Gender = gender };

            if (sequencePoint != null) {
                localized_string.AddReference (RelativeDocumentUrl (sequencePoint.Document.Url), sequencePoint.StartLine);
            }

            if (StringAnalyzer.CheckFormatArguments (localized_string.UntranslatedSingularValue) ||
                StringAnalyzer.CheckFormatArguments (localized_string.UntranslatedPluralValue)) {
                localized_string.StringFormatHint = "csharp-format";
            }

            return localized_string;
        }
    }
}
