using System;
using System.Net.Sockets;
using System.Text;

namespace Tools
{
    public class MPLSSocket: Socket
    {
        public MPLSSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) :
           base(addressFamily, socketType, protocolType)
        {
            ReceiveTimeout = 5000000;
        }
        public MPLSPackage Receive()
        {
            var buffer = new byte[256];
            int bytes = Receive(buffer);
            byte[] receivedBytes = new byte[bytes];
            Array.Copy(buffer, receivedBytes, bytes);
            if (Encoding.ASCII.GetString(receivedBytes, 0, bytes).Substring(0, 9).Equals("KEEPALIVE"))
            {
                return null;
            }

            return MPLSPackage.FromBytes(receivedBytes);
        }

        public int Send(MPLSPackage package)
        {
            return Send(package.ToBytes());
        }
    }
}
