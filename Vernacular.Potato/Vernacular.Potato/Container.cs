// 
// Container.cs
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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Vernacular.Potato
{
    public abstract class Container<TDocumentPart> : IDocumentPart, IEnumerable<TDocumentPart>,
        INotifyCollectionChanged where TDocumentPart : IDocumentPart
    {
        internal Container ()
        {
        }

        public abstract event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasValue {
            get { return FirstChildDocumentPart != null; }
        }

        protected TDocumentPart FirstChildDocumentPart {
            get {
                foreach (TDocumentPart part in this) {
                    return part;
                }

                return default (TDocumentPart);
            }
        }

        public int Line {
            get {
                var part = FirstChildDocumentPart;
                return part == null ? 0 : part.Line;
            }
        }

        public int Column {
            get {
                var part = FirstChildDocumentPart;
                return part == null ? 0 : part.Column;
            }
        }

        public abstract string Generate ();
        public abstract IEnumerator<TDocumentPart> GetEnumerator ();

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator ();
        }
    }
}