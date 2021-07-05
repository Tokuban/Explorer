using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Explorer
{
    class Web2 : WebBrowser
    {
        MyExplorer me;
        public Web2(MyExplorer mee)
        {
            me = mee;
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x210)
            {
                // m1
                if ((int)m.WParam == 513)
                {
                    // clicking webbrowser restores path
                    if (me.addr.Controls[0] is TextBox)
                        me.UpdateAddressBar();
                    // dispose mousemenu
                    if (me.mouseMenu.Controls.Count > 0)
                    {
                        me.mf.Controls.Remove(me.mouseMenu);
                        me.mouseMenu.Dispose();
                    }
                }
            }
            base.WndProc(ref m);
        }
    }
}
