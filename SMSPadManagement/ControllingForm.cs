using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SMSPadManagement;

namespace SMSPadManagement
{
    public partial class ControllingForm : Form
    {
        public ControllingForm()
        {
            InitializeComponent();
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            btn_start.Enabled = false;
            ServiceSMSPad smspadsv = new ServiceSMSPad();
            smspadsv.StarttoListen();
            
        }
    }
}
