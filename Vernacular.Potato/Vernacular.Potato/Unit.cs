// 
// Unit.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Vernacular.Potato
{
    public sealed class Unit : Container<IDocumentPart>
    {
        private ObservableCollection<Comment> comments = new ObservableCollection<Comment> ();
        private ObservableCollection<Message> messages = new ObservableCollection<Message> ();

        private NotifyCollectionChangedEventHandler collection_changed;

        public override event NotifyCollectionChangedEventHandler CollectionChanged {
            add {
                var listen = collection_changed == null;
                collection_changed += value;
                if (collection_changed != null && listen) {
                    comments.CollectionChanged += OnCollectionChanged;
                    messages.CollectionChanged += OnCollectionChanged;
                }
            }

            remove {
                collection_changed -= value;
                if (collection_changed == null) {
                    comments.CollectionChanged -= OnCollectionChanged;
                    messages.CollectionChanged -= OnCollectionChanged;
                }
            }
        }

        private void OnCollectionChanged (object sender, NotifyCollectionChangedEventArgs args)
        {
            var handler = collection_changed;
            if (handler != null) {
                handler (this, args);
            }
        }

        public IEnumerable<Comment> Comments {
            get { return comments; }
        }

        public IEnumerable<Message> Messages {
            get { return messages; }
        }

        public void Add (Comment comment)
        {
            comments.Add (comment);
        }

        public void Add (params Comment [] comments)
        {
            Add ((IEnumerable<Comment>)comments);
        }

        public void Add (IEnumerable<Comment> comments)
        {
            foreach (var comment in comments) {
                Add (comment);
            }
        }

        public void Add (Message message)
        {
            messages.Add (message);
        }

        public void Add (params Message [] messages)
        {
            Add ((IEnumerable<Message>)messages);
        }

        public void Add (IEnumerable<Message> messages)
        {
            foreach (var message in messages) {
                Add (message);
            }
        }

        public override string Generate ()
        {
            var builder = new StringBuilder ();

            foreach (var part in this) {
                builder.Append (part.Generate ());
                builder.Append ("\n");
            }

            if (builder.Length > 0) {
                builder.Length--;
            }

            return builder.ToString ();
        }

        public override IEnumerator<IDocumentPart> GetEnumerator ()
        {
            foreach (var comment in comments) {
                if (comment.HasValue) {
                    yield return comment;
                }
            }

            foreach (var message in messages) {
                if (message.HasValue) {
                    yield return message;
                }
            }
        }
    }
}