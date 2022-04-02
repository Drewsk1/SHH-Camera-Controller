using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using SharpAvi; //SharpAvi NuGet package
using SharpAvi.Codecs; 
using SharpAvi.Output;
using System.Threading;
using System.Threading.Tasks;

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

    public class ScreenRecorder : IDisposable
    {

        //Set to only output Motion Jpeg

        #region Fields

        private Thread _writeThread;

        private AviWriter _writer;
        private IAviVideoStream _stream;

        private int _index;
        private int _writeIndex;
        private int _maxElements;
        private List<byte[]> _bufferList;

        private byte[] _buffer;

        private bool _isRecording;

        public int Height { get; private set; }
        public int Width { get; private set; }
        public string FileName { get; private set; }
        public int FrameRate { get; private set; }
        public int Quality { get; private set; }

        #endregion

        public ScreenRecorder(string file_name, int frame_rate, int quality, int max_elements)
        {

            FileName = file_name;
            FrameRate = frame_rate;
            Quality = quality;

            _maxElements = max_elements;

            Height = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
            Width = (int)System.Windows.SystemParameters.PrimaryScreenWidth;

            _index = _writeIndex = 0;
            _bufferList = new List<byte[]>();

            _buffer = new byte[Width * Height * 4];

            _isRecording = true;

            _writer = new AviWriter(FileName)
            {
                FramesPerSecond = FrameRate,
                EmitIndex1 = true,
            };

            _stream = _writer.AddMJpegWpfVideoStream(Width, Height, Quality);

            _stream.Name = "Drewcorder";

            _writeThread = new Thread(WriteThread)
            {
                Name = typeof(ScreenRecorder).Name,
                IsBackground = true,
            };

            _writeThread.Start();

        }

        public void Dispose()
        {

            _isRecording = false;

            _writeThread.Join();

            _writer.Close();

        }

        private void WriteThread()
        {

            Task videoWriteTask = null;

            while(_isRecording)
            {

                videoWriteTask?.Wait();

                if(_writeIndex != _index)
                {

                    videoWriteTask = _stream.WriteFrameAsync(true, _bufferList[_writeIndex], 0, _bufferList[_writeIndex].Length);

                    if(++_writeIndex > _maxElements)
                        _writeIndex = 0;

                }

                Thread.Sleep(10);

            }

            videoWriteTask?.Wait();

        }

        public void CaptureFrame()
        {

            using (Bitmap bmp = new Bitmap(Width, Height))
            {

                using (Graphics g = Graphics.FromImage(bmp))
                {

                    g.CopyFromScreen(Point.Empty, Point.Empty, new Size(Width, Height), CopyPixelOperation.SourceCopy);

                    g.Flush();

                    BitmapData bits = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

                    Marshal.Copy(bits.Scan0, _buffer, 0, _buffer.Length);

                    bmp.UnlockBits(bits);

                    _bufferList.Insert(_index, _buffer);

                }

            }

            if (++_index > _maxElements)
                _index = 0;

        }

    }

}
