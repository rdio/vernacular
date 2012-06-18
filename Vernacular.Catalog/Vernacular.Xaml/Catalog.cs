//
// Catalog.cs
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

#if WINDOWS_PHONE || SILVERLIGHT

using System;
using System.Globalization;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

#if WINDOWS_PHONE
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
#endif

namespace Vernacular.Xaml
{
    public enum StringModifier
    {
        None,
        Lowercase,
        Uppercase
    }

    public static class Catalog
    {
        public static Func<FrameworkElement, DependencyProperty> FindMessageDependencyProperty { get; set; }

        public static readonly DependencyProperty CommentProperty = DependencyProperty.RegisterAttached (
            "Comment",
            typeof (string),
            typeof (Catalog),
            null);

        public static string GetComment (DependencyObject o)
        {
            return o.GetValue (CommentProperty) as string;
        }

        public static void SetComment (DependencyObject o, string value)
        {
            o.SetValue (CommentProperty, value);
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.RegisterAttached (
            "Message",
            typeof (string),
            typeof (Catalog),
            new PropertyMetadata (OnPropertyChanged));

        public static string GetMessage (DependencyObject o)
        {
            return o.GetValue (MessageProperty) as string;
        }

        public static void SetMessage (DependencyObject o, string value)
        {
            o.SetValue (MessageProperty, value);
        }

        public static readonly DependencyProperty PluralMessageProperty = DependencyProperty.RegisterAttached (
            "PluralMessage",
            typeof (string),
            typeof (Catalog),
            new PropertyMetadata (OnPropertyChanged));

        public static string GetPluralMessage (DependencyObject o)
        {
            return o.GetValue (PluralMessageProperty) as string;
        }

        public static void SetPluralMessage (DependencyObject o, string value)
        {
            o.SetValue (PluralMessageProperty, value);
        }

        public static readonly DependencyProperty PluralCountProperty = DependencyProperty.RegisterAttached (
            "PluralCount",
            typeof (int),
            typeof (Catalog),
            new PropertyMetadata (1, OnPropertyChanged));

        public static int GetPluralCount (DependencyObject o)
        {
            return (int)o.GetValue (PluralCountProperty);
        }

        public static void SetPluralCount (DependencyObject o, int value)
        {
            o.SetValue (PluralCountProperty, value);
        }

        public static readonly DependencyProperty ModifierProperty = DependencyProperty.RegisterAttached (
            "Modifier",
            typeof (StringModifier),
            typeof (Catalog),
            new PropertyMetadata (StringModifier.None, OnPropertyChanged));

        public static StringModifier GetModifier (DependencyObject o)
        {
            return (StringModifier)o.GetValue (ModifierProperty);
        }

        public static void SetModifier (DependencyObject o, StringModifier value)
        {
            o.SetValue (ModifierProperty, value);
        }

        private static void OnPropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var framework_element = sender as FrameworkElement;
            if (framework_element == null) {
                return;
            }

            var message = sender.GetValue (MessageProperty) as string;
            var plural_message = sender.GetValue (PluralMessageProperty) as string;
            var modifier = (StringModifier)sender.GetValue (ModifierProperty);
            var count = 1;

            if (message == null) {
                return;
            } else if (plural_message != null) {
                count = (int)sender.GetValue (PluralCountProperty);
            }

            var localized = plural_message == null
                ? Vernacular.Catalog.GetString (message)
                : Vernacular.Catalog.GetPluralString (message, plural_message, count);

            localized = ModifyString (localized, modifier);
            localized = Format(localized, count);
            var property = FindMessageProperty (framework_element);
            if (property != null) {
                framework_element.SetValue (property, localized);
            }
        }

        private static DependencyProperty FindMessageProperty (FrameworkElement e)
        {
            if (FindMessageDependencyProperty != null) {
                var property = FindMessageDependencyProperty (e);
                if (property != null) {
                    return property;
                }
            }

            if (e is TextBlock) {
                return TextBlock.TextProperty;
            } else if (e is Button) {
                return Button.ContentProperty;
            } else if (e is CheckBox) {
                return Button.ContentProperty;
#if WINDOWS_PHONE
            } else if (e is PanoramaItem) {
                return PanoramaItem.HeaderProperty;
            } else if (e is Pivot) {
                return Pivot.TitleProperty;
            } else if (e is PivotItem) {
                return PivotItem.HeaderProperty;
            } else if (e is ListPicker) {
                return ListPicker.HeaderProperty;
#endif
            } else if (e is TextBox) {
                return TextBox.TextProperty;
            }

            return null;
        }

        private static string ModifyString (string value, string modifier)
        {
            switch (modifier) {
                case "lowercase": return ModifyString (value, StringModifier.Lowercase);
                case "uppercase": return ModifyString (value, StringModifier.Uppercase);
                default: return value;
            }
        }

        public static string ModifyString (string value, StringModifier modifier)
        {
            // German shouldn't convert to lowercase and uppercase.
            if (modifier == StringModifier.None || CultureInfo.CurrentCulture.Name == "de-DE") {
                return value;
            }

            switch (modifier) {
                case StringModifier.Lowercase: return value.ToLower ();
                case StringModifier.Uppercase: return value.ToUpper ();
                default: return value;
            }
        }

        public static string Format (string format, params object [] args)
        {
            return Vernacular.Catalog.Format (format, args);
        }

        public static string GetString (string message,
            StringModifier modifier = StringModifier.None,
            string comment = null)
        {
            return ModifyString (Vernacular.Catalog.GetString (message), modifier);
        }

        public static string GetPluralString (string singularMessage, string pluralMessage, int n,
            StringModifier modifier = StringModifier.None,
            string comment = null)
        {
            return ModifyString (Vernacular.Catalog.GetPluralString (singularMessage, pluralMessage, n), modifier);
        }

#if WINDOWS_PHONE

        public static void Localize (this IApplicationBar applicationBar)
        {
            if (applicationBar == null) {
                return;
            }

            if (applicationBar.Buttons != null) {
                foreach (ApplicationBarIconButton button in applicationBar.Buttons) {
                    button.Text = GetString (button.Text, StringModifier.Lowercase);
                }
            }

            if (applicationBar.MenuItems != null) {
                foreach (ApplicationBarMenuItem menu_item in applicationBar.MenuItems) {
                    menu_item.Text = GetString (menu_item.Text, StringModifier.Lowercase);
                }
            }
        }

#endif

    }
}

#endif