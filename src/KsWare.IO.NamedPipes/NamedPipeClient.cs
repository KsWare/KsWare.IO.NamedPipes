using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using KsWare.IO.NamedPipes.Internal;

namespace KsWare.IO.NamedPipes {

	public class NamedPipeClient : IDisposable {
		//TODO Unsolicited Messages

		private NamedPipeClientStreams _streams;

		public NamedPipeClient(string pipeName) {
			_streams = new NamedPipeClientStreams(pipeName);
		}

		public NamedPipeClient(string pipeName, int connectTimeOut) {
			try {
				_streams = new NamedPipeClientStreams(pipeName);
				_streams.Connect(connectTimeOut);
			}
			catch {
				_streams?.Dispose();
				throw;
			}
		}

		public void Dispose() {
			_streams.Dispose();
		}

		public string SendRequest(string request) {
			if (request==null) throw new ArgumentNullException(nameof(request));

			_streams.Writer.WriteLine(request);
			_streams.Writer.Flush();
			_streams.WaitForPipeDrain();
			var response = _streams.Reader.ReadLine();
			return response;
		}

		public void Connect(int timeoutMilliseconds=100) {
			_streams.Connect(timeoutMilliseconds);
		}
	}

}
