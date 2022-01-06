using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeServer
{
    public class NetworkHelper
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 27001;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        public static void Start()
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }
        private static string ConvertString(char[,] array)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 3; k++)
                {

                    sb.Append(array[i, k]);
                    sb.Append('\t');
                    //var no = array[i, k];
                    //if (no >= 48 && no <= 57)
                    //    sb.Append((Convert.ToInt32(array[i, k]) - 48).ToString() + '\t');
                    //else
                    //    sb.Append((Convert.ToInt32(array[i, k])).ToString() + '\t');

                }
                sb.Append('\n');
            }
            return sb.ToString();
        }
        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }
        static char[,] Points = new char[3, 3] { { '1', '2', '3' }, { '4', '5', '6' }, { '7', '8', '9' } };
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);





            //
            Console.WriteLine("Received Text: " + text);
            var no = text[0];
            var symbol = text[1];
            var number = Convert.ToInt32(no) - 49;
            if (number >= 0 && number <= 2)
                Points[0, number] = symbol;
            else if (number >= 3 && number <= 5)
                Points[1, number - 3] = symbol;
            else if (number >= 6 && number <= 8)
                Points[2, number - 6] = symbol;


            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    Console.Write($"{Points[i, k]}   ");
                }
                Console.WriteLine();
                Console.WriteLine();
            }


            if (text != String.Empty) // Client requested time
            {
                var mydata = ConvertString(Points);
                byte[] data = Encoding.ASCII.GetBytes(mydata);
                // current.Send(data);
                foreach (var item in clientSockets)
                {
                    item.Send(data);
                }

                Console.WriteLine("Time sent to client");
            }
            else if (text.ToLower() == "exit") // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                clientSockets.Remove(current);
                Console.WriteLine("Client disconnected");
                return;
            }
            else
            {
                Console.WriteLine("Text is an invalid request");
                byte[] data = Encoding.ASCII.GetBytes("Invalid request");
                current.Send(data);
                Console.WriteLine("Warning Sent");
            }

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
    }

}


