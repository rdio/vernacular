using System;

using Mono.Cecil.Cil;

namespace Vernacular.Parsers
{
    internal static class CecilExtensions
    {
        public static bool IsStoreInstruction (this Instruction insruction, out string location)
        {
            location = null;

            if (insruction == null) {
                return false;
            } else if (insruction.OpCode == OpCodes.Stloc) {
                location = insruction.Operand.ToString ();
            } else if (insruction.OpCode == OpCodes.Stloc_0) {
                location = "0";
            } else if (insruction.OpCode == OpCodes.Stloc_1) {
                location = "1";
            } else if (insruction.OpCode == OpCodes.Stloc_2) {
                location = "2";
            } else if (insruction.OpCode == OpCodes.Stloc_3) {
                location = "3";
            } else if (insruction.OpCode == OpCodes.Stloc_S) {
                location = "S";
            } else {
                return false;
            }

            return true;
        }

        public static bool IsLoadInstruction (this Instruction insruction, out string location)
        {
            location = null;

            if (insruction == null) {
                return false;
            } else if (insruction.OpCode == OpCodes.Ldloc) {
                location = insruction.Operand.ToString ();
            } else if (insruction.OpCode == OpCodes.Ldloc_0) {
                location = "0";
            } else if (insruction.OpCode == OpCodes.Ldloc_1) {
                location = "1";
            } else if (insruction.OpCode == OpCodes.Ldloc_2) {
                location = "2";
            } else if (insruction.OpCode == OpCodes.Ldloc_3) {
                location = "3";
            } else if (insruction.OpCode == OpCodes.Ldloc_S) {
                location = "S";
            } else {
                return false;
            }

            return true;
        }
    }
}
