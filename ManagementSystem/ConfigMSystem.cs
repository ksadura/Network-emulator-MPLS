using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace ManagementSystemGUI
{
    class ConfigMSystem
    {
        public static int PORT;
        public static string ADDRESS;
        private static string PATH;
        public static Dictionary<string, IPAddress> hosts = new Dictionary<string, IPAddress>();
        private static Dictionary<string, string> NODES = new Dictionary<string, string>();
        public static List<string> DefaultRoutes = new List<string>();

        public static void ReadConfig(string path)
        {
            PATH = path;
            XmlDocument doc = new XmlDocument();
            doc.Load(PATH);
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name.Equals("port"))
                {
                    PORT = int.Parse(node.InnerText);
                }
                else if (node.Name.Equals("address"))
                {
                    ADDRESS = node.InnerText;
                }
                else if (node.Name.Equals("hosts"))
                {
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        string[] splitted = n.InnerText.Split("@");
                        hosts.TryAdd(splitted[0], IPAddress.Parse(splitted[1]));
                    }
                }
                else if (node.Name.Equals("convert"))
                {
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        string[] splitted = n.InnerText.Split("=");
                        NODES.TryAdd(splitted[1], splitted[0]);
                    }
                }
                else if (node.Name.Equals("default_routes"))
                {
                    foreach(XmlNode n in node.ChildNodes)
                    {
                        DefaultRoutes.Add(n.InnerText);
                    }
                }
            }

        }
        //Parsing IP address to node name
        public static string GetNodeName(IPAddress address)
        {
            return NODES[address.ToString()];
        }

        //Parsing node name to IP address
        public static string GetIPAddres(string node)
        {
            var name = NODES.First(x => x.Value == node);
            return name.Key.ToString();
        }
    }
}
