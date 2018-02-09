using System;
using System.ComponentModel;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace KsWare.IO.NamedPipes.Internal {

	public class NamedPipeServerStreams : NamedPipeStreams {

		private readonly string _pipeName;
		private readonly PipeDirection _direction = PipeDirection.InOut;
		private readonly int _maxNumberOfServerInstances = (int) PIPE_UNLIMITED_INSTANCES;
		private readonly PipeTransmissionMode _transmissionMode = PipeTransmissionMode.Byte;
		private readonly PipeOptions _options = PipeOptions.Asynchronous; // always Asynchronous requiered

		private NamedPipeServerStream _readPipe;
		private NamedPipeServerStream _writePipe;

		public NamedPipeServerStreams(string pipeName) : base(pipeName, true) { CreatePipes(); }

		public NamedPipeServerStreams(string pipeName,
			PipeDirection direction,
			int maxNumberOfServerInstances,
			PipeTransmissionMode transmissionMode,
			PipeOptions options) : base(pipeName,true) {
			_pipeName                   = pipeName;
			_direction                  = direction;
			_maxNumberOfServerInstances = maxNumberOfServerInstances;
			_transmissionMode           = transmissionMode;
			_options                    = options | PipeOptions.Asynchronous; // always Asynchronous requiered
			CreatePipes();
		}

		public NamedPipeServerStream WritePipe => _writePipe;

		/// <summary>
		/// Creates the pipes.
		/// </summary>
		private void CreatePipes() {
			_readPipe?.Dispose();
			_writePipe?.Dispose();

			// Create a write pipe for sending notifications to client.
			_writePipe = new NamedPipeServerStream(
				$"{_pipeName}client", 
				PipeDirection.Out,
				_maxNumberOfServerInstances, 
				_transmissionMode, 
				_options);

			// Create a read pipe for receiving notifications from the client.
			// Creating a pipe to high integrity process from low integrity process requires native access list creation (.NET bug).
			SECURITY_ATTRIBUTES securityAttributes    = CreateNativePipeSecurity();
			IntPtr              securityAttributesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(securityAttributes));
			Marshal.StructureToPtr(securityAttributes, securityAttributesPtr, false);

			string nativePipeName = $@"\\.\pipe\{_pipeName}server";

			var openMode = ((_options & PipeOptions.Asynchronous) != 0 ? FILE_FLAG_OVERLAPPED : 0) |
			               ((_options & PipeOptions.WriteThrough) != 0 ? FILE_FLAG_WRITE_THROUGH : 0);

			var pipeMode = _transmissionMode == PipeTransmissionMode.Message ? PIPE_TYPE_MESSAGE : 0;

			SafePipeHandle nativePipe = CreateNamedPipe(
				nativePipeName, 
				PIPE_ACCESS_INBOUND | openMode, 
				pipeMode, 
				_maxNumberOfServerInstances == -1 ? PIPE_UNLIMITED_INSTANCES : (uint)_maxNumberOfServerInstances, 
				0, 
				0,
				NMPWAIT_WAIT_FOREVER,                                      
				securityAttributesPtr);

			int error = Marshal.GetLastWin32Error();

			Marshal.FreeHGlobal(securityAttributesPtr);

			if (nativePipe.IsInvalid) {
				throw new Win32Exception(error);
			}

			_readPipe = new NamedPipeServerStream(
				PipeDirection.In, 
				(_options & PipeOptions.Asynchronous) !=0, 
				false, 
				nativePipe);

			RegisterPipes(_readPipe, _writePipe);
		}

		/// <inheritdoc cref="NamedPipeServerStream.WaitForConnection"/>
		public void WaitForConnection() {
			// the original NamedPipeServerStream does still block and does not return on Dispose.
			// but the workaround requieres PipeOptions.Asynchronous

			var r = _readPipe.BeginWaitForConnection(null, null);
			var w = _writePipe.BeginWaitForConnection(null, null);
			WaitHandle.WaitAll(new[] {r.AsyncWaitHandle, w.AsyncWaitHandle});
			if (_disposeFlag) throw new ObjectDisposedException(nameof(NamedPipeServerStreams));

			_readPipe.EndWaitForConnection(r);
			_writePipe.EndWaitForConnection(w);
		}

		public bool WaitForConnectionCancelable() {
			// the original NamedPipeServerStream does still block and does not return on Dispose.
			// but the workaround requieres PipeOptions.Asynchronous

			var r = _readPipe.BeginWaitForConnection(null, null);
			var w = _writePipe.BeginWaitForConnection(null, null);
			WaitHandle.WaitAll(new[] {r.AsyncWaitHandle, w.AsyncWaitHandle});
			if (_disposeFlag) return false;

			_readPipe.EndWaitForConnection(r);
			_writePipe.EndWaitForConnection(w);
			return true;
		}


		/// <inheritdoc cref="NamedPipeServerStream.Disconnect"/>
		public void Disconnect() {
			_readPipe.Disconnect();
			_writePipe.Disconnect();
		}

	}

}


