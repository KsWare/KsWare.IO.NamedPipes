using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace KsWare.IO.NamedPipes.Internal {

	public abstract class NamedPipeStreams : IDisposable {

		private readonly string _pipeName;
		private bool _isServer;
		private PipeStream _readPipeStream;
		private PipeStream _writePipeStream;
		protected bool _disposeFlag;

		protected NamedPipeStreams(string pipeName, bool isServer) {
			_pipeName = pipeName;
			_isServer = isServer;
		}

		public StreamReader Reader { get; private set; }
		public StreamWriter Writer { get; private set; }

		/// <inheritdoc cref="PipeStream.IsConnected"/>
		public bool IsConnected => (_readPipeStream?.IsConnected ?? false) || (_writePipeStream?.IsConnected ?? false);

		/// <inheritdoc cref="PipeStream.WaitForPipeDrain"/>
		public void WaitForPipeDrain() { _writePipeStream.WaitForPipeDrain(); }

		/// <inheritdoc cref="Stream.Dispose()"/>
		public void Dispose() => Close();

		/// <inheritdoc cref="Stream.Close()"/>
		public void Close() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/* ObjectDisposedException: Auf eine geschlossene Pipe kann nicht zugegriffen werden.
		   bei PipeStream.CheckWriteOperations()
		   bei PipeStream.Write(Byte[] buffer, Int32 offset, Int32 count)
		   bei StreamWriter.Flush(Boolean flushStream, Boolean flushEncoder)
		   bei StreamWriter.Dispose(Boolean disposing)
		   bei TextWriter.Dispose()   
		*/
		/* System.InvalidOperationException: Die Verbindung der Pipe wurde noch nicht hergestellt.
		   PipeStream.CheckWriteOperations()
		   PipeStream.Write(Byte[] buffer, Int32 offset, Int32 count)
		   Flush(Boolean flushStream, Boolean flushEncoder)
		   StreamWriter.Dispose(Boolean disposing)
		   TextWriter.Dispose()
		   NamedPipeStreams.Dispose() 
		*/
		protected virtual void Dispose(bool explicitDisposing) {
			if (explicitDisposing) {
				_disposeFlag = true;
				try { Reader?.Dispose(); }catch { } // => StreamReader.Flush => PipeStream.Write => throws exception
				try { Writer?.Dispose(); }catch { } // System.InvalidOperationException: 'Die Verbindung der Pipe wurde noch nicht hergestellt.'

				_readPipeStream.Close();
				_writePipeStream.Close();
			}
		}

		protected void RegisterPipes(PipeStream readStream, PipeStream writeStream) {
			_readPipeStream = readStream;
			_writePipeStream = writeStream;
			Reader = new StreamReader(readStream);
			Writer = new StreamWriter(writeStream);
		}


		protected const uint PIPE_UNLIMITED_INSTANCES = 255;
		protected const uint NMPWAIT_WAIT_FOREVER = 0xffffffff;
		protected const uint PIPE_ACCESS_INBOUND = 0x00000001;

		/// <summary>
		/// Generate security attributes to allow low integrity process to connect to high integrity service.
		/// </summary>
		/// <returns>A structure filled with proper attributes.</returns>
		protected SECURITY_ATTRIBUTES CreateNativePipeSecurity() {
			// Define the SDDL for the security descriptor.
			var sddl = "D:" +        // Discretionary ACL
				"(A;OICI;GRGW;;;AU)" +  // Allow read/write to authenticated users
				"(A;OICI;GA;;;BA)";     // Allow full control to administrators

			if (!ConvertStringSecurityDescriptorToSecurityDescriptor(
				sddl, 1, out var securityDescriptor, out var securityDescriptorSize)) {
				throw Helper.Win32Exception();
			}

			SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
			sa.nLength = Marshal.SizeOf(sa);
			sa.lpSecurityDescriptor = securityDescriptor;
			sa.bInheritHandle = 0;

			return sa;
		}

		[DllImport("advapi32.dll", SetLastError = true)]
		static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
			string stringSecurityDescriptor,
			uint stringSdRevision,
			out IntPtr securityDescriptor,
			out UIntPtr securityDescriptorSize
		);

		protected const uint FILE_FLAG_OVERLAPPED = 0x40000000;
		protected const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;

		protected const uint PIPE_TYPE_BYTE = 0x00000000;
		protected const uint PIPE_TYPE_MESSAGE = 0x00000004;

		/// <summary>
		/// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa365150.aspx">MSDN</a>
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		protected static extern SafePipeHandle CreateNamedPipe(string lpName,
			uint dwOpenMode,
			uint dwPipeMode,
			uint nMaxInstances,
			uint nOutBufferSize,
			uint nInBufferSize,
			uint nDefaultTimeOut,
			IntPtr lpSecurityAttributes);

		[DllImport("kernel32.dll", SetLastError = true)]
		protected static extern SafePipeHandle CreateNamedPipe(string lpName,
			uint dwOpenMode,
			uint dwPipeMode,
			uint nMaxInstances,
			uint nOutBufferSize,
			uint nInBufferSize,
			uint nDefaultTimeOut,
			SECURITY_ATTRIBUTES lpSecurityAttributes);

		[Flags]
		enum PipeOpenModeFlags : uint {
			PIPE_ACCESS_DUPLEX = 0x00000003,
			PIPE_ACCESS_INBOUND = 0x00000001,
			PIPE_ACCESS_OUTBOUND = 0x00000002,
			FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000,
			FILE_FLAG_WRITE_THROUGH = 0x80000000,
			FILE_FLAG_OVERLAPPED = 0x40000000,
			WRITE_DAC = 0x00040000,
			WRITE_OWNER = 0x00080000,
			ACCESS_SYSTEM_SECURITY = 0x01000000
		}

		[Flags]
		enum PipeModeFlags : uint {
			//One of the following type modes can be specified. The same type mode must be specified for each instance of the pipe.
			PIPE_TYPE_BYTE = 0x00000000,
			PIPE_TYPE_MESSAGE = 0x00000004,

			//One of the following read modes can be specified. Different instances of the same pipe can specify different read modes
			PIPE_READMODE_BYTE = 0x00000000,
			PIPE_READMODE_MESSAGE = 0x00000002,

			//One of the following wait modes can be specified. Different instances of the same pipe can specify different wait modes.
			PIPE_WAIT = 0x00000000,
			PIPE_NOWAIT = 0x00000001,

			//One of the following remote-client modes can be specified. Different instances of the same pipe can specify different remote-client modes.
			PIPE_ACCEPT_REMOTE_CLIENTS = 0x00000000,
			PIPE_REJECT_REMOTE_CLIENTS = 0x00000008
		}

		[StructLayout(LayoutKind.Sequential)]
		protected struct SECURITY_ATTRIBUTES {
			public int nLength;
			public IntPtr lpSecurityDescriptor;
			public int bInheritHandle;
		}


	}

}
