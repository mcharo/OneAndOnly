using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OneAndOnly
{
    class Native
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        public static int ZOrder(IntPtr windowHandle)
        {
            int z = 0;
            while ((windowHandle = GetWindow(windowHandle, 3)) != IntPtr.Zero)
            {
                z++;
            }
            return z;
        }

        public enum WindowLongFlags : int
        {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }

        public const int SWP_ASYNCWINDOWPOS = 0x4000;
        public const int SWP_DEFERERASE = 0x2000;
        public const int SWP_DRAWFRAME = 0x0020;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const int SWP_HIDEWINDOW = 0x0080;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_NOCOPYBITS = 0x0100;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOOWNERZORDER = 0x0200;
        public const int SWP_NOREDRAW = 0x0008;
        public const int SWP_NOREPOSITION = 0x0200;
        public const int SWP_NOSENDCHANGING = 0x0400;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_SHOWWINDOW = 0x0040;

        public const int HWND_TOP = 0;
        public const int HWND_BOTTOM = 1;
        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        public static string GetDaClassName(IntPtr hWnd)
        {
            int nRet;
            StringBuilder ClassName = new StringBuilder(100);
            //Get the window class name
            nRet = GetClassName(hWnd, ClassName, ClassName.Capacity);
            if (nRet != 0)
            {
                return ClassName.ToString();
            }
            else
            {
                return null;
            }
        }

        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processID)
        {
            List<IntPtr> handles = new List<IntPtr>();

            EnumThreadDelegate addWindowHandle = delegate(IntPtr hWnd, IntPtr param)
            {
                string className = GetDaClassName(hWnd);

                switch (className)
                {
                    case null:
                        break;
                    case "ExploreWClass":
                        handles.Add(hWnd);
                        break;
                    case "CabinetWClass":
                        handles.Add(hWnd);
                        break;
                    case "IEFrame":
                        handles.Add(hWnd);
                        break;
                    case "HelpAlert_MainWindow":
                        handles.Add(hWnd);
                        break;
                    case "WindowsForms10.Window.8.app.0.201d787":
                        handles.Add(hWnd);
                        break;
                    case "WindowsForms10.Window.8.app.0.31e27ec":
                        handles.Add(hWnd);
                        break;
                    default:
                        //Debug.WriteLine(className.ToString());
                        break;
                }

                return true;
            };

            foreach (ProcessThread thread in Process.GetProcessById(processID).Threads)
                EnumThreadWindows(thread.Id, addWindowHandle, IntPtr.Zero);

            return handles;
        }

        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;

        public const UInt32 WS_OVERLAPPED = 0;
        public const UInt32 WS_POPUP = 0x80000000;
        public const UInt32 WS_CHILD = 0x40000000;
        public const UInt32 WS_MINIMIZE = 0x20000000;
        public const UInt32 WS_VISIBLE = 0x10000000;
        public const UInt32 WS_DISABLED = 0x8000000;
        public const UInt32 WS_CLIPSIBLINGS = 0x4000000;
        public const UInt32 WS_CLIPCHILDREN = 0x2000000;
        public const UInt32 WS_MAXIMIZE = 0x1000000;
        public const UInt32 WS_CAPTION = 0xC00000;      // WS_BORDER or WS_DLGFRAME  
        public const UInt32 WS_BORDER = 0x800000;
        public const UInt32 WS_DLGFRAME = 0x400000;
        public const UInt32 WS_VSCROLL = 0x200000;
        public const UInt32 WS_HSCROLL = 0x100000;
        public const UInt32 WS_SYSMENU = 0x80000;
        public const UInt32 WS_THICKFRAME = 0x40000;
        public const UInt32 WS_GROUP = 0x20000;
        public const UInt32 WS_TABSTOP = 0x10000;
        public const UInt32 WS_MINIMIZEBOX = 0x20000;
        public const UInt32 WS_MAXIMIZEBOX = 0x10000;
        public const UInt32 WS_TILED = WS_OVERLAPPED;
        public const UInt32 WS_ICONIC = WS_MINIMIZE;
        public const UInt32 WS_SIZEBOX = WS_THICKFRAME;
    }
}
