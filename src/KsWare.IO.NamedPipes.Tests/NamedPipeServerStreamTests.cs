using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KsWare.IO.NamedPipes.Tests {

	[TestClass]
	[Ignore /* manual compatibility tests */]
	public class NamedPipeServerStreamTests {

		[TestMethod]
		public void WaitForConnectionDisposeTest() {
			var server = new NamedPipeServerStream(Guid.NewGuid().ToString("N"), PipeDirection.Out, 1, PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);
			Task.Run(() => {
				try {
					server.WaitForConnection();
					Debug.WriteLine($"IsConnected:  {server.IsConnected}");
					Debugger.Break(); // never executed
				}
				catch (Exception ex) {
					Debug.WriteLine(ex);
				}
			});
			Thread.Sleep(500);
			server.Dispose();
			Thread.Sleep(500);
		}

		[TestMethod]
		public void WaitForConnectionCloseTest() {
			var server = new NamedPipeServerStream(Guid.NewGuid().ToString("N"), PipeDirection.Out, 1, PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);
			Task.Run(() => {
				try {
					server.WaitForConnection();
					Debug.WriteLine($"IsConnected:  {server.IsConnected}");
					Debugger.Break(); // never executed
				}
				catch (Exception ex) {
					Debug.WriteLine(ex);
				}
			});
			Thread.Sleep(500);
			server.Close();
			Thread.Sleep(500);
		}


		[TestMethod]
		public void WaitForConnectionDisconnectTest() {
			var server = new NamedPipeServerStream(Guid.NewGuid().ToString("N"), PipeDirection.Out, 1, PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);
			Task.Run(() => {
				try {
					server.WaitForConnection();
					Debug.WriteLine($"IsConnected:  {server.IsConnected}");
					Debugger.Break(); // never executed
				}
				catch (Exception ex) {
					Debug.WriteLine(ex);
				}
			});
			Thread.Sleep(500);
			server.Disconnect(); // System.InvalidOperationException: 'Die Verbindung der Pipe wurde noch nicht hergestellt.'
			Thread.Sleep(500);
		}

	}
}
