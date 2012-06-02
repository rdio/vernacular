// 
// HeaderCollection.cs
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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Vernacular.Potato
{
    public sealed class HeaderCollection : Container<Header>, INotifyCollectionChanged, IList<Header>
    {
        private ObservableCollection<Header> headers = new ObservableCollection<Header> ();

        public override event NotifyCollectionChangedEventHandler CollectionChanged {
            add { headers.CollectionChanged += value; }
            remove { headers.CollectionChanged -= value; }
        }

        private Header Find (Header header)
        {
            int index;
            return Find (header.Name, out index);
        }

        private Header Find (string header)
        {
            int index;
            return Find (header, out index);
        }

        private Header Find (Header header, out int index)
        {
            return Find (header.Name, out index);
        }

        private Header Find (string headerName, out int index)
        {
            int i = 0;
            foreach (var header in this) {
                if (String.Equals (header.Name, headerName, StringComparison.InvariantCultureIgnoreCase)) {
                    index = i;
                    return header;
                }
                i++;
            }

            index = -1;
            return null;
        }

        public Header this [int index] {
            get { return headers [index]; }
            set { headers [index] = value; }
        }

        public string this [string headerName] {
            get {
                var header = Find (headerName);
                return header == null ? null : header.Value;
            }

            set {
                var header = Find (headerName);
                if (header == null) {
                    Add (headerName, value);
                } else {
                    header.Value = value;
                }
            }
        }

        public void Insert (int index, Header header)
        {
            if (Contains (header)) {
                throw new Exception ("header already exists");
            }

            headers.Insert (index, header);
        }

        public int IndexOf (Header header)
        {
            int index;
            Find (header, out index);
            return index;
        }

        public int IndexOf (string headerName)
        {
            int index;
            Find (headerName, out index);
            return index;
        }

        public void Add (string name, string value)
        {
            Add (new Header { Name = name, Value = value });
        }

        public void Add (Header header)
        {
            if (Contains (header)) {
                throw new Exception ("header already exists; use this[string] to replace");
            }

            headers.Add (header);
        }

        public bool Remove (Header header)
        {
            var found = Find (header);
            if (found == null) {
                return false;
            }

            return headers.Remove (found);
        }

        public bool Remove (string headerName)
        {
            var found = Find (headerName);
            if (found == null) {
                return false;
            }

            return headers.Remove (found);
        }

        public void RemoveAt (int index)
        {
            headers.RemoveAt (index);
        }

        public bool Contains (Header header)
        {
            return Find (header) != null;
        }

        public bool Contains (string headerName)
        {
            return Find (headerName) != null;
        }

        public void Clear ()
        {
            headers.Clear ();
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public int Count {
            get { return headers.Count; }
        }

        public void CopyTo (Header [] array, int index)
        {
            headers.CopyTo (array, index);
        }

        public override string Generate ()
        {
            var builder = new StringBuilder ();

            foreach (var header in headers) {
                if (header.HasValue) {
                    builder.Append (header.Generate ());
                    builder.Append ('\n');
                }
            }

            if (builder.Length > 0) {
                builder.Length--;
            }

            return builder.ToString ();
        }

        public override IEnumerator<Header> GetEnumerator ()
        {
            foreach (var header in headers) {
                yield return header;
            }
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator ();
        }
    }
}