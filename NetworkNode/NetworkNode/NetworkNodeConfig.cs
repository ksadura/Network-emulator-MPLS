using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System;
using System.Xml.Linq;

namespace NetworkNodes
{
    public class NetworkNodeConfig
    {
        public IPAddress ManagementSystemAddress { get; set; }

        public ushort ManagementSystemPort { get; set; }

        public string NodeName { get; set; }

        public IPAddress CloudAddress { get; set; }

        public ushort CloudPort { get; set; }

        public static IPAddress NodeAddress;

        public static NetworkNodeConfig ParseConfig(string FileName)
        {
            var config = new NetworkNodeConfig();
	    XDocument doc = new XDocument();
            doc = XDocument.Load(FileName);

            config.ManagementSystemAddress = IPAddress.Parse(doc.Element("NodeConfig").Element("ManagementAddress").Value);
            config.ManagementSystemPort = ushort.Parse(doc.Element("NodeConfig").Element("ManagementPort").Value);
            config.NodeName = doc.Element("NodeConfig").Element("NodeName").Value;
            NodeAddress = IPAddress.Parse(doc.Element("NodeConfig").Element("NodeAddress").Value);
            config.CloudAddress = IPAddress.Parse(doc.Element("NodeConfig").Element("CloudAddress").Value);
            config.CloudPort = ushort.Parse(doc.Element("NodeConfig").Element("CloudPort").Value);
	    Console.Title = config.NodeName;

            return config;
        }
    }
}
