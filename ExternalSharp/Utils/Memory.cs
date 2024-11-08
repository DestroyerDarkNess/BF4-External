using EasyImGui.Core;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        public string targetWindowName = "Battlefield 4";
        public int PID;
        public ProcessHacker.Native.Objects.ProcessHandle  pHandle;

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
                    pHandle = new ProcessHacker.Native.Objects.ProcessHandle(processId, ProcessHacker.Native.Security.ProcessAccess.VmRead | ProcessHacker.Native.Security.ProcessAccess.VmWrite);
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

        public void Init(int TargetID)
        {
            PID = TargetID;
            pHandle = new ProcessHacker.Native.Objects.ProcessHandle(TargetID, ProcessHacker.Native.Security.ProcessAccess.VmRead | ProcessHacker.Native.Security.ProcessAccess.VmWrite);
            OnMemReady?.Invoke(this, true);
        }

        public T Read<T>(IntPtr address) where T : struct
        {
            try {
                byte[] buffer = pHandle.ReadMemory(address, Marshal.SizeOf<T>());
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                T value = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();
                return value;
            } catch { return default; }
        }

        public int Write<T>(IntPtr address, T value) where T : struct
        {
            try
            {
            byte[] buffer =  new byte[Marshal.SizeOf<T>()];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), true);
            handle.Free();
            return pHandle.WriteMemory(address, buffer);
            }
            catch { return default; }
        }


        public bool ReadString(IntPtr address, out string value, int size)
        {
            try {
                byte[] buffer = pHandle.ReadMemory(address, size);
                value = System.Text.Encoding.Default.GetString(buffer);
                return !string.IsNullOrEmpty(value);
            } catch { value = string.Empty; return false; }
           
        }
    }

}
