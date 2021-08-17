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
    // HELLO IS THE PUSH WORKING
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            MyExplorer me = new MyExplorer();
            me.Init();
        }
    }
    
    class MyExplorer
    {
        public Web2 wb;
        public Form2 mf;
        public MyTab currentTab;
        public int tabStartX = 30;
        int tabWidth = 80;
        int xWidth = 30;
        public Panel addr;
        string action = "";
        public List<MyTab> tabs = new List<MyTab>();
        Panel cuts;
        public Panel mouseMenu;
        
        public void Init()
        {
            // basic form config
            mf = new Form2(this);
            // default size when minimized
            mf.Size = new Size(Screen.FromControl(mf).Bounds.Width / 4 * 3, Screen.FromControl(mf).Bounds.Height / 4 * 3);
            
            mf.WindowState = FormWindowState.Maximized;

            // mousemenu config
            mouseMenu = new Panel();

            cuts = new Panel();
            cuts.Name = "cuts";
            mf.ClientSizeChanged += (s, e) =>
            {
                wb.Height = mf.Height - wb.Location.Y - 30;
                wb.Width = mf.Width - cuts.Width - 20;
                cuts.Height = mf.Height - cuts.Location.Y - 30;
                addr.Size = new Size(mf.Width - addr.Location.X, xWidth);
            };

            // + button
            Button plus = new Button();
            plus.Text = "+";
            plus.Size = new Size(xWidth, xWidth);
            plus.Click += (s, e) =>
            {
                CreateTabClass();
            };
            mf.Controls.Add(plus);

            // address bar
            addr = new Panel();
            addr.Location = new Point(xWidth * 2, xWidth);
            addr.Size = new Size(mf.Width - addr.Location.X, xWidth);
            addr.BackColor = Color.FromArgb(255, 255, 255);  // back also changes all buttons, maybe change?
            addr.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (FullCreateMenu())
                        CreateMenuButton("Copy address", (object ss, MouseEventArgs ee) => Clipboard.SetText(wb.Url.AbsolutePath));
                }
                if (e.Button == MouseButtons.Left)
                {
                    // destroy path
                    ClearControls(addr);

                    // turn into a textfield
                    TextBox tb = new TextBox();
                    tb.KeyDown += (ss,ee) =>
                    {
                        if (ee.KeyCode == Keys.Enter)
                        {
                            wb.Navigate(tb.Text);
                            tb.Dispose();
                        }
                    };

                    addr.Controls.Add(tb);
                    tb.Focus();
                    tb.MinimumSize = new Size(xWidth, xWidth);
                    tb.Size = addr.Size;
                    tb.Text = wb.Url.AbsolutePath;
                    tb.SelectAll();
                }
            };
            mf.Controls.Add(addr);

            // common shortcuts
            cuts.Size = new Size(150, 0);
            cuts.Click += (s, e) =>
            {
                if (addr.Controls[0] is TextBox)
                    UpdateAddressBar();
                // dispose mousemenu
                if (mouseMenu.Controls.Count > 0)
                {
                    mf.Controls.Remove(mouseMenu);
                    mouseMenu.Dispose();
                }
            };
            mf.Controls.Add(cuts);
            cuts.Top = xWidth * 2;
            // shortcuts
            string user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            CreateShortcut(Path.Combine(user, "Documents"));
            CreateShortcut(Path.Combine(user, "Downloads"));
            CreateShortcut(Path.Combine(user, "Pictures"));

            // webbrowser
            wb = new Web2(this);
            wb.Size = new Size(1920, 500);
            wb.Location = new Point(cuts.Width, xWidth * 2);
            wb.Navigated += (s, e) =>
            {
                OnUrlChange();
            };
            
            mf.Controls.Add(wb);
            CreateTabClass();

            // back button
            Button back = new Button();
            back.Text = "<";
            back.Size = new Size(xWidth, xWidth);
            back.Top = xWidth;
            back.Click += (s, e) =>
            {
                TabChange("back", currentTab.uriIndex - 1);
            };
            mf.Controls.Add(back);

            // forward button
            Button forward = new Button();
            forward.Text = ">";
            forward.Size = new Size(xWidth, xWidth);
            forward.Location = new Point(xWidth, xWidth);
            forward.Click += (s, e) =>
            {
                TabChange("forward", currentTab.uriIndex + 1);
            };
            mf.Controls.Add(forward);

            mf.ShowDialog();
        }

        public bool FullCreateMenu()
        {
            // if menu already exists, move it
            if (mouseMenu.Controls.Count > 0)
            {
                mouseMenu.Location = mf.PointToClient(Control.MousePosition);
                return false;
            }
            else
            {
                // new menu
                CreateMouseMenu();
                mouseMenu.BringToFront();
                return true;
            }
        }

        public void CreateMenuButton(string text, Action<object, MouseEventArgs> myMethod)
        {
            // new menu
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(100, 25);
            btn.Dock = DockStyle.Top;

            btn.MouseClick += ((s, e) => {
                myMethod(s, e);
                mf.Controls.Remove(mouseMenu);
                mouseMenu.Dispose();
            });
            mouseMenu.Controls.Add(btn);
            mouseMenu.Size = new Size(100, 25 * mouseMenu.Controls.Count);
        }

        public void CreateMouseMenu()
        {
            mouseMenu = new Panel();
            /*mouseMenu.AutoSize = true;
            mouseMenu.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mouseMenu.MinimumSize = new Size(xWidth, xWidth);*/
            mf.Controls.Add(mouseMenu);
            mouseMenu.Location = mf.PointToClient(Control.MousePosition);
        }

        private void CreateShortcut(string path)
        {
            Button cut = new Button();
            string[] dirs = path.Split('\\');
            cut.Text = dirs.Last();
            cut.Size = new Size(cuts.Width, xWidth);
            cut.Dock = DockStyle.Top;
            cut.Click += (s, e) =>
            {
                if (currentTab.uris.Count >= 1 && new Uri(path) == currentTab.uris[currentTab.uriIndex])
                {
                    return;
                }
                wb.Navigate(path);
            };
            cuts.Controls.Add(cut);
        }

        private void CreateTabClass()
        {
            MyTab mt = new MyTab(this, mf, wb, tabWidth, xWidth);
            mt.CreateTab();
            tabs.Add(mt);
        }

        public void MoveTabs2()
        {
            tabStartX = xWidth;
            foreach (MyTab tab in tabs)
            {
                tab.p.Left = tabStartX;
                tabStartX += tabWidth + xWidth;
            }
        }

        public void CheckAdjacentTab()
        {
            int w = currentTab.p.Width;
            // if currentTab going left
            if (tabs.IndexOf(currentTab) + 1 < tabs.Count && currentTab.p.Location.X + w > tabs[tabs.IndexOf(currentTab) + 1].p.Location.X + (w / 2))
            {
                MyTab leftTab = tabs[tabs.IndexOf(currentTab) + 1];
                leftTab.p.Left -= w;
                tabs.Remove(currentTab);
                tabs.Insert(tabs.IndexOf(leftTab) + 1, currentTab);
            }
            // muuta + tabwidth
            else if (tabs.IndexOf(currentTab) >= 1 && currentTab.p.Location.X < tabs[tabs.IndexOf(currentTab) - 1].p.Location.X + (w / 2))
            {
                MyTab rightTab = tabs[tabs.IndexOf(currentTab) - 1];
                rightTab.p.Left += w;
                tabs.Remove(currentTab);
                tabs.Insert(tabs.IndexOf(rightTab), currentTab);
            }
        }

        public void TabChange(string act, int idx)
        {
            if ((act == "back" && currentTab.uriIndex <= 0) || (act == "forward" && currentTab.uriIndex >= currentTab.uris.Count - 1))
            {
                // couldnt go back or forward, dont do anything
                return;
            }
            action = act;
            wb.Navigate(currentTab.uris[idx]);
        }

        private void OnUrlChange()
        {
            string[] addrs = wb.Url.Segments;
            currentTab.tab.Text = addrs[addrs.Length - 1].Replace("%20", " ");

            // going to dir naturally
            if (action == "")
            {
                // going forward through different means
                if (currentTab.uriIndex + 2 <= currentTab.uris.Count)
                {
                    // should be different, but at the same level
                    currentTab.uris.RemoveRange(currentTab.uriIndex + 1, currentTab.uris.Count - currentTab.uriIndex - 1);
                    currentTab.uris.Add(wb.Url);
                }
                else if (currentTab.uris.Count == 0)
                {
                    currentTab.uris.Add(wb.Url);
                }
                // default
                else currentTab.uris.Add(wb.Url);
                currentTab.uriIndex++;
            }
            else if (action == "back")
            {
                currentTab.uriIndex--;
                
            }
            else if (action == "forward")
            {
                currentTab.uriIndex++;
            }
            else if (action == "changeTab")
            {
                // placeholder
                //Console.WriteLine("tab changed");
            }
            action = "";

            UpdateAddressBar();
        }

        private void ClearControls(Control c)
        {
            int i = 0;
            while (i < c.Controls.Count)
            {
                c.Controls[0].Dispose();
            }
        }

        public void UpdateAddressBar()
        {
            // destroy old bar
            ClearControls(addr);

            // fix spaces
            string[] addrs = wb.Url.Segments;
            for (int j = 0; j < addrs.Length; j++)
            {
                addrs[j] = addrs[j].Replace("%20", " ");
            }
            
            // add new addr
            int partStartX = 0;
            string uriNew = "";
            for (int j = 0; j < addrs.Length; j++)
            {
                if (addrs[j] != "/")
                {
                    Button part = new Button();
                    part.Text = addrs[j];
                    part.MinimumSize = new Size(xWidth, xWidth);
                    part.AutoSize = true;
                    part.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                    uriNew += addrs[j];
                    string uriu = uriNew;
                    part.Click += (s, e) =>
                    {
                        // currentTab.uris.Count >= 1 && 
                        // if clicking on current url (last button)
                        if (new Uri(uriu) == currentTab.uris[currentTab.uriIndex])
                        {
                            return;
                        }
                        wb.Navigate(uriu);
                    };
                    
                    addr.Controls.Add(part);
                    part.Left = partStartX;
                    partStartX += part.Width;
                }
            }
        }
    }
}
