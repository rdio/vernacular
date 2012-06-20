// 
// Hunspell.cs
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
using System.Runtime.InteropServices;

namespace Vernacular.Analyzers
{
    // Note: this is a very lazy binding not meant to be used
    // in a non-tool-like application. No attempt is made to
    // destroy the native instance and an IntPtr is used for
    // the native handle (as opposed to a more correct SafeHandle
    // or HandleRef). Probably should do proper string marshalling
    // as well (UTF-8).
    public class Hunspell
    {
#if WIN32
        private const string LIBHUNSPELL = "libhunspell";
#else
        private const string LIBHUNSPELL = "libhunspell-1.2";
#endif

#if WIN32
        [DllImport (LIBHUNSPELL, CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport (LIBHUNSPELL)]
#endif
        private static extern IntPtr Hunspell_create (string affpath, string dicpath);

#if WIN32
        [DllImport (LIBHUNSPELL, CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport (LIBHUNSPELL)]
#endif
        private static extern int Hunspell_spell (IntPtr handle, string word);

#if WIN32
        [DllImport (LIBHUNSPELL, CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport (LIBHUNSPELL)]
#endif
        private static extern int Hunspell_add (IntPtr handle, string word);

        private IntPtr handle;

        public Hunspell (string affpath, string dicpath)
        {
            handle = Hunspell_create (affpath, dicpath);
        }

        public bool SpellCheckWord (string word)
        {
            return Hunspell_spell (handle, word) != 0;
        }

        public bool AddWordToDictionary (string word)
        {
            return Hunspell_add (handle, word) != 0;
        }
    }
}