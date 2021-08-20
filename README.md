# KsWare.IO.NamedPipes #

A named pipe communication class library.

Solves the problem of communication on different access levels. 

[![Build status](https://ci.appveyor.com/api/projects/status/5i23fkavsbeyk9e3/branch/master?svg=true)](https://ci.appveyor.com/project/KsWare/ksware-io-namedpipes/branch/master)

## Usage ##

Simple Example:

    var server = new NamedPipeServer(pipeName, 1, 1, (s, e) => e.Response = "Pong");
    var client = new NamedPipeClient(pipeName, 100);
    var response=client.SendRequest("Ping");

