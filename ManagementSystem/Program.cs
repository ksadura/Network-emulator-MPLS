using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagementSystemGUI
{
    static class Program
    {
        static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                ConfigMSystem.ReadConfig(args[0]);
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(args[1]));
            }
            else
            {
            //    ConfigMSystem.ReadConfig(@"C:\Users\kenic\source\repos\ManagementSystemGUI\SystemConfig.xml");
            //    Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //    Application.EnableVisualStyles();
            //    Application.SetCompatibleTextRenderingDefault(false);
            //    Application.Run(new Form1(@"C:\Users\kenic\Desktop\TSST\projekt\tsst\Resources\iconMS.ico"));
            }
        }
    }
}
