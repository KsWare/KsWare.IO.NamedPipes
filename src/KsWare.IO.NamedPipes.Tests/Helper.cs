using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KsWare.IO.NamedPipes.Tests {

	public static class Helper {

		public static string CreatePipeName([CallerMemberName] string callerMemberName = null) {
			return callerMemberName;
		}
	}
}
