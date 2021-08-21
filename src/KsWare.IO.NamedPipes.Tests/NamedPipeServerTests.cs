using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KsWare.IO.NamedPipes.Tests {

	[TestClass]
	public class NamedPipeServerTests {

		[TestMethod]
		public void StartServerAndDisposeTest() {

			var pipeName = Guid.NewGuid().ToString("N");
			NamedPipeServer server = null;

			try {
				server = new NamedPipeServer(pipeName, 1, 1);
				Thread.Sleep(500);
				server.Dispose();
				Thread.Sleep(500);
				Debugger.Break();
			}
			catch (Exception ex) {
				Debug.WriteLine(ex);
			}
			finally {
				server?.Dispose();
			}
		}

		[TestMethod]
		public void StartServer() {

			var pipeName = Guid.NewGuid().ToString("N");
			NamedPipeServer server = null;

			try {
				server = new NamedPipeServer(pipeName, 1, 1, (sender, messageArgs) => {
					Debug.WriteLine(messageArgs.Request);
					messageArgs.Response = "Echo";
				});

			}
			catch (Exception ex) {
				Debug.WriteLine(ex);
			}
			finally {
				server?.Dispose();
			}
		}

		[TestMethod]
		public void StartServerWithMaxConnections() {

			var pipeName = Guid.NewGuid().ToString("N");
			NamedPipeServer server = null;

			try {
				server = new NamedPipeServer(pipeName, -1, 1, (sender, messageArgs) => {
					Debug.WriteLine(messageArgs.Request);
					messageArgs.Response = "Echo";
				});

			}
			catch (Exception ex) {
				Debug.WriteLine(ex);
			}
			finally {
				server?.Dispose();
			}
		}
	}
}
