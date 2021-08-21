﻿// ***********************************************************************
// Assembly         : KsWare.IO.NamedPipes
// Author           : SchreinerK
// Created          : 02-03-2018
// ***********************************************************************
// <copyright file="NamedPipeServerStreams.cs" company="KsWare">
//     Copyright © 2018-2021 by KsWare. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.ComponentModel;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace KsWare.IO.NamedPipes.Internal {

	/// <summary>
	/// Class NamedPipeServerStreams.
	/// </summary>
	/// <seealso cref="KsWare.IO.NamedPipes.Internal.NamedPipeStreams" />
	/// <autogeneratedoc />
	public class NamedPipeServerStreams : NamedPipeStreams {

		private readonly string _pipeName;
		private readonly PipeDirection _direction = PipeDirection.InOut;
		// private readonly int _maxNumberOfServerInstances = (int) PIPE_UNLIMITED_INSTANCES;
		private readonly int _maxNumberOfServerInstances = NamedPipeServerStream.MaxAllowedServerInstances; // -1 and not 255
		private readonly PipeTransmissionMode _transmissionMode = PipeTransmissionMode.Byte;
		private readonly PipeOptions _options = PipeOptions.Asynchronous; // always Asynchronous required

		private NamedPipeServerStream _readPipe;
		private NamedPipeServerStream _writePipe;

		/// <summary>
		/// Initializes a new instance of the <see cref="NamedPipeServerStreams"/> class.
		/// </summary>
		/// <param name="pipeName">Name of the pipe.</param>
		/// <autogeneratedoc />
		public NamedPipeServerStreams(string pipeName) : base(pipeName, true) { CreatePipes(); }

		/// <summary>
		/// Initializes a new instance of the <see cref="NamedPipeServerStreams"/> class.
		/// </summary>
		/// <param name="pipeName">Name of the pipe.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="maxNumberOfServerInstances">The maximum number of server instances. [1-254 or NamedPipeServerStream.MaxAllowedServerInstances = -1]</param>
		/// <param name="transmissionMode">The transmission mode.</param>
		/// <param name="options">The <see cref="PipeOptions"/>. NOTE that <see cref="PipeOptions.Asynchronous"/> is always enabled.</param>
		/// <autogeneratedoc />
		public NamedPipeServerStreams(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, 
			PipeTransmissionMode transmissionMode, PipeOptions options) 
			: base(pipeName,true) {
			// if (maxNumberOfServerInstances != NamedPipeServerStream.MaxAllowedServerInstances && (maxNumberOfServerInstances < 1 || maxNumberOfServerInstances > 254))
			// 	throw new ArgumentOutOfRangeException(nameof(maxNumberOfServerInstances), maxNumberOfServerInstances,
			// 		$"maxNumberOfServerInstances must be either a value between 1 and 254 or NamedPipeServerStream.MaxAllowedServerInstances (to determine the maximum number allowed for the system resources).");
			_pipeName                   = pipeName;
			_direction                  = direction;
			_maxNumberOfServerInstances = maxNumberOfServerInstances;
			_transmissionMode           = transmissionMode;
			_options                    = options | PipeOptions.Asynchronous; // always Asynchronous required
			CreatePipes();
		}

		/// <summary>
		/// Gets the write pipe.
		/// </summary>
		/// <value>The write pipe.</value>
		/// <autogeneratedoc />
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
			// Creating a pipe to high integrity process from low integrity process requires native access list creation (.NET bug (2018)).
			var securityAttributes = CreateNativePipeSecurity();
			var securityAttributesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(securityAttributes));
			Marshal.StructureToPtr(securityAttributes, securityAttributesPtr, false);

			var nativePipeName = $@"\\.\pipe\{_pipeName}server";

			var openMode = ((_options & PipeOptions.Asynchronous) != 0 ? FILE_FLAG_OVERLAPPED : 0) |
			               ((_options & PipeOptions.WriteThrough) != 0 ? FILE_FLAG_WRITE_THROUGH : 0);

			var pipeMode = _transmissionMode == PipeTransmissionMode.Message ? PIPE_TYPE_MESSAGE : 0;

			var nativePipe = CreateNamedPipe(
				nativePipeName, 
				PIPE_ACCESS_INBOUND | openMode, 
				pipeMode, 
				_maxNumberOfServerInstances == -1 ? PIPE_UNLIMITED_INSTANCES : (uint)_maxNumberOfServerInstances, 
				0, 
				0,
				NMPWAIT_WAIT_FOREVER,                                      
				securityAttributesPtr);

			var error = Marshal.GetLastWin32Error();

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

		/// <summary>
		/// Waits for connection.
		/// </summary>
		/// <exception cref="ObjectDisposedException">NamedPipeServerStreams</exception>
		/// <inheritdoc cref="NamedPipeServerStream.WaitForConnection" />
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

		/// <summary>
		/// Waits for connection. cancelable.
		/// </summary>
		/// <returns><c>true</c> if connected, <c>false</c> otherwise.</returns>
		/// <remarks>
		/// <para>the original NamedPipeServerStream does still block and does not return on Dispose. this workaround requires PipeOptions.Asynchronous</para>
		/// </remarks>
		public bool WaitForConnectionCancelable() {
			// the original NamedPipeServerStream does still block and does not return on Dispose.
			// but the workaround requires PipeOptions.Asynchronous

			var r = _readPipe.BeginWaitForConnection(null, null);
			var w = _writePipe.BeginWaitForConnection(null, null);

			// possible DuplicateWaitObjectException: Duplicate objects in argument. in core3.1 /net5.0 
			if(r.AsyncWaitHandle != w.AsyncWaitHandle) 
				WaitHandle.WaitAll(new[] { r.AsyncWaitHandle, w.AsyncWaitHandle });
			else {
				w.AsyncWaitHandle.WaitOne();
			}
			
			if (_disposeFlag) return false; // canceled

			_readPipe.EndWaitForConnection(r);
			_writePipe.EndWaitForConnection(w);
			return true;
		}


		/// <summary>
		/// Disconnects this instance.
		/// </summary>
		/// <inheritdoc cref="NamedPipeServerStream.Disconnect" />
		public void Disconnect() {
			_readPipe.Disconnect();
			_writePipe.Disconnect();
		}

	}

}


