using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Explorer
{
    class MyTab
    {
        public List<Uri> uris = new List<Uri>();
        public int uriIndex = -1;
        Form2 mf;
        Web2 wb;
        MyExplorer me;
        int tW;
        int xW;
        public Button tab;
        public Button x;
        public Panel p;
        bool isMouseDown = false;
        int xOffset = 0;
        public MyTab(MyExplorer mee, Form2 mff, Web2 wbb, int tWW, int xWW)
        {
            mf = mff;
            wb = wbb;
            me = mee;
            tW = tWW;
            xW = xWW;
        }
        // duplicate constructor
        public MyTab(MyExplorer mee, Form2 mff, Web2 wbb, int tWW, int xWW, List<Uri> uriis, int idx)
        {
            mf = mff;
            wb = wbb;
            me = mee;
            tW = tWW;
            xW = xWW;
            uris = uriis;
            uriIndex = idx;
        }
        public void CreateTab(string dupl="")
        {
            p = new Panel();
            tab = new Button();
            x = new Button();

            p.Size = new Size(tW + xW, xW);
            tab.Size = new Size(tW, xW);
            x.Size = new Size(xW, xW);

            tab.Text = "Documents";
            x.Text = "X";

            // test dragging
            tab.MouseMove += (s, e) =>
            {
                //Console.WriteLine("moving");
                if (isMouseDown)
                {
                    p.Left = mf.PointToClient(Control.MousePosition).X - xOffset;
                    // check if we should move adjacent tab
                    me.CheckAdjacentTab();
                    //mf.Refresh();  // causes jitter!! but eliminates artifacts
                    mf.Update();  // eliminates MOST artifacts
                }
            };
            tab.MouseUp += (s, e) => {
                isMouseDown = false;
                p.Left = me.tabs.IndexOf(this) * p.Width + 30;
            };
            // Changing tabs, update old tab
            tab.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isMouseDown = true;
                    xOffset = e.Location.X;
                    p.BringToFront();
                }
                    
                // new logic
                if (me.currentTab != this)
                {
                    me.currentTab.tab.BackColor = Color.FromArgb(-986896);
                    me.currentTab.x.BackColor = Color.FromArgb(-986896);
                    me.currentTab = this;
                    tab.BackColor = Color.FromArgb(255, 255, 255);
                    x.BackColor = Color.FromArgb(255, 255, 255);
                    me.TabChange("changeTab", uriIndex);
                }
                if (e.Button == MouseButtons.Right)
                {
                    if (me.FullCreateMenu())
                    {
                        me.CreateMenuButton("Duplicate", DuplicateButton);
                        me.CreateMenuButton("Move right", MoveRight);
                    }
                }
            };
            x.Click += (s, e) =>
            {
                // destroy panel
                mf.Controls.Remove(p);
                p.Dispose();
                // move tabs appropriately
                me.tabs.Remove(this);
                me.MoveTabs2();
                // what should be the new currentTab? last one by default for now
                me.currentTab.tab.BackColor = Color.FromArgb(-986896);
                me.currentTab.x.BackColor = Color.FromArgb(-986896);

                // if zero tabs left, close application?
                if (me.tabs.Count == 0)
                {
                    Application.Exit();
                    return;  // we actually need to do this?
                }

                me.currentTab = me.tabs.Last();
                me.TabChange("changeTab", me.currentTab.uriIndex);
                me.currentTab.tab.Focus();
                me.currentTab.tab.BackColor = Color.FromArgb(255, 255, 255);
                me.currentTab.x.BackColor = Color.FromArgb(255, 255, 255);
            };

            p.Controls.Add(tab);
            p.Controls.Add(x);

            x.Left = tW;

            mf.Controls.Add(p);
            
            p.Left = me.tabStartX;
            me.tabStartX += tW + xW;
            // then make this tab the current tab
            if (me.currentTab != null)
            {
                me.currentTab.tab.BackColor = Color.FromArgb(-986896);
                me.currentTab.x.BackColor = Color.FromArgb(-986896);
            }
            me.currentTab = this;

            if (dupl == "dupl")
            {
                wb.Navigate(uris.Last());
            }
            else
            {
                // when creating a tab, always go to base (document)
                wb.Navigate(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents"));
            }

            // set focus and highlight to tab
            tab.Focus();
            tab.BackColor = Color.FromArgb(255, 255, 255);
            x.BackColor = Color.FromArgb(255, 255, 255);
        }
        private void DuplicateButton(object s, MouseEventArgs e)
        {
            MyTab dupl = new MyTab(me, mf, wb, tW, xW, new List<Uri>(uris), uriIndex);
            dupl.CreateTab("dupl");
            me.tabs.Insert(me.tabs.IndexOf(this) + 1, dupl);
            me.MoveTabs2();
        }
        private void MoveRight(object s, MouseEventArgs e)
        {
            Console.WriteLine("in the future move tab right");
        }
    }
}
