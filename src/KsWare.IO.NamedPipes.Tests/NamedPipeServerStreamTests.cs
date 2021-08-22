using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KsWare.IO.NamedPipes.Tests {

	[TestClass, TestCategory("LocalOnly")]
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

		[TestMethod]
		public void ClientServerConnectionTest() {
			Helper.RunMta(() => {
				var pipeName=Guid.NewGuid().ToString("N");
				var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
				var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
				var serverWaitHandle = new ManualResetEventSlim();
				var clientWaitHandle = new ManualResetEventSlim();

				Task.Run(() => {
					try {
						server.WaitForConnection();
						Trace.WriteLine("Server connected");
						serverWaitHandle.Set();
					}
					catch (Exception ex) {
						Debug.WriteLine(ex);
					}
				});
				Task.Run(() => {
					try {
						client.Connect();
						Trace.WriteLine("Client connected");
						clientWaitHandle.Set();
					}
					catch (Exception ex) {
						Debug.WriteLine(ex);
					}
				});

				var success = WaitHandle.WaitAll(new[] { serverWaitHandle.WaitHandle, clientWaitHandle.WaitHandle }, 500);

				client.Dispose();
				server.Dispose();
			
				Assert.AreEqual(true,success);
			});
		}
		[TestMethod]
		public void ClientServerAsyncConnectionTest() {
			Helper.RunMta(() => {
				var pipeName=Guid.NewGuid().ToString("N");
				var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
				var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
				var serverWaitHandle = new ManualResetEventSlim();
				var clientWaitHandle = new ManualResetEventSlim();

				var awh= server.BeginWaitForConnection(ar => {
					server.EndWaitForConnection(ar);
					Trace.WriteLine("Server connected");
					serverWaitHandle.Set();
				},null);

				Task.Run(() => {
					try {
						client.Connect();
						Trace.WriteLine("Client connected");
						clientWaitHandle.Set();
					}
					catch (Exception ex) {
						Debug.WriteLine(ex);
					}
				});

				var success = WaitHandle.WaitAll(new[] { serverWaitHandle.WaitHandle, clientWaitHandle.WaitHandle }, 500);

				client.Dispose();
				server.Dispose();
			
				Assert.AreEqual(true,success);
			});
		}

	}
}
