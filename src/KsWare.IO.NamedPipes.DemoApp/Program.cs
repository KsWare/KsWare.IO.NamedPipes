using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using KsWare.IO.NamedPipes;

namespace KsWare.NamedPipeDemo {

	public static class Program {

		static void Main(string[] args) {
			switch (args.FirstOrDefault()) {
				case null: break;
				case "StartEchoServer": EchoServer(args.Skip(1).ToArray()); return;
			}


//			NamedPipeServerStreamWaitForConnectionDisposeTest();
//			NamedPipeServerStreamWaitForConnectionCloseTest();
//			NamedPipeServerStreamWaitForConnectionDisconnectTest();

//			NamedPipeServerTests.StartServerAndDisposeTest();
//			NamedPipeClientTests.StartClientAndDisposeTest();
//			NamedPipeClientTests.StartServerAndClientAndDispose();
//			NamedPipeClientTests.StartServerAndClientAndSend();
//			NamedPipeClientTests.StartEchoServerAndClientAndSend();

//			NamedPipeClientTests.StartEchoServerAndMultipleClientsAndSend();

            // Console.WriteLine("Run Demo 1:");
            // Demo1.Run();
            // Console.WriteLine();
			Console.WriteLine("Run Demo 2:");
            Demo2.Run();
		}

		public static Process StartEchoServer(string pipeName,
			int maxNumberOfServerInstances,
			int initialNumberOfServerInstances) {

			var p = new Process {
				StartInfo = new ProcessStartInfo {
					FileName  = GetExecutable(),
					Arguments = $"StartEchoServer {pipeName} {maxNumberOfServerInstances} {initialNumberOfServerInstances}",
					UseShellExecute = true,
					Verb = "runas"
				}
			};
			p.Start();
			var startedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, $@"Global\{pipeName}");
			startedEvent.WaitOne();
			return p;
		}

		private static string GetExecutable() {
			// in .net core Location returns a DLL
			var file = Assembly.GetExecutingAssembly().Location;
			if (file.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
				file = file.Substring(0, file.Length - 4) + ".exe";
			return file;
		}

		private static void EchoServer(string[] args) {
			Console.WriteLine(@"EchoServer start");
			// {pipeName} {maxNumberOfServerInstances} {initialNumberOfServerInstances}
			var name = args.Length > 0 ? args[0] : "EchoServer";
			var max = args.Length > 1 ? int.Parse(args[1]) : 1;
			var num = args.Length > 2 ? int.Parse(args[2]) : 1;
			Console.Title = $"EchoServer [{name}]";

			var server = new NamedPipeServer(name, max, num,
			(s, e) => {
				switch (e.Request) {
					case "Ping": e.Response = "Pong"; break;
					case "Exit": e.Response="OK"; ((NamedPipeServerInstance)s).Dispose(); break;
					default:e.Response="ERROR unknown command"; break;
				}
			});

			_consoleCtrlHandler += sig => {
				if(sig== CtrlType.CTRL_CLOSE_EVENT) server.Dispose();
				return false;
			};
			SetConsoleCtrlHandler(_consoleCtrlHandler, true);

			server.Run();
			Console.WriteLine(@"EchoServer exit");
		}


		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);

		private delegate bool ConsoleCtrlDelegate(CtrlType sig);

		static ConsoleCtrlDelegate _consoleCtrlHandler;

		enum CtrlType {
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}
	}

}
