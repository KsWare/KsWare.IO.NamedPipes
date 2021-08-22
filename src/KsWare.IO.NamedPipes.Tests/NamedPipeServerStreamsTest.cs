using System;
using System.Threading;
using System.Threading.Tasks;
using KsWare.IO.NamedPipes.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KsWare.IO.NamedPipes.Tests {

	[TestClass]
	public class NamedPipeServerStreamsTest {

		[TestMethod]
		public void ClientServerConnectionTest() {
			Helper.RunMta(() => {
				var pipeName=Guid.NewGuid().ToString("N");
				var server = new NamedPipeServerStreams(pipeName);
				var client = new NamedPipeClientStreams(pipeName);
				var serverWaitHandle = new ManualResetEventSlim();
				var clientWaitHandle = new ManualResetEventSlim();

				Task.Run(() => {
					try {
						server.WaitForConnection();
						Console.WriteLine("Server connected");
						serverWaitHandle.Set();
					}
					catch (Exception ex) {
						Console.WriteLine(ex);
					}
				});
				Task.Run(() => {
					try {
						client.Connect();
						Console.WriteLine("Client connected");
						clientWaitHandle.Set();
					}
					catch (Exception ex) {
						Console.WriteLine(ex);
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
