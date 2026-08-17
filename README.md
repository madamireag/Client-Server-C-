# Client-Server Messaging System

A TCP-based client-server application in C# built as a technical onboarding assignment. Two clients and a server communicate asynchronously, exchanging messages through a shared file that acts as the message bus.

## How it works

- The **server** listens on `127.0.0.1:9999` and accepts multiple client connections asynchronously
- **Clients** connect to the server, then write messages to a shared text file in the format `Sender MessageContent Recipient`
- Each participant polls the file on a configurable interval, reads messages addressed to them, prints them to the console, and writes back an acknowledgement
- Already-processed messages are removed from the file to avoid duplicate reads

## Tech Stack

C# · .NET · TCP Sockets · Multithreading ·
