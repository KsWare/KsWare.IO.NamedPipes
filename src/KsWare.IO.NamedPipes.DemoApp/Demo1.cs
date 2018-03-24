using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace KsWare.NamedPipeDemo
{
    class Demo1
    {
        private static string pipeName = "Demo1Pipe";

        public static void Run()
        {
            Task.Run(() => Server());

            Task.Delay(300).Wait();

            Client();
        }

        static void Server()
        {
            using (var server = new NamedPipeServerStream(pipeName))
            {
                server.WaitForConnection();

                var reader = new StreamReader(server);
                var writer = new StreamWriter(server);

                var received = reader.ReadLine();
                Console.WriteLine("Received from client: " + received);

                var toSend = "Hello, client.";
                writer.WriteLine(toSend);
                writer.Flush();
            }
        }

        static void Client()
        {
            using (var client = new NamedPipeClientStream(pipeName))
            {
                client.Connect(100);

                var writer = new StreamWriter(client);
                var request = "Hello, server.";
                writer.WriteLine(request);
                writer.Flush();

                var reader = new StreamReader(client);
                var response = reader.ReadLine();
                Console.WriteLine("Response from server: " + response);
            }
        }
    }
}
