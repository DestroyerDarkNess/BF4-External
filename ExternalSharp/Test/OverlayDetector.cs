using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSharp.Test
{
    public class OverlayDetector
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);

        const int GWL_STYLE = -16;
        const int GWL_EXSTYLE = -20;
        const uint GA_PARENT = 1;
        const uint GW_HWNDNEXT = 2;

        static StringBuilder GetProcessImageFileName(int processId)
        {
            const int MAX_PATH = 260;
            StringBuilder fileName = new StringBuilder(MAX_PATH);
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);

            if (hProcess != IntPtr.Zero)
            {
                if (GetModuleFileNameEx(hProcess, IntPtr.Zero, fileName, (uint)MAX_PATH) == 0)
                {
                    fileName = null;
                }

                CloseHandle(hProcess);
            }

            return fileName;
        }

        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);

        public static void Scan()
        {
            IntPtr desktop = GetDesktopWindow();
            IntPtr hParent;

            // Find the top-most window
            StringBuilder title = new StringBuilder(64);
            int dwProcId = 0;

            IntPtr window = GetTopWindow(desktop);

            do
            {
                GetWindowThreadProcessId(window, out dwProcId);

                int dwStyle = GetWindowLong(window, GWL_STYLE);
                int dwExStyle = GetWindowLong(window, GWL_EXSTYLE);

                hParent = GetAncestor(window, GA_PARENT);

                title.Clear();
                StringBuilder imgFileName = GetProcessImageFileName(dwProcId);

                if (imgFileName != null)
                {
                    int ret = GetWindowText(window, title, 64);

                    if (ret != 0 || dwExStyle == 0x22080828)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{imgFileName} Title: {title} hWnd: {window} pId: {dwProcId} exStyle: 0x{dwExStyle:X} style: 0x{dwStyle:X}");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{imgFileName} Title: NULL hWnd: {window} pId: {dwProcId} exStyle: 0x{dwExStyle:X} style: 0x{dwStyle:X}");
                    }
                }

            } while ((window = GetWindow(window, GW_HWNDNEXT)) != IntPtr.Zero);
        }

    }
}

