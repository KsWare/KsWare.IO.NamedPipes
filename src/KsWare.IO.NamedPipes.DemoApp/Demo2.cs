using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KsWare.IO.NamedPipes;

namespace KsWare.NamedPipeDemo {

	
	internal class Demo2 {

		private static string pipeName = "Demo2Pipe";

		/// <summary>
		/// Starts a server with max 4 connections and 8 clients each sending 3 requests
		/// </summary>
		public static void Run() {
			Task.Run(() => Server());

			Task.Delay(300).Wait();

			var clients = new List<string>() {
				"Client 1",
				"Client 2",
				"Client 3",
				"Client 4",
				"Client 5",
				"Client 6",
				"Client 7",
				"Client 8"
			};

			Parallel.ForEach(clients, (c) => Client(c));
		}

		static void Server() {
			var server = new NamedPipeServer(pipeName, 4, 1);
			server.RequestReceived += (s, e) => e.Response = "Echo. " + e.Request;
			server.Start();
			Console.WriteLine("Server started");

			Task.Delay(10000).Wait();
			Console.WriteLine("Server shutdown");
			server.Dispose();
		}

		static void Client(string clientName) {
			try {
				using (var client = new NamedPipeClient(pipeName, 10000)) {
					var request = clientName + " Request a";
					var response = client.SendRequest(request);
					Console.WriteLine(response);
					Task.Delay(100).Wait();

					var request1 = clientName + " Request b";
					var response1 = client.SendRequest(request1);
					Console.WriteLine(response1);
					Task.Delay(100).Wait();

					var request2 = clientName + " Request c";
					var response2 = client.SendRequest(request2);
					Console.WriteLine(response2);
				}
			}
			catch (Exception ex) {
				Console.WriteLine($"{clientName} {ex.GetType().Name} {ex.Message}");
			}
		}
	}

}
