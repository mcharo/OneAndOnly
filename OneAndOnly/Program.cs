using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ManagedWinapi;
using System.Collections;

namespace OneAndOnly
{
    class Program
    {

        private static string[] special = { "explorer", "iexplore", "g2alauncherexpert", "g2acomm", "GotoAssist", "VpxClient", "TechnicianClient" };
        private static List<Window> windows = new List<Window>();

        static void Main(string[] args)
        {

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Exit", new System.EventHandler(exit_click));
            if (args.Length == 0 || args[0] != "runas")
            {
                menu.MenuItems.Add("Run as Administrator", new System.EventHandler(uac_click));
            }

            Hotkey minimizeHotkey = new Hotkey();
            minimizeHotkey.WindowsKey = true;
            minimizeHotkey.KeyCode = Keys.Oemtilde;
            minimizeHotkey.Enabled = true;
            minimizeHotkey.HotkeyPressed += new EventHandler(minimizeHotkey_HotkeyPressed);

            Hotkey restoreHotkey = new Hotkey();
            restoreHotkey.WindowsKey = true;
            restoreHotkey.KeyCode = Keys.A;
            restoreHotkey.Enabled = true;
            restoreHotkey.HotkeyPressed += new EventHandler(restoreHotkey_HotkeyPressed);

            NotifyIcon icon = new NotifyIcon();
            icon.Icon = new Icon(Properties.Resources.downarrow, new Size(32,32));
            icon.Text = "Win + ~ to minimize, Win + A to restore";
            icon.ContextMenu = menu;
            icon.DoubleClick += new EventHandler(icon_doubleclick);
            icon.Visible = true;

            Application.Run();

            icon.Visible = false;
        }

        static void exit_click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        static void uac_click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = Application.ExecutablePath;
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.Arguments = "runas";
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                p.StartInfo.Verb = "runas";
            }
            p.Start();
            Application.Exit();
        }

        static void minimizeHotkey_HotkeyPressed(object sender, EventArgs e)
        {
            minimizeWindowsOnActiveScreen();
        }

        static void restoreHotkey_HotkeyPressed(object sender, EventArgs e)
        {
            restoreWindowsOnActiveScreen();
        }

        private static void restoreWindowsOnActiveScreen()
        {
            
            var screenWindows =
                from w in windows
                orderby w.Zorder descending
                where w.Screenname == Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y)).DeviceName
                select w;
            foreach (Window w in screenWindows)
            {
                w.restoreWindow();
            }
            windows.RemoveAll(w => w.Screenname == Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y)).DeviceName);
        }

        private static void minimizeWindowsOnActiveScreen()
        {
            Screen CurrentScreen = Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y));

            foreach (Process proc in Process.GetProcesses())
            {
                // if special case, iterate through window handles
                // looking for specific classes (detail in Native.cs
                if (special.Contains(proc.ProcessName))
                {
                    foreach (IntPtr ptr in Native.EnumerateProcessWindowHandles(proc.Id))
                    {
                        addToList(CurrentScreen, proc, ptr);
                    }
                }
                else
                {
                    IntPtr ptr = proc.MainWindowHandle;
                    addToList(CurrentScreen, proc, ptr);
                }

            }
            foreach (Window w in windows)
            {
                if (!w.isMinimized())
                {
                    w.minimizeWindow();
                }
            }
        }

        private static void addToList(Screen CurrentScreen, Process proc, IntPtr ptr)
        {
            if (Screen.FromHandle(ptr).DeviceName == CurrentScreen.DeviceName)
            {
                Window win = new Window(ptr, CurrentScreen.DeviceName);
                win.Proc = proc;
                win.Zorder = Native.ZOrder(ptr);
                if (!win.isMinimized())
                {
                    windows.RemoveAll(w => w.Handle == win.Handle);
                    windows.Add(win);
                }
            }
        }

        static void icon_doubleclick(object sender, EventArgs e)
        {
            //minimizeWindowsOnActiveScreen();
            string output = "";
            foreach (Window w in windows)
            {
                output += w.Proc.MainWindowTitle + ", zorder: " + w.Zorder + "\n";
            }
            MessageBox.Show(output);
        }


    }
}

