using System;
using System.Threading;
using System.Threading.Tasks;


namespace CableCloud
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                ConfigCloud.ReadConfig(args[0]);
                var cloud = Task.Run(() => new FiberCloud().StartCloud());
                var terminal = Task.Run(() => new Terminal().Start());

                cloud.Wait();
                terminal.Wait();
                //new Thread(new FiberCloud().StartCloud).Start();
            }

        }
    }
}
