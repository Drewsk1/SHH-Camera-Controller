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

    }
}
