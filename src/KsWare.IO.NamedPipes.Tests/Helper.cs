using System.Runtime.CompilerServices;
using System.Threading;

namespace KsWare.IO.NamedPipes.Tests {

	public static class Helper {

		public static string CreatePipeName([CallerMemberName] string callerMemberName = null) {
			return callerMemberName;
		}

		public static void RunMta(ThreadStart action) {
			var t = new Thread(action);
			t.SetApartmentState(ApartmentState.MTA);
			t.Start();
			t.Join();
		}
	}
}
