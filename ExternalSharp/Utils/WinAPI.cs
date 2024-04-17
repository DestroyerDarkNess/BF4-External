using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSharp.Utils
{
    public class WinAPI
    {

        // The GetForegroundWindow function returns a handle to the foreground window
        // (the window  with which the user is currently working).
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        // The GetWindowThreadProcessId function retrieves the identifier of the thread
        // that created the specified window and, optionally, the identifier of the
        // process that created the window.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        public const uint MOUSEEVENTF_MOVE = 0x0001;

        public const int AimBoneHead = 104;
        public const int AimBoneChest = 6;

        public static uint ForegroundProcessID()
        {
            IntPtr hwnd = GetForegroundWindow();

            // The foreground window can be NULL in certain circumstances, 
            // such as when a window is losing activation.
            if (hwnd == null)
                return 0;

            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return pid;
        }

        public static void MoveMouse(int deltaX, int deltaY)
        {
            mouse_event(MOUSEEVENTF_MOVE, deltaX, deltaY, 0, 0);
        }
    }
}
