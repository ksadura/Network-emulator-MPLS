using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Host.MPLSRelated;

namespace Host
{
    class Host
    {
        public static MPLSSocket established_socket { get; set; }
        public static int sent_packages = 0;

        static void Main(string[] args)
        {
            bool flag = true;
            string Handler;
            string[] Parameters = new string[2];
            HostParseConfig host = new HostParseConfig();
            if (args.Length != 0) { host = HostParseConfig.LoadFromConfigFile(args[0]); }
            else { Console.WriteLine("Błędny śćieżka do pliku konfiguracyjnego"); host = HostParseConfig.LoadFromConfigFile("./HostConfigExample.xml"); }
            Console.Clear();
            Task.Run(() => EstablishCloudConnection(host));
            Console.Title = host.host_name;
            Console.WriteLine("Wpisz help w celu wyświetlenia wszystkich dostępnych komend\n");
            while (flag)
            {
                Handler = Console.ReadLine().ToLower();
                if (Handler.Contains("sendmessage"))
                {
                    int index_of_message = 0;
                    int index_of_quanity = 0;
                    index_of_message = Handler.IndexOf("-m");
                    index_of_quanity = Handler.IndexOf("-q");
                    if (index_of_message < index_of_quanity)
                    {
                        Parameters[0] = Handler.Substring(index_of_message + 2, index_of_quanity - index_of_message - 2);
                        Parameters[1] = Handler.Substring(index_of_quanity + 2);
                        Handler = Handler.Substring(0, index_of_message - 1);
                        Parameters[0] = Parameters[0].Trim();
                        Parameters[1] = Parameters[1].Trim();
                        Handler = Handler.Trim();
                    }
                    else if(index_of_message > index_of_quanity)
                    {
                        Parameters[0] = Handler.Substring(index_of_message + 2);
                        Parameters[1] = Handler.Substring(index_of_quanity + 2, index_of_message - index_of_quanity - 2);
                        Handler = Handler.Substring(0, index_of_quanity - 1);
                        String.Concat(Parameters[0].Where(c => !Char.IsWhiteSpace(c)));
                        String.Concat(Parameters[1].Where(c => !Char.IsWhiteSpace(c)));
                        String.Concat(Handler.Where(c => !Char.IsWhiteSpace(c)));
                    }
                    else if(index_of_message==-1 || index_of_quanity==-1 || index_of_message==index_of_quanity)
                    {
                        if (Handler.IndexOf("sendmessage") == 0)
                        {
                            Handler = "sendmessage";
                            Parameters = null;
                        }
                    }

                }
                switch (Handler)
                {
                    case "help":
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.WriteLine("Dostępne komendy:");
                        Console.WriteLine("sendmessage -m <Message> -q <Ilość wysłanych wiadomości>");
                        Console.WriteLine("help");
                        Console.WriteLine("clearconsole");
                        Console.WriteLine("exit");
                        Console.ResetColor();
                        break;
                    case "sendmessage":
                        if (Parameters != null)
                        {
                            if (int.Parse(Parameters[1]) > 0)
                            {
                                string user_choice = null;
     
                                Console.WriteLine("Wybierz hosta do którego chcesz wysłać wiadomość:");
                                foreach(OtherHostsInfo i in host.other_hosts)
                                {
                                    Console.WriteLine($"{host.other_hosts.IndexOf(i) + 1}.{i.other_host_name}:{i.IP}");
                                }
                                user_choice = Console.ReadLine();
                                if (user_choice.All(Char.IsDigit) == true)
                                {
                                    if (int.Parse(user_choice) > 0 && int.Parse(user_choice) <= host.other_hosts.Count)
                                    {
                                        string message = Parameters[0];
                                        int how_many = int.Parse(Parameters[1]);
                                        int index_number = int.Parse(user_choice);
                                        Task.Run(async () =>
                                        {
                                            for (int i = 0; i < how_many; i++)
                                            {
                                                sendMessage(host, index_number-1 ,message);

                                                await Task.Delay(TimeSpan.FromMilliseconds(2000));
                                            }
                                        });
                                    }
                                    else
                                        Console.WriteLine("Wybrany element nie istnieje na liście");
                                }
                                else
                                    Console.WriteLine("Podano błędny numer na liście hostów");
                            }
                            else
                                Console.WriteLine("Podano błędne parametry");
                        }
                        else
                        {
                            Console.WriteLine("Błędna forma wiadomości wpisz help w celu sprawdzenia poprawnej formuły");
                            return;
                        }
                        break;
                    case "clearconsole":
                        Console.Clear();
                        break;
                    case "exit":
                        flag = false;
                        break;
                }
            }

        }
        public static void sendMessage(HostParseConfig host, int index, string payload)
        {
            if(established_socket == null || !established_socket.Connected)
                return;

            MPLSPackage mpls_package = new MPLSPackage();

            mpls_package.ID = sent_packages++;
            mpls_package.DestinationIP = host.other_hosts[index].IP;
            mpls_package.Payload = payload;
            mpls_package.Port = host.output_port;
            mpls_package.PrevIP = host.IP; //tutaj zmiana z TTL
            mpls_package.SourceIP = host.IP;

            try
            {
                established_socket.Send(mpls_package.ToBytes());
                AddLogInfo($"Wysłano pakiet : {mpls_package.Packet_Information()}");
            }
            catch(Exception e)
            {
                AddLogInfo($"Nie udało się wysłać pakietu: {e.Message}");
            }
            
        }
        public static void listenMessage(HostParseConfig host)
        {
            while (true)
            {
                while (established_socket == null || !established_socket.Connected)
                {
                    AddLogInfo("Próba wznowienia połączenia z chmurą kablową");
                    EstablishCloudConnection(host);
                }

                try
                {
                    MPLSPackage package = established_socket.Receive();

                    if (package != null)
                    {
                        AddLogInfo($"Otrzymano pakiet: {package.Packet_Information()}");
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.TimedOut)
                    {
                        if (e.SocketErrorCode == SocketError.Shutdown || e.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            AddLogInfo("Zerwano połączenie z chmurą kablową!");
                            continue;
                        }

                        else
                        {
                            AddLogInfo("Nie udało się połączyć z chmurą kablową!");
                        }
                    }
                }

            }
        }
        public static MPLSSocket EstablishCloudConnection(HostParseConfig host)
        {
            AddLogInfo($"Trwa łączenie z chmurą kablową {host.cloud_IP}:{host.cloud_port}");
            try
            {
                established_socket = new MPLSSocket(host.cloud_IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                established_socket.Connect(new IPEndPoint(host.cloud_IP, host.cloud_port));
                established_socket.Send(Encoding.ASCII.GetBytes($"HELLO-{host.IP}"));
                Task.Run(() => listenMessage(host));
                AddLogInfo("Zestawiono połączenie z chmurą kablową");

            }
            catch (Exception)
            {
                AddLogInfo("Nie udało się połączyć z chmurą kablową");
            }
            return established_socket;

        }
        public static void AddLogInfo(string info)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond.ToString().PadLeft(3, '0')}] {info}");
            Console.ResetColor();
        }

    }
}
