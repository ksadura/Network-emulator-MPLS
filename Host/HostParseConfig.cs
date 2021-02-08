using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace Host
{
    public class HostParseConfig
    {
        public string host_name { get; set;  }
        public ushort output_port { get; set; }
        public IPAddress IP { get; set; }
        public IPAddress cloud_IP { get; set; }
        public ushort cloud_port { get; set; }
        public List<OtherHostsInfo> other_hosts { get; set; }

        public HostParseConfig()
        {
            other_hosts = new List<OtherHostsInfo>();
        }
        public static HostParseConfig LoadFromConfigFile(string filename)
        {
            HostParseConfig hostconfig=new HostParseConfig();
            XDocument doc = XDocument.Load(filename);

            hostconfig.host_name=doc.Element("HostConfig").Element("HostName").Value;
            hostconfig.cloud_IP = IPAddress.Parse(doc.Element("HostConfig").Element("CloudIP").Value);
            hostconfig.cloud_port = ushort.Parse(doc.Element("HostConfig").Element("CloudPort").Value);
            hostconfig.IP = IPAddress.Parse(doc.Element("HostConfig").Element("IP").Value);
            hostconfig.output_port = ushort.Parse(doc.Element("HostConfig").Element("OutputPort").Value);

            for(int i = 0; i < int.Parse(doc.Element("HostConfig").Element("OtherHosts").Element("HostCount").Value); i++)
            {
                var temp = doc.Element("HostConfig").Element("OtherHosts").Element("Host" + i.ToString()).Value.Split("@");
                hostconfig.other_hosts.Add(new OtherHostsInfo(IPAddress.Parse(temp[0]), temp[1]));
            }

            return hostconfig;

        }
    }
}
