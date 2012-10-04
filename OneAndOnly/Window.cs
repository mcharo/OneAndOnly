using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace OneAndOnly
{
    class Window
    {
        private IntPtr _handle;
        private string _screenname;
        private Process _proc;
        private int _showcmd;
        private int _zorder;

        public int Zorder
        {
            get { return _zorder; }
            set { _zorder = value; }
        }

        public Window(IntPtr ptr, string screen)
        {
            _handle = ptr;
            _screenname = screen;
            Native.WINDOWPLACEMENT winPlacement = new Native.WINDOWPLACEMENT();
            Native.GetWindowPlacement(_handle, ref winPlacement);
            _showcmd = winPlacement.showCmd;
        }

        public Process Proc
        {
            get { return _proc; }
            set { _proc = value; }
        }

        public string Screenname
        {
            get { return _screenname; }
            set { _screenname = value; }
        }

        public IntPtr Handle
        {
            get { return _handle; }
            set { _handle = value; }
        }

        public void minimizeWindow()
        {
            Native.WINDOWPLACEMENT winPlacement = new Native.WINDOWPLACEMENT();
            Native.GetWindowPlacement(_handle, ref winPlacement);
            winPlacement.showCmd = Native.SW_MINIMIZE;
            Native.SetWindowPlacement(_handle, ref winPlacement);
        }

        public bool IsWindowVisible()
        {
            return (Native.GetWindowLong(_handle, (int)Native.WindowLongFlags.GWL_STYLE) & Native.WS_VISIBLE) != 0;
        }

        public void restoreWindow()
        {
            Native.WINDOWPLACEMENT winPlacement = new Native.WINDOWPLACEMENT();
            Native.GetWindowPlacement(_handle, ref winPlacement);
            switch (_showcmd)
            {
                case Native.SW_MAXIMIZE:
                    winPlacement.showCmd = Native.SW_SHOWMAXIMIZED;
                    break;
                case Native.SW_SHOWNA:
                    winPlacement.showCmd = Native.SW_SHOWNOACTIVATE;
                    break;
                default:
                    winPlacement.showCmd = Native.SW_RESTORE;
                    break;
            }
            
            Native.SetWindowPlacement(_handle, ref winPlacement);
        }

        public bool isMinimized()
        {
            Native.WINDOWPLACEMENT winPlacement = new Native.WINDOWPLACEMENT();
            Native.GetWindowPlacement(_handle, ref winPlacement);
            switch (winPlacement.showCmd)
            {
                case Native.SW_HIDE:
                case Native.SW_SHOWMINIMIZED:
                case Native.SW_SHOWMINNOACTIVE:
                    return true;
                default:
                    Debug.WriteLine("proc: " + Proc.MainWindowTitle + ", showcmd: " + winPlacement.showCmd);
                    return false;
            }
        }

        public void moveWindow()
        {
            // move window to another screen in the same x,y pos
            // if there's only one screen, minimize

            Native.WINDOWPLACEMENT winPlacement = new Native.WINDOWPLACEMENT();
            Native.GetWindowPlacement(_handle, ref winPlacement);
            int winShow = winPlacement.showCmd;
            Native.RECT myRect;
            Native.GetWindowRect(_handle, out myRect);

            IntPtr ptrAfter = (IntPtr)0;
            Screen CurrentScreen = Screen.FromPoint(new Point(myRect.Left, myRect.Top));
            Console.WriteLine(CurrentScreen.DeviceName + ": " + myRect.Left + ", " + myRect.Top);
            Screen targetScreen = CurrentScreen;
            foreach (Screen screen in Screen.AllScreens)
            {
                targetScreen = CurrentScreen;
                if (screen.DeviceName != CurrentScreen.DeviceName)
                {
                    targetScreen = screen;
                    break;
                }
            }
            if (targetScreen.DeviceName == CurrentScreen.DeviceName)
            {
                minimizeWindow();
            }
            else
            {

                int xpos = targetScreen.Bounds.Left + myRect.Left;
                int ypos = targetScreen.Bounds.Top + myRect.Top;
                if (xpos == myRect.Left) { xpos += CurrentScreen.Bounds.Width; }

                Console.WriteLine(targetScreen.DeviceName + ": " + xpos + ", " + ypos);
                Native.SetWindowPos(_handle, ptrAfter, xpos, ypos, myRect.Right - myRect.Left, myRect.Bottom - myRect.Top, Native.SWP_NOZORDER | Native.SWP_NOSIZE | Native.SWP_SHOWWINDOW);
                if (winShow == 3)
                {
                    winPlacement.showCmd = Native.SW_MAXIMIZE;
                    Native.SetWindowPlacement(_handle, ref winPlacement);
                }
            }
        }
    }
}
