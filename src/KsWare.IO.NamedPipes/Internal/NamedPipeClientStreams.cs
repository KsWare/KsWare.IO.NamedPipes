using System.IO.Pipes;

namespace KsWare.IO.NamedPipes.Internal {

	public class NamedPipeClientStreams : NamedPipeStreams {
		private readonly string _pipeName;

		private NamedPipeClientStream readPipe;
		private NamedPipeClientStream writePipe;
		private PipeOptions _options = PipeOptions.Asynchronous;

		public NamedPipeClientStreams(string pipeName):base(pipeName,false) {
			_pipeName = pipeName;
			CreatePipes();
		}

		public NamedPipeClientStream ReadPipe => readPipe;

		/// <inheritdoc cref="NamedPipeClientStream.Connect()"/>
		public void Connect() {
			readPipe.Connect();
			writePipe.Connect();
		}

		/// <inheritdoc cref="NamedPipeClientStream.Connect(int)"/>
		public void Connect(int timeOut) {
			readPipe.Connect(timeOut);
			writePipe.Connect(timeOut);
		}

		/// <summary>
		/// Creates the pipes.
		/// </summary>
		private void CreatePipes() {
			readPipe?.Dispose();
			writePipe?.Dispose();

			// Create a read pipe for receiving notifications from server.
			readPipe = new NamedPipeClientStream(".", $"{_pipeName}client", PipeDirection.In,_options);

			// Create a write pipe for sending notifications to the server.
			writePipe = new NamedPipeClientStream(".", $"{_pipeName}server", PipeDirection.Out, _options);

			RegisterPipes(readPipe, writePipe);
		}

		protected override void Dispose(bool explicitDispose) {
			base.Dispose(explicitDispose);
		}

		
	}

}