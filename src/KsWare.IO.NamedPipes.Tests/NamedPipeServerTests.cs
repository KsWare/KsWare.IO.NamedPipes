using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KsWare.IO.NamedPipes.Tests {

	[TestClass]
	public class NamedPipeServerTests {

		[TestMethod]
		public void StartServerAndDisposeTest() {

			string          pipeName = nameof(StartServerAndDisposeTest);
			NamedPipeServer Server;

			try {
				Server = new NamedPipeServer(pipeName, 1, 1);
				Thread.Sleep(500);
				Server.Dispose();
				Thread.Sleep(500);
				Debugger.Break();
			}
			catch (Exception ex) {
				Debug.WriteLine(ex);
			}
			finally {
				Server = null;
			}
		}

		[TestMethod]
		public void StartServer() {

			string          pipeName = nameof(StartServer);
			NamedPipeServer Server;

			try {
				Server = new NamedPipeServer(pipeName, 1, 1, (sender, messageArgs) => {
					Debug.WriteLine(messageArgs.Request);
					messageArgs.Response = "Echo";
				});

			}
			catch (Exception ex) {
				Debug.WriteLine(ex);
			}
			finally {
				Server = null;
			}
		}


	}
}
