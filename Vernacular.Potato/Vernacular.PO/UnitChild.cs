// 
// UnitChild.cs
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

namespace Vernacular.Potato
{
    public abstract class UnitChild : IDocumentPart, INotifyPropertyChanged
    {
        internal UnitChild ()
        {
        }

        public abstract bool HasValue { get; }

        private int line;
        public int Line {
            get { return line; }
            set {
                if (line != value) {
                    line = value;
                    NotifyPropertyChanged ("Line");
                }
            }
        }

        private int column;
        public int Column {
            get { return column; }
            set {
                if (column != value) {
                    column = value;
                    NotifyPropertyChanged ("Column");
                }
            }
        }

        private string value;
        public string Value {
            get { return value; }
            set {
                if (this.value != value) {
                    this.value = value;
                    NotifyPropertyChanged ("Value");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged (string property)
        {
            var handler = PropertyChanged;
            if (handler != null) {
                handler (this, new PropertyChangedEventArgs (property));
            }
        }

        public abstract string Generate ();
    }
}