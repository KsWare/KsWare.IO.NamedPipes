using System;

namespace KsWare.IO.NamedPipes {

	public class PipeMsgEventArgs : EventArgs {

		public PipeMsgEventArgs() { }

		public PipeMsgEventArgs(string request) {
			Request = request;
		}

		public string Request { get; }

		public string Response { get; set; }


	}

}
