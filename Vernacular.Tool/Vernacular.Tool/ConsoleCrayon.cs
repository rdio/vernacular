//
// ConsoleCrayon.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright 2008-2010 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Vernacular
{
    public static class ConsoleCrayon
    {
        [Flags]
        public enum TextStyle
        {
            None = 0,
            Bold = 1 << 0,
            Italic = 1 << 1,
            Underline = 1 << 2,
            Inverse = 1 << 3,
            Strike = 1 << 4
        }

        private static int [] ansi_text_style_control_codes = new int [] {
            1, 22,
            3, 23,
            4, 24,
            7, 27,
            9, 29
        };

#region Public API

        private static TextStyle style;
        public static TextStyle Style {
            get { return style; }
            set {
                if (!XtermColors) {
                    return;
                }

                int i = 0;
                foreach (var style_test in new [] {
                    TextStyle.Bold,
                    TextStyle.Italic,
                    TextStyle.Underline,
                    TextStyle.Inverse,
                    TextStyle.Strike
                }) {
                    if ((style & style_test) == 0 && (value & style_test) != 0) {
                        WriteAnsi (ansi_text_style_control_codes[i * 2]);
                    } else if ((style & style_test) != 0 && (value & style_test) == 0) {
                        WriteAnsi (ansi_text_style_control_codes[i * 2 + 1]);
                    }

                    i++;
                }

                style = value;
            }
        }

        private static ConsoleColor foreground_color;
        public static ConsoleColor ForegroundColor {
            get { return foreground_color; }
            set {
                foreground_color = value;
                SetColor (foreground_color, true);
            }
        }

        private static ConsoleColor background_color;
        public static ConsoleColor BackgroundColor {
            get { return background_color; }
            set {
                background_color = value;
                SetColor (background_color, false);
            }
        }

        public static void Reset ()
        {
            if (XtermColors) {
                style = TextStyle.None;
                WriteAnsi (0);
            } else if (Environment.OSVersion.Platform != PlatformID.Unix && !RuntimeIsMono) {
                Console.ResetColor ();
            }
        }

        private static void SetColor (ConsoleColor color, bool isForeground)
        {
            if (color < ConsoleColor.Black || color > ConsoleColor.White) {
                throw new ArgumentOutOfRangeException ("color", "Not a ConsoleColor value.");
            }

            if (XtermColors) {
                Console.Write (GetAnsiColorControlCode (color, isForeground));
            } else if (Environment.OSVersion.Platform != PlatformID.Unix && !RuntimeIsMono) {
                if (isForeground) {
                    Console.ForegroundColor = color;
                } else {
                    Console.BackgroundColor = color;
                }
            }
        }

#endregion

#region Ansi/VT Code Calculation

        // Modified from Mono's System.TermInfoDriver
        // License: MIT/X11
        // Authors: Gonzalo Paniagua Javier <gonzalo@ximian.com>
        // (C) 2005-2006 Novell, Inc <http://www.novell.com>

        private static int TranslateColor (ConsoleColor desired, out bool light)
        {
            light = false;
            switch (desired) {
                // Dark colors
                case ConsoleColor.Black: return 0;
                case ConsoleColor.DarkRed: return 1;
                case ConsoleColor.DarkGreen: return 2;
                case ConsoleColor.DarkYellow: return 3;
                case ConsoleColor.DarkBlue: return 4;
                case ConsoleColor.DarkMagenta: return 5;
                case ConsoleColor.DarkCyan: return 6;
                case ConsoleColor.Gray: return 7;

                // Light colors
                case ConsoleColor.DarkGray: light = true; return 0;
                case ConsoleColor.Red: light = true; return 1;
                case ConsoleColor.Green: light = true; return 2;
                case ConsoleColor.Yellow: light = true; return 3;
                case ConsoleColor.Blue: light = true; return 4;
                case ConsoleColor.Magenta: light = true; return 5;
                case ConsoleColor.Cyan: light = true; return 6;
                case ConsoleColor.White: default: light = true; return 7;
            }
        }

        private static string GetAnsiColorControlCode (ConsoleColor color, bool isForeground)
        {
            // lighter fg colors are 90 -> 97 rather than 30 -> 37
            // lighter bg colors are 100 -> 107 rather than 40 -> 47
            bool light;
            return GetAnsi (TranslateColor (color, out light) + (isForeground ? 30 : 40) + (light ? 60 : 0));
        }

        private static string GetAnsi (int controlCode)
        {
            return String.Format ("\x001b[{0}m", controlCode);
        }

        private static void WriteAnsi (int controlCode)
        {
            Console.Write (GetAnsi (controlCode));
        }

#endregion

#region xterm Detection

        private static bool? xterm_colors = null;
        public static bool XtermColors {
            get {
                if (xterm_colors == null) {
                    DetectXtermColors ();
                }

                return xterm_colors.Value;
            }
        }

        [System.Runtime.InteropServices.DllImport ("libc", EntryPoint="isatty")]
        private extern static int _isatty (int fd);

        private static bool isatty (int fd)
        {
            try {
                return _isatty (fd) == 1;
            } catch {
                return false;
            }
        }

        private static void DetectXtermColors ()
        {
            bool _xterm_colors = false;

            switch (Environment.GetEnvironmentVariable ("TERM")) {
                case "xterm":
                case "rxvt":
                case "rxvt-unicode":
                    if (Environment.GetEnvironmentVariable ("COLORTERM") != null) {
                        _xterm_colors = true;
                    }
                    break;
                case "xterm-color":
                    _xterm_colors = true;
                    break;
            }

            xterm_colors = _xterm_colors && isatty (1) && isatty (2);
        }

#endregion

#region Runtime Detection

        private static bool? runtime_is_mono;
        public static bool RuntimeIsMono {
            get {
                if (runtime_is_mono == null) {
                    runtime_is_mono = Type.GetType ("System.MonoType") != null;
                }

                return runtime_is_mono.Value;
            }
        }

#endregion

#region Tests

        public static void Test ()
        {
            TestSelf ();
            Console.WriteLine ();
            TestAnsi ();
            Console.WriteLine ();
            TestRuntime ();
            Console.WriteLine ();
            TestAnsiStyles ();
        }

        private static void TestSelf ()
        {
            Console.WriteLine ("==SELF TEST==");
            foreach (ConsoleColor color in Enum.GetValues (typeof (ConsoleColor))) {
                ForegroundColor = color;
                Console.Write (color);
                Reset ();
                Console.Write (" :: ");
                BackgroundColor = color;
                Console.Write (color);
                Reset ();
                Console.WriteLine ();
            }
        }

        private static void TestAnsi ()
        {
            Console.WriteLine ("==ANSI TEST==");
            foreach (ConsoleColor color in Enum.GetValues (typeof (ConsoleColor))) {
                string color_code_fg = GetAnsiColorControlCode (color, true);
                string color_code_bg = GetAnsiColorControlCode (color, false);
                Console.Write ("{0}{1}: {2}{3} :: {4}{1}: {5}{3}", color_code_fg, color, color_code_fg.Substring (2),
                    GetAnsi (0), color_code_bg, color_code_bg.Substring (2));
                Console.WriteLine ();
            }
        }

        private static void TestRuntime ()
        {
            Console.WriteLine ("==RUNTIME TEST==");
            foreach (ConsoleColor color in Enum.GetValues (typeof (ConsoleColor))) {
                Console.ForegroundColor = color;
                Console.Write (color);
                Console.ResetColor ();
                Console.Write (" :: ");
                Console.BackgroundColor = color;
                Console.Write (color);
                Console.ResetColor ();
                Console.WriteLine ();
            }
        }

        private static void TestAnsiStyles ()
        {
            foreach (var style in new [] {
                TextStyle.Bold,
                TextStyle.Italic,
                TextStyle.Strike,
                TextStyle.Underline,
                TextStyle.Inverse
            }) {
                Style = style;
                Console.Write (Style);
                Reset ();
                Console.Write (" ");
            }

            Console.WriteLine ();

            Console.Write ("None  ");
            Style = TextStyle.Bold;
            Console.Write ("Bold  ");
            Style |= TextStyle.Strike;
            Console.Write ("BoldStrike  ");
            Style |= TextStyle.Underline;
            Console.Write ("BoldStrikeUnderline  ");
            Style |= TextStyle.Inverse;
            Console.Write ("BoldStrikeUnderlineInverse");
            Console.WriteLine ();
            Style &= ~TextStyle.Inverse;
            Console.Write ("BoldStrikeUnderline  ");
            Style &= ~TextStyle.Underline;
            Console.Write ("BoldStrike  ");
            Style &= ~TextStyle.Strike;
            Console.Write ("Bold  ");
            Style &= ~TextStyle.Bold;
            Console.Write ("None");

            Console.WriteLine ();
        }

#endregion

    }
}
