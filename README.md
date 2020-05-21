# PerfectlyNormalUnity
Helper classes for unity development

---------------------------------

This is meant to be classes that you can reference into your unity project

This project's references are to the unity install location.  Since that's constantly getting updated, you'll probably have to repair those references before compiling

After compiling, copy this dll from:
PerfectlyNormalUnity\bin\Debug\netstandard2.0\PerfectlyNormalUnity.dll

To anywhere under your unity Assets folder (I like to make a lib folder)

Then any class that you want to use this from needs a using statment at the top:
using PerfectlyNormalUnity;

Alternatively, you can probably just copy the classes into a folder under Assets and let everything live inside unity

---------------------------------

The easiest way to add new code to this project is to develop and test it in a unity project.  Then when you have it the way you want, copy into this solution

If there is a new reference needed, just select the type (from your unity project), hit F12 and the top of the generated file tells what dll that type came from

This page has more details:
https://docs.unity3d.com/Manual/UsingDLL.html
