﻿// ***********************************************************************
// Assembly         : KsWare.IO.NamedPipes
// Author           : SchreinerK
// Created          : 02-02-2018
//
// Last Modified By : SchreinerK
// Last Modified On : 02-04-2018
// ***********************************************************************
// <copyright file="NamedPipeStreams.cs" company="KsWare">
//     Copyright © 2018 by KsWare. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace KsWare.IO.NamedPipes.Internal {

	/// <summary>
	/// Class NamedPipeStreams.
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	/// <autogeneratedoc />
	public abstract class NamedPipeStreams : IDisposable {
		protected readonly string _pipeName;
		protected readonly string _serverName = ".";
		private bool _isServer;
		private PipeStream _readPipeStream;
		private PipeStream _writePipeStream;
		/// <summary>
		/// The dispose flag
		/// </summary>
		/// <autogeneratedoc />
		protected bool _disposeFlag;


		/// <summary>
		/// Initializes a new instance of the <see cref="NamedPipeStreams"/> class.
		/// </summary>
		/// <param name="pipeName">Name of the pipe.</param>
		/// <param name="isServer">if set to <c>true</c> [is server].</param>
		/// <autogeneratedoc />
		protected NamedPipeStreams(string pipeName, bool isServer) {
			_pipeName = pipeName;
			_isServer = isServer;
		}

		protected NamedPipeStreams(string serverName, string pipeName) {
			_serverName = serverName;
			_pipeName = pipeName;
			_isServer = false;
		}

		/// <summary>
		/// Gets the reader.
		/// </summary>
		/// <value>The reader.</value>
		/// <autogeneratedoc />
		public StreamReader Reader { get; private set; }
		/// <summary>
		/// Gets the writer.
		/// </summary>
		/// <value>The writer.</value>
		/// <autogeneratedoc />
		public StreamWriter Writer { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance is connected.
		/// </summary>
		/// <value><c>true</c> if this instance is connected; otherwise, <c>false</c>.</value>
		/// <inheritdoc cref="PipeStream.IsConnected" />
		public bool IsConnected => (_readPipeStream?.IsConnected ?? false) || (_writePipeStream?.IsConnected ?? false);

		/// <summary>
		/// Waits for pipe drain.
		/// </summary>
		/// <inheritdoc cref="PipeStream.WaitForPipeDrain" />
		public void WaitForPipeDrain() { _writePipeStream.WaitForPipeDrain(); }

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <inheritdoc cref="Stream.Dispose()" />
		public void Dispose() => Close();

		/// <summary>
		/// Closes this instance.
		/// </summary>
		/// <inheritdoc cref="Stream.Close()" />
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
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="explicitDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		/// <autogeneratedoc />
		protected virtual void Dispose(bool explicitDisposing) {
			if (explicitDisposing) {
				_disposeFlag = true;
				try { Reader?.Dispose(); }catch { } // => StreamReader.Flush => PipeStream.Write => throws exception
				try { Writer?.Dispose(); }catch { } // System.InvalidOperationException: 'The pipe connection has not been established yet.'

				_readPipeStream.Close();
				_writePipeStream.Close();
			}
		}

		/// <summary>
		/// Registers the pipes.
		/// </summary>
		/// <param name="readStream">The read stream.</param>
		/// <param name="writeStream">The write stream.</param>
		/// <autogeneratedoc />
		protected void RegisterPipes(PipeStream readStream, PipeStream writeStream) {
			_readPipeStream = readStream;
			_writePipeStream = writeStream;
			Reader = (readStream != null) ? new StreamReader(readStream) : null;
			Writer = (writeStream != null) ? new StreamWriter(writeStream) : null;
		}

		/// <summary>
		/// PIPE_UNLIMITED_INSTANCES (255). NOTE: for managed API this constant must not be used because different value is required (-1).
		/// </summary>
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

		/// <summary>
		/// Enum PipeModeFlags
		/// </summary>
		/// <autogeneratedoc />
		[Flags]
		enum PipeModeFlags : uint {
			PIPE_TYPE_BYTE = 0x00000000,
			PIPE_TYPE_MESSAGE = 0x00000004,
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
