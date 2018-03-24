using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using KsWare.NamedPipeDemo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KsWare.IO.NamedPipes.Tests {

	[TestClass]
	public class NamedPipeClientTests {

		[TestMethod]
		public void StartClientAndDisposeTest() {
			string pipeName = nameof(StartClientAndDisposeTest);
			var client = new NamedPipeClient(pipeName);
			client.Dispose();
		}

		[TestMethod]
		public void StartClientAndConnectNoServer() {
			string pipeName = nameof(StartClientAndConnectNoServer);
			var    client   = new NamedPipeClient(pipeName);
			Assert.ThrowsException<TimeoutException>(() => client.Connect(100));
			client.Dispose();
		}

		[TestMethod]
		public void StartServerAndClientAndDispose() {
			string pipeName = nameof(StartServerAndClientAndDispose);
			var server = new NamedPipeServer(pipeName, 1, 1,(s, e) => {});
			server.Start();
			var client = new NamedPipeClient(pipeName,100);
			client.Dispose();
			server.Dispose();
		}

		[TestMethod]
		public void StartServerAndClientAndSend() {
			string pipeName = nameof(StartServerAndClientAndSend);
			var server = new NamedPipeServer(pipeName, 1, 1, (s, e) => e.Response = "Pong");
			var client = new NamedPipeClient(pipeName, 100);
			var response=client.SendRequest("Ping");
			Assert.AreEqual("Pong", response);
			client.Dispose();
			server.Dispose();
		}

		[TestMethod]
		public void StartEchoServerAndClientAndSend() {
			string pipeName = nameof(StartEchoServerAndClientAndSend);
			Process p=null;
			try {
				p        = Program.StartEchoServer(pipeName, 4, 1);
				var client   = new NamedPipeClient(pipeName, 100);
				var c = 0;
				var m = 1000;
				for (int i = 0; i < m; i++) {
					var response = client.SendRequest("Ping");
					if (response == "Pong") c++;
				}
				client.Dispose();
				Assert.AreEqual(m, c);
			}
			finally {
				p?.CloseMainWindow();
				p?.WaitForExit(500);
				p?.Kill();
				Debug.WriteLine("done");
			}
		}

		[TestMethod]
		public void StartEchoServerAndMultipleClientsAndSend() {
			

			string  pipeName = nameof(StartEchoServerAndMultipleClientsAndSend);
			Process p        = null;
			try {
				p = Program.StartEchoServer(pipeName, 4, 1);
				var clients = new List<string>();
				for (int i = 0; i < 4; i++) {
					clients.Add($"Client#{i+1}");
				}

				Parallel.ForEach(clients, (c) => InternalStartClientsAndSend(c, 3));
			}
			finally {
				p.CloseMainWindow();
				p.WaitForExit(500);
				p.Kill();
				Debug.WriteLine("done");
			}

			void InternalStartClientsAndSend(string clientName, int count) {
				try {
					var client = new NamedPipeClient(pipeName, 100);
					var c      = 0;
					for (int i = 0; i < count; i++) {
						var response = client.SendRequest("Ping");
						if (response == "Pong") c++;
					}
					client.Dispose();
					if (c != count) {
						Debug.WriteLine($"{count - c} messages lost.");
					}
				}
				catch (Exception ex) {
					Debug.WriteLine(ex);
				}
				finally {
					Debug.WriteLine($"{clientName} done");
				}
			}
		}

	}
}
