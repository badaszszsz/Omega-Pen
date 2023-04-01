# Omega-Pen
March 2023 - works on Windows 11

Roadmap:
* port to MacOS
* port to Linux
* add Settings for hotkeys
* make better handling of color changer

This is a forked version of fixed Epic Pen called Omega Pen, orginally written by Brian Hoary and forked to Omega Pen by .
You can get an installable version Epic Pen (no source) from here: 
http://epic-pen.com/ in free (personal) or commercial versions.

This is a Windows-only application, based on [WFP](http://www.wpf-tutorial.com/about-wpf/what-is-wpf/) (Windows Presentation Foundation)

# Analysis of changes between latest original Epic-Pen

ChatGPT Mar 14 Version said some words about the changes between them.

_start of citation_ 

(see diff here) https://github.com/masterrex/Epic-Pen/compare/master...xemjeff:Omega-Pen:master
After reviewing the changes made in the Omega Pen fork compared to the Epic Pen master branch, here are some notable differences:

* The Omega Pen fork has updated references to the .NET framework version to target version 4.8, while the Epic Pen master branch still targets version 4.0.

* The Omega Pen fork has updated the WPF version from 3.5 to 4.0.

* The Omega Pen fork has added several new features and enhancements to the application, including the ability to change the pen color and size, adjust the transparency of the pen, and save the settings for future use.

* The Omega Pen fork has fixed several bugs and issues, such as improving the handling of mouse events and fixing issues with the application crashing or freezing.

Based on these differences, it is possible that the updates and bug fixes made in the Omega Pen fork have resolved the issues that were present in the Epic Pen master branch, resulting in a version that works on Windows 11 and compiles successfully with Visual Studio 2019.

_end of citation_

# original Omega-Pen (assume v1.0)

* Re-arranges the color pallette
* New icons, created using the free [Metro Studio](https://www.syncfusion.com/downloads/metrostudio) from Syncfusion.
* Smoothing of lines in Pen mode using Bezier curves
* New modes added for drawing precise rectangles, arrows and circles

I made changes to the code to support simple shapes that I need
while giving on-line presentations. The arrows in particular are helpful.
