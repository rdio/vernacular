// 
// Document.cs
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
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Vernacular.Potato.Internal;

namespace Vernacular.Potato
{
    public sealed class Document : Container<Unit>
    {
        private ObservableCollection<Unit> units = new ObservableCollection<Unit> ();

        public override event NotifyCollectionChangedEventHandler CollectionChanged {
            add { units.CollectionChanged += value; }
            remove { units.CollectionChanged -= value; }
        }

        public void Add (Unit unit)
        {
            units.Add (unit);
        }

        public void Add (params Unit [] units)
        {
            Add ((IEnumerable<Unit>)units);
        }

        public void Add (IEnumerable<Unit> units)
        {
            foreach (var unit in units) {
                Add (unit);
            }
        }

        public void Load (string path)
        {
            Add (new Parser ().Parse (path));
        }

        public void Load (StreamReader reader, string documentName = null)
        {
            Add (new Parser ().Parse (reader, documentName));
        }

        public override string Generate ()
        {
            var builder = new StringBuilder ();

            foreach (var part in this) {
                builder.Append (part.Generate ());
                builder.Append ("\n\n");
            }

            builder.Length--;

            return builder.ToString ();
        }

        public override IEnumerator<Unit> GetEnumerator ()
        {
            foreach (var unit in units) {
                if (unit.HasValue) {
                    yield return unit;
                }
            }
        }
    }
}