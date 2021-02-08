using System;
using System.Net;
namespace Host
{
    public class OtherHostsInfo
    {
        public IPAddress IP { get; set; }
        public string other_host_name { get; set; }
        public OtherHostsInfo()
        {
        }

        public OtherHostsInfo(IPAddress IP, string other_host_name)
        {
            this.IP = IP;
            this.other_host_name = other_host_name;
        }
    }
}
