using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMSPadManagement
{
    static class Program
    {
      public  static int state = 1;
            
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (state == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ControllingForm());
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new ServiceSMSPad()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
