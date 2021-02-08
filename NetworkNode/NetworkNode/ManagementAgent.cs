using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Tools;
using static NetworkNodes.NetworkNode;


namespace NetworkNodes
{
    public class ManagementAgent
    {
        public NetworkNodeConfig Config { get; set; }

        public Socket ConnectedSocket { get; set; }

        public NetworkNodeRoutingTables RoutingTable { get; set; }

        private ManualResetEvent mre = new ManualResetEvent(false);

        public ManagementAgent(NetworkNodeConfig networkNodeConfig)
        {
            Config = networkNodeConfig;
            RoutingTable = new NetworkNodeRoutingTables();
        }

        public void StartTask()
        {
            Task.Run(() => Start());
        }

        public void Start()
        {
            while (true)
            {
                AddLog("Management agent starting...", LogType.Information);

                ConnectToMS();

                if (ConnectedSocket == null)
                {
                    continue;
                }

                while (true)
                {
                    try
                    {
                        byte[] buffer = new byte[512];
                        int bytes = ConnectedSocket.Receive(buffer);

                        var message = Encoding.ASCII.GetString(buffer, 0, bytes);

                        Task.Run(() => HandleMessage(message));
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode != SocketError.TimedOut)
                        {
                            if (e.SocketErrorCode == SocketError.Shutdown || e.SocketErrorCode == SocketError.ConnectionReset)
                            {
                                AddLog("Connection to MS broken!", LogType.Error);
                                break;
                            }

                            else
                            {
                                AddLog($"{e.Source}: {e.SocketErrorCode}", LogType.Error);
                            }
                        }
                    }
                }
            }
        }

        private void ConnectToMS()
        {
            AddLog($"Connecting to MS at {Config.ManagementSystemAddress}:{Config.ManagementSystemPort}", LogType.Information);
            while (true)
            {
                Socket socket = new Socket(Config.ManagementSystemAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveTimeout = 20000;

                try
                {
                    var result = socket.BeginConnect(new IPEndPoint(Config.ManagementSystemAddress, Config.ManagementSystemPort), null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(5000, true);
                    if (success)
                    {
                        socket.EndConnect(result);
                    }
                    else
                    {
                        socket.Close();
                        AddLog("Connection to MS not established - timeout...", LogType.Error);
                        continue;
                    }
                }
                catch (Exception)
                {
                    AddLog("Retrying...", LogType.Information);
                }

                try
                {
                    AddLog($"Sending hello to MS...", LogType.Information);
                    socket.Send(Encoding.ASCII.GetBytes($"{ManagementActions.HELLO}-{NetworkNodeConfig.NodeAddress}"));
                    AddLog("Estabilished connection with MS", LogType.Information);
                    ConnectedSocket = socket;
                    break;
                }
                catch (Exception)
                {
                    AddLog("Couldn't connect to MS!", LogType.Error);
                }
            }
        }


        private void HandleMessage(string message)
        {
            AddLog($"Message received from MS: {message}", LogType.Information);
            var action = message.Split(" ")[0].Trim();
            var data = message.Replace($"{action} ", "");
            if (action == ManagementActions.ADD_MPLS_ENTRY)
            {
                string tmp_message = RoutingTable.HandleModifyForwardingTable(action, new MplsTableRow(data, false));
                if (tmp_message != "")
                {
                    ConnectedSocket.Send(Encoding.ASCII.GetBytes(tmp_message));
                }
            }
            else if (action == ManagementActions.REMOVE_MPLS_ENTRY)
            {
                string tmp_message = RoutingTable.HandleModifyForwardingTable(action, new MplsTableRow(data, true));
            }
        }

        private void AddLog(string log, LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            log = $"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond.ToString().PadLeft(3, '0')}] {log}";
            Console.WriteLine(log);
        }
    }
}
