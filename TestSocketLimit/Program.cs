using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TestSocketLimit
{
    class Program
    {
        static string PlayloadFormat = "GET / HTTP/1.1\r\nConnection: Keep-Alive\r\nAccept: text/html, application/xhtml+xml, */*\r\nAccept-Encoding: gzip, deflate\r\nAccept-Language: en-US,en;q=0.5\r\nHost: {0}\r\nMax-Forwards: 10\r\nUser-Agent: TestSockets\r\n\r\n";

        static void Main(string[] args)
        {
            try
            {
                TestSockets();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void TestSockets()
        {
            var clients = new List<TcpData>();

            foreach (var stamp in new[] { "bay", "am2", "blu", "db3" })
            {
                var i = 1;
                while (true)
                {
                    try
                    {
                        var hostName = string.Format("waws-{0}-{1:000}.azurewebsites.net", stamp, i);
                        Console.WriteLine("Connect to " + hostName);
                        var client = new TcpClient();
                        client.Connect(hostName, 80);

                        clients.Add(new TcpData
                        {
                            HostName = hostName,
                            Client = client,
                            Stream = client.GetStream()
                        });

                        i += 2;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }

            foreach (var client in clients)
            {
                Console.WriteLine("Write to " + client.HostName);
                var request = String.Format(PlayloadFormat, client.HostName);
                var data = Encoding.UTF8.GetBytes(request);
                client.Stream.Write(data, 0, data.Length);
            }

            foreach (var client in clients)
            {
                Console.WriteLine("Read from " + client.HostName);
                var reader = new StreamReader(client.Stream);
                var data = reader.ReadLine();
                Console.WriteLine(data);
            }

            foreach (var client in clients)
            {
                client.Stream.Close();
                client.Client.Close();
            }
        }

        class TcpData
        {
            public string HostName { get; set; }
            public TcpClient Client { get; set; }
            public NetworkStream Stream { get; set; }
        }
    }
}
