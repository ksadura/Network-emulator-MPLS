using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ManagementSystemGUI
{
    //State object for receiving messages
    class StateObject
    {
        public static readonly int BufferSize = 1024;
        public byte[] buffer;
        public StringBuilder receivedData = new StringBuilder();
        public Socket tempSocket = null;

        public StateObject() => buffer = new byte[BufferSize];
    }
    class ManagementSystem
    {
        
        //This field controls a thread
        private ManualResetEvent allDone;

        //Address, port and endPoint
        private IPAddress address;
        private IPEndPoint localEndPoint;
        private int port;

        //Dictionary that stores connected sockets and their names
        Dictionary<string, Socket> NodeSockets;

        protected Form1 window;

        //Constructor
        public ManagementSystem()
        {
            allDone = new ManualResetEvent(false);
            NodeSockets = new Dictionary<string, Socket>();
        }

        //Network nodes say hello to system
        public void GreetNode(Socket socket, StateObject state, string content)
        {
            string[] packet = content.Split(new char[] { '-', ' ' });

            if (packet[0].Equals("HELLO"))
            {
                AddLog(content);
                AvaiableNodes(packet[1],true);
                NodeSockets.TryAdd(packet[1], socket);
                SendDefaultEntries(state, socket, packet[1]);
            }
            else if (packet[0].Equals("RECEIVED"))
            {
                AddLog("ACK: " + content);
                UploadEntries(content);
                content = String.Empty;
            }
            
        }

        //Start the server
        public void Start(Form1 _window)
        {
            window = _window;
            address = IPAddress.Parse(ConfigMSystem.ADDRESS);
            port = ConfigMSystem.PORT;
            localEndPoint = new IPEndPoint(address, port);
            Socket listener = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult asyncResult)
        {
            allDone.Set();
            Socket tempListener = (Socket)asyncResult.AsyncState;
            Socket handler = tempListener.EndAccept(asyncResult);

            StateObject dataReader = new StateObject();
            dataReader.tempSocket = handler;

            handler.BeginReceive(dataReader.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), dataReader);
            tempListener.BeginAccept(new AsyncCallback(AcceptCallback), tempListener);
        }

        public void ReadCallback(IAsyncResult asyncResult)
        {
            StateObject dataReader = (StateObject)asyncResult.AsyncState;
            Socket handler = dataReader.tempSocket;

            string content = string.Empty;
            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(asyncResult);
            }
            catch (Exception e)
            {
                var router = NodeSockets.First(x => x.Value == handler);
                AddLog($"{router.Key} has been shutdown");
                NodeSockets.Remove(router.Key);
                AvaiableNodes(router.Key, false);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                return;
            }

            dataReader.receivedData.Clear();

            if (bytesRead > 0)
            {
                dataReader.receivedData.Append(Encoding.ASCII.GetString(dataReader.buffer, 0, bytesRead));
                content = dataReader.receivedData.ToString();
                //Console.WriteLine($"Read {content.Length} bytes from socket.\nData : {content}");
                GreetNode(handler, dataReader, content);
            }
            dataReader.receivedData.Clear();
            handler.BeginReceive(dataReader.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), dataReader);
        }

        public void Send(Socket handler, StateObject st, string data)
        {
            //Convert to bits
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            //Begin sending data to client
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        public void SendCallback(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                handler = (Socket)ar.AsyncState;
                //Complete sending
                int bytesSend = handler.EndSend(ar);
                string ip = NodeSockets.First(x => x.Value == handler).Key;
                AddLog($"New command has been sent to the node {ip} ({ConfigMSystem.GetNodeName(IPAddress.Parse(ip))})");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
        }
        public void AddLog(string s)
        {
            window.Invoke((MethodInvoker)delegate ()
            {
                window.AddLog(s);
            });
        }

        public void UploadEntries(string s)
        {
            window.Invoke((MethodInvoker)delegate ()
            {
                window.UploadEntriesList(s);
            });
        }

        public void Action(string packet)
        {
            Socket s = null;
            string[] parts = packet.Split(null);
            StateObject state = new StateObject();
            if (parts[0] == ManagementActions.ADD_MPLS_ENTRY)
            {
                s = NodeSockets[ConfigMSystem.GetIPAddres(parts[parts.Length - 2])];
            }
            else if (parts[0] == ManagementActions.REMOVE_MPLS_ENTRY)
            {
                s = NodeSockets[ConfigMSystem.GetIPAddres(parts[parts.Length - 3])];
            }
            Configure(packet, state, s);
        }

        public void AvaiableNodes(string nodeName, bool flag)
        {
            if (flag)
            {
                window.Invoke((MethodInvoker)delegate ()
                {
                    window.EnableButtons(nodeName);
                });
            }
            else
            {
                window.Invoke((MethodInvoker)delegate ()
                {
                    window.DisableButton(nodeName);
                });
            }
        }

        //Sending an upload to the routers
        public void Configure(string command, StateObject state, Socket socket)
        {
            Send(socket, state, command);
            //UploadEntries(command);
        }

        //Sending default routes and waiting for the ACK's
        public void SendDefaultEntries(StateObject state, Socket socket, string address)
        {
            byte[] bytes = new byte[256];
          
            foreach (var entry in ConfigMSystem.DefaultRoutes)
            {
                string[] splitted = entry.Split("@");
                if (splitted[splitted.Length - 2].Equals(ConfigMSystem.GetNodeName(IPAddress.Parse(address))))
                {
                    string command = entry.Replace("@", " ");
                    Send(socket, state, command);
                    Thread.Sleep(500);
                    socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(Callback), state);
                }
            }
        }

        private void Callback(IAsyncResult ar)
        {
            StateObject dataReader = ar.AsyncState as StateObject;
            Socket socket = dataReader.tempSocket;
            int read = 0;
            string content = "";
            try
            {
                read = socket.EndReceive(ar);
            }
            catch(Exception e)
            {
                var router = NodeSockets.First(x => x.Value == socket);
                AddLog($"{router.Key} has been shutdown");
                NodeSockets.Remove(router.Key);
                AvaiableNodes(router.Key, false);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return;
            }
            dataReader.receivedData.Clear();
            if (read > 0)
            {
                dataReader.receivedData.Append(Encoding.ASCII.GetString(dataReader.buffer, 0, read));
                content = dataReader.receivedData.ToString();
                AddLog("ACK: " + content);
                string [] parts = content.Split(" ");
                if (parts[0] == "RECEIVED")
                    UploadEntries(content);
            }

        }

    }
}
