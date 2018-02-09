using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KsWare.IO.NamedPipes.Internal {

	internal static class Helper {
		public static Exception Win32Exception() {
			return new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

}