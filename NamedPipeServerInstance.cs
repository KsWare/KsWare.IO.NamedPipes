using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KsWare.IO.NamedPipes.Internal;

namespace KsWare.IO.NamedPipes {

	public class NamedPipeServerInstance : IDisposable {

		private static int _lastInstanceId;

		private NamedPipeServerStreams _streams;
		private bool _disposeFlag = false;


		public NamedPipeServerInstance(string pipeName, int maxNumberOfServerInstances) {
			_streams = new NamedPipeServerStreams(
				pipeName,
				PipeDirection.InOut, 
				maxNumberOfServerInstances,
				PipeTransmissionMode.Byte, 
				PipeOptions.None
			);
		}

		public Task Task { get; private set; }

		public int InstanceId { get; } = Interlocked.Increment(ref _lastInstanceId);

		public event EventHandler Connected = delegate { };

		public event EventHandler Disconnected = delegate { };

		public event EventHandler<PipeMsgEventArgs> RequestReceived = delegate { };

		public void Start() {
			Task = Task.Factory.StartNew(Communication);
//			if(_thread!=null) throw new InvalidOperationException("Already started.");
//			_thread = new Thread(Communication);
//			_thread.Start();
		}

		public void Dispose() {
			_disposeFlag = true;
			_streams.Dispose();
		}

		private void Communication() {
			string exitReason = "None";
			try {
				Debug.WriteLine($"[Server {InstanceId}] Communication Started");
				if (!_streams.WaitForConnectionCancelable()) { exitReason = "Connect canceled";return;}
				Debug.WriteLine($"[Server {InstanceId}] Communication Connected");
				Connected?.Invoke(this, EventArgs.Empty);

				while (true) {
					if(_disposeFlag) { exitReason = "Disposed"; break; }
					if (_streams.Reader.EndOfStream) { exitReason = "EndOfStream"; break;}

					var request = _streams.Reader.ReadLine(); // TODO read strategy message/byte/line
					Debug.WriteLine($"[Server {InstanceId}] Communication Received");

					if (request != null) {
						var msgEventArgs = new PipeMsgEventArgs(request);
						RequestReceived.Invoke(this, msgEventArgs);
						_streams.Writer.WriteLine(msgEventArgs.Response);
						_streams.Writer.Flush();
					}
				}
			}
			catch (IOException ex) {
				exitReason = "IOException";
				Debug.WriteLine(ex);
			}
			catch (Exception ex) {
				exitReason = "Exeption";
				Debug.WriteLine(ex);
			}
			finally {
				Debug.WriteLine($"[Server {InstanceId}] Communication exit. Reason: {exitReason}");
				Disconnected?.Invoke(this,EventArgs.Empty);
				Dispose();
			}
		}
	}
}


