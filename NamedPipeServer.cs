using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KsWare.IO.NamedPipes {

	/// <summary>
	/// Provides a named pipe server with multiple clients support.
	/// </summary>
	public class NamedPipeServer {

		private readonly string _pipeName;
		private readonly int _maxNumberOfServerInstances;
		private readonly int _initialNumberOfServerInstances;

		private readonly List<NamedPipeServerInstance> _servers = new List<NamedPipeServerInstance>();
		private readonly ManualResetEvent _dispose=new ManualResetEvent(false);
		private EventWaitHandle _readyEvent;

		public NamedPipeServer(string pipeName) : this(pipeName, 1, 1) { }

		public NamedPipeServer(string pipeName, int maxNumberOfServerInstances, int initialNumberOfServerInstances) {
			_pipeName                   = pipeName;
			_maxNumberOfServerInstances = maxNumberOfServerInstances;
			_initialNumberOfServerInstances = initialNumberOfServerInstances;
			_readyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, $@"Global\{pipeName}");
		}

		public NamedPipeServer(string pipeName, int maxNumberOfServerInstances, int initialNumberOfServerInstances,
			EventHandler<PipeMsgEventArgs> messageEventHandler):this(pipeName,maxNumberOfServerInstances, initialNumberOfServerInstances) {
			RequestReceived += messageEventHandler;
			Start();
		}

		public event EventHandler<PipeMsgEventArgs> RequestReceived = delegate { };

		public void Start() {
			lock (_servers) {
				while (_servers.Count< _initialNumberOfServerInstances) {
					CreateServerInstance();
				}
			}
			_readyEvent.Set();
		}

		public void Run() {
			Start();
			_dispose.WaitOne();
		}

		public void Dispose() { Dispose(true); GC.SuppressFinalize(this);}

		private void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				CleanServers(true);
				_dispose.Set();
			}
		}

		private void AtConnected(object sender, EventArgs eventArgs) {
			// Run clean servers anyway
			CleanServers(false);

			lock (_servers) {
				// Start a new server instance only when the number of server instances
				// is smaller than maxNumberOfServerInstances
				if (_servers.Count < _maxNumberOfServerInstances) {
					CreateServerInstance();
				}				
			}

		}

		private void AtDisconnected(object sender, EventArgs eventArgs) {
			lock (_servers) {
				_servers.Remove((NamedPipeServerInstance) sender);
			}

			// Run clean servers anyway
			CleanServers(false);
		}

		private void CreateServerInstance() {
			var server = new NamedPipeServerInstance(_pipeName, _maxNumberOfServerInstances);
			server.Connected += AtConnected;
			server.RequestReceived += (s, e) => RequestReceived.Invoke(s, e);
			server.Disconnected += AtDisconnected;
			server.Start();
			_servers.Add(server);
		}

		/// <summary>
		/// A routine to clean NamedPipeServerInstances. When disposeAll is true,
		/// it will dispose all server instances. Otherwise, it will only dispose
		/// the instances that are completed, canceled, or faulted.
		/// PS: disposeAll is true only for this.Dispose()
		/// </summary>
		/// <param name="disposeAll"></param>
		private void CleanServers(bool disposeAll) {
			lock (_servers) {
				if (disposeAll) {
					foreach (var server in _servers) {
						server.Dispose();
					}
				}
				else {
					for (int i = _servers.Count - 1; i >= 0; i--) {
						if (_servers[i]                   == null) {
							_servers.RemoveAt(i);
						}
						else if (_servers[i].Task         != null &&
								 (_servers[i].Task.Status == TaskStatus.RanToCompletion ||
								  _servers[i].Task.Status == TaskStatus.Canceled        ||
								  _servers[i].Task.Status == TaskStatus.Faulted)) {
							_servers[i].Dispose();
							_servers.RemoveAt(i);
						}
					}
				}				
			}
		}
	}

}
