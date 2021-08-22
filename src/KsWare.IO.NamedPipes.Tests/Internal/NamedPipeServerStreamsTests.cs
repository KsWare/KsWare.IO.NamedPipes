using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using KsWare.IO.NamedPipes.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KsWare.IO.NamedPipes.Tests.Internal {

	[TestClass]
	public class NamedPipeServerStreamsTests {

		[TestMethod]
		public void WaitForConnectionCancelableAndDispose() {
			var pipeName = Helper.CreatePipeName();
			var streams=new NamedPipeServerStreams(pipeName,PipeDirection.InOut, 1,PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
			Task.Run(() => streams.WaitForConnectionCancelable());
			Thread.Sleep(100);
			streams.Dispose();
		}

		[TestMethod]
		public void MaxNumberOfServiceInstances1000() {
			var pipeName = Helper.CreatePipeName();
			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				new NamedPipeServerStreams(pipeName, PipeDirection.InOut, 1000, PipeTransmissionMode.Byte,
					PipeOptions.Asynchronous));

		}

		[TestMethod]
		public void DecreaseNumberOfServiceInstancesShouldFail() {
			var pipeName = Helper.CreatePipeName();
			var max      = NamedPipeServerStream.MaxAllowedServerInstances;
			var streams = new NamedPipeServerStreams(pipeName, PipeDirection.InOut, 5, PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);

			Assert.ThrowsException<UnauthorizedAccessException>(() =>
				new NamedPipeServerStreams(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous));
			streams.Dispose();
		}

		[TestMethod]
		public void IncreaseNumberOfServiceInstances1And2() {
			var pipeName = Helper.CreatePipeName();
			var streams1 = new NamedPipeServerStreams(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);

			// IOException: Alle Pipeinstanzen sind ausgelastet. (2018)
			// Win32Exception: Alle Pipeinstanzen sind ausgelastet (2021)
			Assert.ThrowsException<Win32Exception>(() =>
				new NamedPipeServerStreams(pipeName, PipeDirection.InOut, 2, PipeTransmissionMode.Byte, PipeOptions.Asynchronous));

			streams1.Dispose();
		}

		[TestMethod]
		public void IncreaseNumberOfServiceInstances1CloseAnd2() {
			var pipeName = Helper.CreatePipeName();
			var streams1 = new NamedPipeServerStreams(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);
			streams1.Dispose();
			var streams2 = new NamedPipeServerStreams(pipeName, PipeDirection.InOut, 2, PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);
			streams2.Dispose();
		}

		[TestMethod]
		public void IncreaseNumberOfServiceInstances2And3() {
			var pipeName = Helper.CreatePipeName();
			var max      = NamedPipeServerStream.MaxAllowedServerInstances;
			var streams1 = new NamedPipeServerStreams(pipeName, PipeDirection.InOut, 2, PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);

			var streams2 = new NamedPipeServerStreams(pipeName, PipeDirection.InOut, 3, PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);
			streams1.Dispose();
			streams2.Dispose();
		}
	}
}