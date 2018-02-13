using System.Reflection;
using System.Runtime.InteropServices;
using KsWare.IO.NamedPipes;

[assembly: AssemblyTitle("KsWare.IO.NamedPipes")]
[assembly: AssemblyDescription("A named pipes library.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("KsWare")]
[assembly: AssemblyProduct("KsWare.IO.NamedPipes")]
[assembly: AssemblyCopyright("Copyright © 2018 by KsWare. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("378cac5b-1f20-4278-a129-9a9ce5243c7c")]

[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfo.Version + AssemblyInfo.PreVersion)]

// ReSharper disable once CheckNamespace
namespace KsWare.IO.NamedPipes {

	public static class AssemblyInfo {

		public const string Version = "1.0.0";

		public const string PreVersion = "-beta";
	}

}
