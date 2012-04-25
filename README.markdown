Vernacular
==========

Vernacular is a localization tool for developers. It currently is focused on
providing a unified localization system for
[MonoTouch](http://xamarin.com/monotouch),
[Mono for Android](http://xamarin.com/monoforandroid), and
Windows Phone.

Vernacular consists of two primary components:

* a tool for extracting strings and generating resource files
* a small library that applications can use to read localized strings

Why?
----

At [Rdio](http://www.rdio.com), our mobile applications share a common
C#/.NET core, but we ran into countless problems with sharing localized
strings across the applications. We developed Vernacular to solve this
problem.

Localization support is fairly poor and inconsistent across the dominant
mobile phone platforms. For instance, Android and Windows Phone make
localization a huge chore with generated code, naming of resources, and
converters, and iPhone doesn't support plurals (providing only
`NSLocalizedString(message)`).

Vernacular solves this by providing a
[gettext-inspired](http://www.gnu.org/software/gettext) API for localizing
strings directly within application code. It supports plurals and even
genders.

Not Complete
------------

While Vernacular is currently very useful, it is still under development.
There are certainly some bugs, and this documentation is nowhere near
complete.

License
-------

Vernacular is licensed under the
[MIT X11 license](http://www.github.com/rdio/vernacular/LICENSE).
Copyright 2012 Rdio, Inc.
