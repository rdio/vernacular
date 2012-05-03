//
// CecilExtensions.cs
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
