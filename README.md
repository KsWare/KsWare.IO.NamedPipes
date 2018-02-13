# KsWare.IO.NamedPipes #

A named pipe communication class library.

## Usage ##

Simple Example:

    var server = new NamedPipeServer(pipeName, 1, 1, (s, e) => e.Response = "Pong");
    var client = new NamedPipeClient(pipeName, 100);
    var response=client.SendRequest("Ping");

