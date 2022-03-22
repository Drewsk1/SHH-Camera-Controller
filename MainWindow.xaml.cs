using Drew;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SH_Camera
{
    public partial class MainWindow : Window
    {

        #region fields

        private const double Radians2Degrees = 57.2957795130823f;

        private Thread? _guiThread;
        private Thread? _injectThread;

        private HotKey _cameraForward;
        private HotKey _cameraBackward;
        private HotKey _cameraLeft;
        private HotKey _cameraRight;    
        private HotKey _cameraUp;
        private HotKey _cameraDown;

        private float _cameraSpeed;
        private float _cameraX;
        private float _cameraY;
        private float _cameraZ;

        private float _deltaTime;

        private float _cameraYawSine;
        private float _cameraYawCosine;
        private float _cameraPitchSine;

        private short _cameraHorizontalInput;
        private short _cameraForwardInput;
        private short _cameraVerticalInput;

        private bool _isAttached;
        private bool _isInjected;

        private bool _running;

        private Process[] _process;
        private ProcessModule _procMod;

        private IntPtr _cameraCoordinateAddress;
        private IntPtr _cameraHorizontalRotationAddress;
        private IntPtr _cameraVerticalRotationAddress;
        private IntPtr _cameraXMethodAddress;
        private IntPtr _cameraYMethodAddress;

        private Stopwatch _stopWatch;
        private TimeSpan _stopWatchInterval;

        #endregion

        public MainWindow()
        {

            InitializeComponent();

            _cameraCoordinateAddress = (IntPtr)0x1158F1C0;
            _cameraHorizontalRotationAddress = (IntPtr)0x1158F190;
            _cameraVerticalRotationAddress = (IntPtr)0x1158F1A4;
            _cameraXMethodAddress = (IntPtr)0x1053CC8A; // 66 0F D6 42 30 //90 90 90 90 90
            _cameraYMethodAddress = (IntPtr)0x1053CC94; // 66 0F D6 42 38 //90 90 90 90 90

            _cameraSpeed = 5f;

            _cameraX = 0f;
            _cameraY = 0f;
            _cameraZ = 0f;

            _deltaTime = 0f;

            _cameraYawSine = 0f;
            _cameraYawCosine = 0f;
            _cameraPitchSine = 0f;

            _cameraHorizontalInput = 0;
            _cameraForwardInput = 0;
            _cameraVerticalInput = 0;

            _stopWatch = new Stopwatch();

            _isAttached = false;
            _isInjected = false;

            AttachButton.IsEnabled = true;
            InjectButton.IsEnabled = false;

            _running = true;

            _guiThread = new Thread(new ThreadStart(GUIThread));
            _guiThread.Start();
            while(!_guiThread.IsAlive) ;

            _injectThread = new Thread(new ThreadStart(InjectThread));
            _injectThread.Start();
            while(!_injectThread.IsAlive) ;

        }
        
        private void GUIThread()
        {

            while(_running)
            {
                if (_isAttached)
                {

                    CameraX_Label.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => CameraX_Label.Content = _cameraX.ToString()));
                    CameraY_Label.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => CameraY_Label.Content = _cameraY.ToString()));
                    CameraZ_Label.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => CameraZ_Label.Content = _cameraZ.ToString()));

                    CameraYawSine_Label.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => CameraYawSine_Label.Content = _cameraYawSine.ToString()));
                    CameraYawCosine_Label.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => CameraYawCosine_Label.Content = _cameraYawCosine.ToString()));

                    CameraPitchSine_Label.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => CameraPitchSine_Label.Content = _cameraPitchSine.ToString()));

                }

                Thread.Sleep(500);

            }

        }

        private void InjectThread()
        {

            int bytesRead;
            int bytesWritten;

            //double hAngleY;
            //double hAngleX;
            //double vAngle;

            while(_running)
            {

                if (_isInjected)
                {
                    
                    //_cameraX = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress, 4, out bytesRead));
                    //_cameraZ = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 4, 4, out bytesRead));
                    //_cameraY = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 8, 4, out bytesRead));

                    _cameraYawSine = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraHorizontalRotationAddress, 4, out bytesRead));
                    _cameraYawCosine = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraHorizontalRotationAddress + 32, 4, out bytesRead));

                    _cameraPitchSine = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraVerticalRotationAddress + 16, 4, out bytesRead));


                    if (_cameraHorizontalInput != 0 || _cameraForwardInput != 0 || _cameraVerticalInput != 0)
                    {

                        //hAngleY = hAngleX = Math.Atan2((double)_cameraHorizontalSine, (double)_cameraHorizontalCosine) * Radians2Degrees;
                        //vAngle = Math.Atan2((double)_cameraVerticalSine, (double)_cameraVerticalCosine) * Radians2Degrees;

                        if (_cameraForwardInput != 0)
                        {
                            

                            //Wrong but saving for later projects
                            //Debug.WriteLine("Forward");

                            //if(hAngleY > 90d)
                            //{
                            //    hAngleY = 180d - hAngleY;
                            //}
                            //else if(hAngleY < -90d)
                            //{
                            //    hAngleY = -180d - hAngleY;
                            //}

                            //ySpeed = (float)(hAngleY / 90d);


                            //if(hAngleX >= 0d)
                            //{
                            //    hAngleX = 90d - hAngleX;
                            //}
                            //else if(hAngleX < 0d)
                            //{
                            //    hAngleX = 90d - (-hAngleX);
                            //}

                            //xSpeed = (float)(hAngleX / 90d);


                            //if(vAngle > 90d)
                            //{
                            //    vAngle = 180d - vAngle;
                            //}
                            //else if(vAngle < -90d)
                            //{
                            //    vAngle = -180d - vAngle;
                            //}

                            //zSpeed = (float)(vAngle / 90d);
                            

                            _cameraY += ((_cameraYawSine * _cameraSpeed) * _deltaTime) * (float)_cameraForwardInput;
                            _cameraX += ((_cameraYawCosine * _cameraSpeed) * _deltaTime) * (float)_cameraForwardInput;
                            _cameraZ += ((_cameraPitchSine * _cameraSpeed) * _deltaTime) * (float)_cameraForwardInput;

                        }
                        else if (_cameraHorizontalInput != 0)
                        {

                            _cameraX += ((_cameraYawSine * _cameraSpeed) * _deltaTime) * (float)_cameraHorizontalInput;
                            _cameraY += ((-_cameraYawCosine * _cameraSpeed) * _deltaTime) * (float)_cameraHorizontalInput;

                        }

                        if (_cameraVerticalInput != 0)
                        {

                            _cameraZ += ((float)_cameraVerticalInput * (_cameraSpeed/2)) * _deltaTime;

                        }

                    }

                    //Debug.WriteLine(_cameraX.ToString() + " , " + _cameraY.ToString() + " , " + _cameraZ.ToString());

                    MemoryUtility.WriteMemory(_process[0], _cameraCoordinateAddress, _cameraX, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraCoordinateAddress + 4, _cameraZ, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraCoordinateAddress + 8, _cameraY, out bytesWritten);

                    Thread.Sleep(16);

                    _stopWatch.Stop();

                    _stopWatchInterval = _stopWatch.Elapsed;

                    _deltaTime = (float)_stopWatchInterval.Milliseconds / 100f;

                    //Debug.WriteLine(_stopWatchInterval.Milliseconds.ToString());

                    _stopWatch.Restart();
                    
                }
                else
                {
                    Thread.Sleep(500);
                }   

            }

        }

        private void OnHotKeyHandler(HotKey hotKey)
        {

            //toggle keys

            if (hotKey.Key == Key.W)
            {
                if (_cameraForwardInput != 1)
                    _cameraForwardInput = 1;
                else
                    _cameraForwardInput = 0;
            }
            else if (hotKey.Key == Key.S)
            {
                if (_cameraForwardInput != -1)
                    _cameraForwardInput = -1;
                else
                    _cameraForwardInput = 0;
            }
            else if (hotKey.Key == Key.D)
            {
                if (_cameraHorizontalInput != 1)
                    _cameraHorizontalInput = 1;
                else
                    _cameraHorizontalInput = 0;
            }
            else if (hotKey.Key == Key.A)
            {
                if (_cameraHorizontalInput != -1)
                    _cameraHorizontalInput = -1;
                else
                    _cameraHorizontalInput = 0;
            }
            else if (hotKey.Key == Key.F)
            {
                if (_cameraVerticalInput != 1)
                    _cameraVerticalInput = 1;
                else
                    _cameraVerticalInput = 0;
            }
            else if (hotKey.Key == Key.Space)
            {
                if (_cameraVerticalInput != -1)
                    _cameraVerticalInput = -1;
                else
                    _cameraVerticalInput = 0;
            }

        }

        #region WPF Events

        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {

            int bytesRead;

            if (!_isAttached)
            {

                try
                {

                    _process = Process.GetProcessesByName("SilentHill");

                    foreach(ProcessModule processMod in _process[0].Modules)
                    {
                        Debug.WriteLine(processMod.ModuleName);

                        if(processMod.ModuleName == "SilentHill.exe")
                        {

                            _cameraX = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress, 4, out bytesRead));
                            _cameraZ = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 4, 4, out bytesRead));
                            _cameraY = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 8, 4, out bytesRead));

                            _procMod = processMod;

                            _isAttached = true;

                            InjectButton.IsEnabled = true;
                            AttachButton.IsEnabled = false;

                        }

                    }

                }
                catch (IndexOutOfRangeException ior)
                {

                    Debug.WriteLine("Process Not Found");

                }

            }

        }

        private void InjectButton_Click(object sender, RoutedEventArgs e)
        {

            int bytesRead;
            int bytesWritten;

            if (_isAttached)
            {
                if (!_isInjected)
                {

                    _cameraForward = new HotKey(Key.W, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
                    _cameraBackward = new HotKey(Key.S, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
                    _cameraLeft = new HotKey(Key.A, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
                    _cameraRight = new HotKey(Key.D, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
                    _cameraUp = new HotKey(Key.F, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
                    _cameraDown = new HotKey(Key.Space, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);

                    _cameraX = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress, 4, out bytesRead));
                    _cameraZ = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 4, 4, out bytesRead));
                    _cameraY = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 8, 4, out bytesRead));

                    MemoryUtility.WriteMemory(_process[0], _cameraXMethodAddress, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraYMethodAddress, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }, out bytesWritten);

                    _stopWatch.Reset();
                    _stopWatch.Start();

                    _isInjected = true;

                    InjectButton.Content = "UNINJECT";

                }
                else if (_isInjected)
                {

                    _cameraForward.Dispose();
                    _cameraBackward.Dispose();
                    _cameraLeft.Dispose();
                    _cameraRight.Dispose(); 
                    _cameraUp.Dispose();
                    _cameraDown.Dispose();

                    _cameraForward.ClearHotkeys();
                    _cameraBackward.ClearHotkeys();
                    _cameraLeft.ClearHotkeys();
                    _cameraRight.ClearHotkeys();
                    _cameraUp.ClearHotkeys();
                    _cameraDown.ClearHotkeys();

                    MemoryUtility.WriteMemory(_process[0], _cameraXMethodAddress, new byte[] { 0x66, 0x0F, 0xD6, 0x42, 0x30 }, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraYMethodAddress, new byte[] { 0x66, 0x0F, 0xD6, 0x42, 0x38 }, out bytesWritten);

                    _stopWatch.Reset();

                    _isInjected = false;

                    InjectButton.Content = "INJECT";

                }

            }

        }

        private void SHCWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (_guiThread.IsAlive)
            {
                _running = false;
                _guiThread.Join();
            }

            if (_injectThread.IsAlive)
            {
                _running = false;
                _guiThread.Join();
            }

        }

        #endregion

    }

}
