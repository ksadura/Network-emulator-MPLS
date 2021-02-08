using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CableCloud
{
    class Terminal
    {
        private const string SPOIL = "spoil";
        private const string RESTORE = "restore";
        private string[] parameters;
        private string[] methods;
        private string[] routers = new string[] { "R1", "R2", "R3", "R4", "R5" };

        public Terminal() => methods = new string[] { SPOIL, RESTORE };

        public void Start()
        {
            while (true)
            {
                while (true)
                {
                    string s = Console.ReadLine();
                    parameters = s.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parameters.Length != 3 || !methods.Contains(parameters[0]))
                    {
                        Console.WriteLine("Bad syntax. Try again");
                    }
                    else if (!routers.Contains(parameters[1]) || !routers.Contains(parameters[2]))
                        Console.WriteLine("Unavailable routers");
                    else { break; }
                }
                switch (parameters[0].ToLower())
                {
                    case SPOIL:
                        RoutingTable.SpoilOrRestoreLink(parameters[1].ToUpper(), parameters[2].ToUpper(), 0);
                        break;
                    case RESTORE:
                        RoutingTable.SpoilOrRestoreLink(parameters[1].ToUpper(), parameters[2].ToUpper(), 1);
                        break;
                }
            }
        }

    }
}
