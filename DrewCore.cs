using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Drew
{
    public class MemoryUtility
    {

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            Terminate = 1,
            CreateThread = 2,
            VMOperation = 8,
            VMRead = 16,
            VMWrite = 32,
            DupHandle = 64,
            SetInformation = 512,
            QueryInformation = 1024,
            Synchronize = 1048576,
            All = 2035711
        }

        #region DLLImports

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern int CloseHandle(IntPtr hProcess);

        #endregion

        #region Wrapper

        public static byte[] ReadMemory(Process process, uint address, int numOfBytes, out int bytesRead)
        {
            IntPtr intPtr = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            byte[] buffer = new byte[numOfBytes];
            ReadProcessMemory(intPtr, new IntPtr((long)address), buffer, numOfBytes, out bytesRead);
            return buffer;
        }

        public static byte[] ReadMemory(Process process, IntPtr address, int numOfBytes, out int bytesRead)
        {
            IntPtr intPtr = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            byte[] buffer = new byte[numOfBytes];
            ReadProcessMemory(intPtr, address, buffer, numOfBytes, out bytesRead);
            return buffer;
        }

        public static bool WriteMemory(Process process, uint address, byte[] buffer, out int bytesWritten)
        {
            IntPtr hProc = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            bool flag = WriteProcessMemory(hProc, new IntPtr((long)address), buffer, (uint)buffer.Length, out bytesWritten);
            CloseHandle(hProc);
            return flag;
        }

        public static bool WriteMemory(Process process, IntPtr address, byte[] buffer, out int bytesWritten)
        {
            IntPtr hProc = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            bool flag = WriteProcessMemory(hProc, address, buffer, (uint)buffer.Length, out bytesWritten);
            CloseHandle(hProc);
            return flag;
        }

        public static bool WriteMemory(Process process, uint address, float value, out int bytesWritten)
        {
            IntPtr hProc = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            byte[] val = BitConverter.GetBytes(value);
            bool flag = WriteProcessMemory(hProc, new IntPtr((long)address), val, (uint)((long)val.Length), out bytesWritten);
            CloseHandle(hProc);
            return flag;
        }

        public static bool WriteMemory(Process process, IntPtr address, short value, out int bytesWritten)
        {
            IntPtr hProc = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            byte[] val = BitConverter.GetBytes(value);
            bool flag = WriteProcessMemory(hProc, address, val, (uint)((long)val.Length), out bytesWritten);
            CloseHandle(hProc);
            return flag;
        }

        public static bool WriteMemory(Process process, IntPtr address, float value, out int bytesWritten)
        {
            IntPtr hProc = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            byte[] val = BitConverter.GetBytes(value);
            bool flag = WriteProcessMemory(hProc, address, val, (uint)((long)val.Length), out bytesWritten);
            CloseHandle(hProc);
            return flag;
        }

        #endregion

    }

    public class MouseUtility
    {
        public enum MouseEvent : int
        {

            MOUSEEVENTF_ABSOLUTE = 0x8000,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_HWHEEL = 0x01000,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,

        }

        #region DLLImports

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);

        #endregion

        #region Wrapper

        public static void MoveMouseRelative(int relX, int relY)
        {
            mouse_event((int)MouseEvent.MOUSEEVENTF_MOVE, relX, relY, 0, 0);
        }

        #endregion

    }

}
