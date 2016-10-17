using Channels.Networking.Libuv;
using Channels.Text.Primitives;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SampleServer
{
    public class TrivialClient : IDisposable
    {
        UvTcpClient client;
        UvThread thread;
        UvTcpConnection connection;

        internal Task SendAsync(string line)
        {
            
            try
            {
                if (connection == null)
                {
                    Console.WriteLine($"[client] (no connection; cannot send)");
                    return done;
                }
                else if (string.IsNullOrEmpty(line))
                {
                    return done;
                }
                else
                {
                    var buffer = connection.Output.Alloc();
                    Console.WriteLine($"[client] sending {line.Length} bytes...");
                    buffer.WriteAsciiString(line);
                    return buffer.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Program.WriteError(ex);
                return done;
            }
        }
        static readonly Task done = Task.FromResult(0);

        internal async Task ConnectAsync(IPEndPoint endpoint)
        {
            thread = new UvThread();
            client = new UvTcpClient(thread, endpoint);
            connection = await client.ConnectAsync();
            ReadLoop(); // will hand over to libuv thread
        }
        internal async void ReadLoop()
        {
            Console.WriteLine("[client] read loop started");
            try
            {
                while (true)
                {
                    var readResult = await connection.Input.ReadAsync();
                    if (readResult.Buffer.IsEmpty && (connection == null || readResult.IsCompleted))
                    {
                        Console.WriteLine("[client] input ended");
                        break;
                    }

                    var s = readResult.Buffer.GetAsciiString();
                    connection.Input.Advance(readResult.Buffer.End, readResult.Buffer.End);

                    Console.Write("[client] received: ");
                    Console.WriteLine(s);
                }
            }
            catch(Exception ex)
            {
                Console.Write("[client] read loop exploded");
                Program.WriteError(ex);
            }
            finally
            {
                Console.WriteLine("[client] read loop ended");
            }
        }
        public void Close()
        {
            if (connection != null) Close(connection);
            connection = null;
            // client.Dispose(); //
            thread?.Dispose();
            thread = null;
        }
        public void Dispose() => Dispose(true);
        private void Dispose(bool disposing)
        {
            if (disposing) Close();
        }

        private void Close(UvTcpConnection connection, Exception error = null)
        {
            Console.WriteLine("[client] closing connection...");
            connection.Input.Complete(error);
            connection.Output.Complete(error);
            Console.WriteLine("[client] connection closed");
        }
    }
}
