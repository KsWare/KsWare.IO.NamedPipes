From https://devzone.channeladam.com/notebooks/languages/dotnet/net-5-guidance-for-library-developers/

# TFM Usage Cheat Sheet
Here is the Cheat Sheet for guidance on TFM usage:

- do NOT target .NET Standard 1.x for new libraries (.NET Core 1.x is end-of-life and is no longer supported);
- target a minimum of netstandard2.0 if you want to support minimum usage of .NET Core 2.x or .NET Framework (from 4.6.1). See .NET Standard Versions for more information on version compatibility;
- target a minimum of netstandard2.1 if you want to support minimum usage of .NET Core 3.x or share code between Mono, Xamarin, and .NET Core 3.x; and
- target a minimum of net5.0 if you do NOT need to support the .NET Core 2.x, .NET Core 3.x or .NET Framework (from 4.6.1).

__Boiling it all down to one sentence: in order to enable the largest reach of your library, multi-target both netstandard2.0 and net5.0.__
