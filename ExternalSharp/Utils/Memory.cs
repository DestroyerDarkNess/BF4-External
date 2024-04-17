using EasyImGui.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalSharp.Utils
{
    public class Memory
    {
        public delegate void OnMemoryReady(object sender, bool Status);

        public event OnMemoryReady OnMemReady = null;

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const int MAX_PATH = 260;

        public string targetWindowName = "Battlefield 4";
        public int PID;
        public IntPtr pHandle;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, int uType);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            var windowTitle = new StringBuilder(256);
            GetWindowText(hWnd, windowTitle, windowTitle.Capacity);
        
            if (windowTitle.ToString() == targetWindowName)
            {
                int processId = 0;
                GetWindowThreadProcessId(hWnd, out processId);
               
                if (processId == 0) {
                    OnMemReady?.Invoke(this, false);
                } else {
                    PID = processId;
                    pHandle = OpenProcess(PROCESS_ALL_ACCESS, false, PID);
                    OnMemReady?.Invoke(this, true);
                }

                return false;
            }

            return true;
        }

       public int OverlayPID = 0;

        public void Init()
        {
            OverlayPID = Process.GetCurrentProcess().Id;
            EnumWindows(EnumWindowsCallback, IntPtr.Zero);
        }

       

        public T Read<T>(IntPtr address) where T : struct
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            ReadProcessMemory(pHandle, address, buffer, buffer.Length, out _);
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T value = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return value;
        }

        public void Write<T>(IntPtr address, T value) where T : struct
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), true);
            handle.Free();
            WriteProcessMemory(pHandle, address, buffer, buffer.Length, out _);
        }

        public bool ReadString(IntPtr address, out string value, int size)
        {
            byte[] buffer = new byte[size];
            ReadProcessMemory(pHandle, address, buffer, buffer.Length, out _);
            value = System.Text.Encoding.Default.GetString(buffer);
            return !string.IsNullOrEmpty(value);
        }
    }

}
