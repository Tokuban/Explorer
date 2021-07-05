using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Explorer
{
    class Form2 : Form
    {
        MyExplorer me;
        public Form2(MyExplorer mee)
        {
            me = mee;
            base.DoubleBuffered = true;
        }
        protected override void WndProc(ref Message m)
        {
            //Console.WriteLine(m.WParam);
            if (m.Msg == 0x210)
            {
                if ((int)m.WParam == 0x1020b)
                {
                    me.TabChange("back", me.currentTab.uriIndex - 1);
                }
                else if ((int)m.WParam == 131595)
                {
                    me.TabChange("forward", me.currentTab.uriIndex + 1);
                }
            }
            base.WndProc(ref m);
        }
    }
}
