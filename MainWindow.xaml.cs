using Drew;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SHH_Camera_Controller
{
    public partial class MainWindow : Window
    {

        #region fields

        private const float Radians2Degrees = 57.29578f;

        private Thread? _guiThread;
        private Thread? _injectThread;

        private HotKey _cameraForward;
        private HotKey _cameraBackward;
        private HotKey _cameraLeft;
        private HotKey _cameraRight;
        private HotKey _cameraUp;
        private HotKey _cameraDown;
        private HotKey _mouseLeft;
        private HotKey _mouseRight;
        private HotKey _mouseUp;
        private HotKey _mouseDown;

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

        private short _mouseVerticalInput;
        private short _mouseHorizontalInput;

        private int _mouseSensitivity;

        private bool _mouseEnabled;

        private bool _isAttached;
        private bool _isInjected;

        private bool _running;

        private bool _error;

        private double _errorOpacity;

        private Process[] _process;
        private ProcessModule _procMod;

        private IntPtr _cameraCoordinateAddress;
        private IntPtr _cameraYawAddress;
        private IntPtr _cameraPitchAddress;
        private IntPtr _cameraXAssemblyAddress;
        private IntPtr _cameraYAssemblyAddress;

        private Stopwatch _stopWatch;
        private TimeSpan _stopWatchInterval;

        #endregion

        public MainWindow()
        {

            InitializeComponent();

            _cameraCoordinateAddress = (IntPtr)0x1158F1C0;
            _cameraYawAddress = (IntPtr)0x1158F190;
            _cameraPitchAddress = (IntPtr)0x1158F1A4;
            _cameraXAssemblyAddress = (IntPtr)0x1053CC8A; // 66 0F D6 42 30 //90 90 90 90 90
            _cameraYAssemblyAddress = (IntPtr)0x1053CC94; // 66 0F D6 42 38 //90 90 90 90 90

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

            _mouseHorizontalInput = 0;
            _mouseVerticalInput = 0;

            _mouseSensitivity = 10;

            _mouseEnabled = false;

            _stopWatch = new Stopwatch();

            _isAttached = false;
            _isInjected = false;

            _error = false;

            _errorOpacity = 0d;

            AttachButton.IsEnabled = true;
            InjectButton.IsEnabled = false;

            _running = true;

            _guiThread = new Thread(new ThreadStart(GUIThread));
            _guiThread.Start();
            while (!_guiThread.IsAlive) ;

            _injectThread = new Thread(new ThreadStart(InjectThread));
            _injectThread.Start();
            while (!_injectThread.IsAlive) ;

        }

        private void GUIThread()
        {

            while (_running)
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

                if (_error)
                {
                    if (_errorOpacity > 0d)
                    {

                        _errorOpacity -= 0.1d;

                        ErrorMessage_Label.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => ErrorMessage_Label.Opacity = _errorOpacity));

                    }
                    else
                    {
                        _error = false;
                    }
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

            while (_running)
            {

                if (_isInjected)
                {

                    //_cameraX = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress, 4, out bytesRead));
                    //_cameraZ = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 4, 4, out bytesRead));
                    //_cameraY = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 8, 4, out bytesRead));

                    _cameraYawSine = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraYawAddress, 4, out bytesRead));
                    _cameraYawCosine = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraYawAddress + 32, 4, out bytesRead));

                    _cameraPitchSine = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraPitchAddress + 16, 4, out bytesRead));


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

                            _cameraZ += ((float)_cameraVerticalInput * (_cameraSpeed / 2)) * _deltaTime;

                        }

                    }

                    if (_mouseEnabled)
                    {

                        if (_mouseHorizontalInput != 0)
                            MouseUtility.MoveMouseRelative(_mouseHorizontalInput * _mouseSensitivity, 0);

                        if (_mouseVerticalInput != 0)
                            MouseUtility.MoveMouseRelative(0, _mouseVerticalInput * _mouseSensitivity);

                    }

                    //Debug.WriteLine(_cameraX.ToString() + " , " + _cameraY.ToString() + " , " + _cameraZ.ToString());

                    MemoryUtility.WriteMemory(_process[0], _cameraCoordinateAddress, _cameraX, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraCoordinateAddress + 4, _cameraZ, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraCoordinateAddress + 8, _cameraY, out bytesWritten);

                    Thread.Sleep(3);

                    _stopWatch.Stop();

                    _stopWatchInterval = _stopWatch.Elapsed;

                    _deltaTime = (float)_stopWatchInterval.Milliseconds / 100f;

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

            switch (hotKey.Key)
            {

                case Key.W:

                    if (_cameraForwardInput != 1)
                        _cameraForwardInput = 1;
                    else
                        _cameraForwardInput = 0;

                    break;

                case Key.S:

                    if (_cameraForwardInput != -1)
                        _cameraForwardInput = -1;
                    else
                        _cameraForwardInput = 0;

                    break;

                case Key.A:

                    if (_cameraHorizontalInput != -1)
                        _cameraHorizontalInput = -1;
                    else
                        _cameraHorizontalInput = 0;

                    break;

                case Key.D:

                    if (_cameraHorizontalInput != 1)
                        _cameraHorizontalInput = 1;
                    else
                        _cameraHorizontalInput = 0;

                    break;

                case Key.F:

                    if (_cameraVerticalInput != 1)
                        _cameraVerticalInput = 1;
                    else
                        _cameraVerticalInput = 0;

                    break;

                case Key.Space:

                    if (_cameraVerticalInput != -1)
                        _cameraVerticalInput = -1;
                    else
                        _cameraVerticalInput = 0;

                    break;
                case Key.Up:

                    if(_mouseVerticalInput != -1)
                        _mouseVerticalInput = -1;
                    else
                        _mouseVerticalInput= 0;

                    break;
                case Key.Down:

                    if (_mouseVerticalInput != 1)
                        _mouseVerticalInput = 1;
                    else
                        _mouseVerticalInput = 0;

                    break;
                case Key.Left:

                    if (_mouseHorizontalInput != -1)
                        _mouseHorizontalInput = -1;
                    else
                        _mouseHorizontalInput = 0;

                    break;
                case Key.Right:

                    if (_mouseHorizontalInput != 1)
                        _mouseHorizontalInput = 1;
                    else
                        _mouseHorizontalInput = 0;

                    break;

            }


        }

        private void ErrorMessage(string message)
        {

            ErrorMessage_Label.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => ErrorMessage_Label.Content = message));

            _errorOpacity = 1d;

            ErrorMessage_Label.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => ErrorMessage_Label.Opacity = _errorOpacity));

            _error = true;

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

                    foreach (ProcessModule processMod in _process[0].Modules)
                    {
                        Debug.WriteLine(processMod.ModuleName);

                        if (processMod.ModuleName == "SilentHill.exe")
                        {

                            _cameraX = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress, 4, out bytesRead));
                            _cameraZ = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 4, 4, out bytesRead));
                            _cameraY = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraCoordinateAddress + 8, 4, out bytesRead));

                            _procMod = processMod;

                            _isAttached = true;

                            InjectButton.IsEnabled = true;
                            AttachButton.IsEnabled = false;
                            AMouse_CheckBox.IsEnabled = true;

                        }

                    }

                }
                catch (IndexOutOfRangeException ior)
                {

                    Debug.WriteLine("Process Not Found");

                    ErrorMessage("Process Not Found");

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

                    MemoryUtility.WriteMemory(_process[0], _cameraXAssemblyAddress, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraYAssemblyAddress, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }, out bytesWritten);

                    _stopWatch.Reset();
                    _stopWatch.Start();

                    _isInjected = true;

                    InjectButton.Content = "UNINJECT";

                    AMouse_CheckBox.IsEnabled = true;

                }
                else if (_isInjected)
                {

                    _cameraForward.Dispose();
                    _cameraBackward.Dispose();
                    _cameraLeft.Dispose();
                    _cameraRight.Dispose();
                    _cameraUp.Dispose();
                    _cameraDown.Dispose();

                    if (_mouseEnabled)
                    {

                        _mouseUp.Dispose();
                        _mouseDown.Dispose();
                        _mouseLeft.Dispose();
                        _mouseRight.Dispose();

                        _mouseUp.ClearHotkeys();
                        _mouseDown.ClearHotkeys();
                        _mouseLeft.ClearHotkeys();
                        _mouseRight.ClearHotkeys();

                        _mouseEnabled = false;

                        AMouse_CheckBox.IsChecked = false;
                        AMouse_CheckBox.IsEnabled = false;

                        MouseS_Slider.IsEnabled = false;

                    }

                    _cameraForward.ClearHotkeys();
                    _cameraBackward.ClearHotkeys();
                    _cameraLeft.ClearHotkeys();
                    _cameraRight.ClearHotkeys();
                    _cameraUp.ClearHotkeys();
                    _cameraDown.ClearHotkeys();

                    MemoryUtility.WriteMemory(_process[0], _cameraXAssemblyAddress, new byte[] { 0x66, 0x0F, 0xD6, 0x42, 0x30 }, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraYAssemblyAddress, new byte[] { 0x66, 0x0F, 0xD6, 0x42, 0x38 }, out bytesWritten);

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

        private void CameraSpeed_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            _cameraSpeed = (float)CameraSpeed_Slider.Value;

            if (CameraSpeedValue_Label != null)
                CameraSpeedValue_Label.Content = _cameraSpeed.ToString();

        }

        private void MouseS_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            _mouseSensitivity = (int)MouseS_Slider.Value;

            if(MouseSValue_Label != null)
                MouseSValue_Label.Content = _mouseSensitivity.ToString();

        }

        private void AMouse_CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            _mouseEnabled = true;

            MouseS_Slider.IsEnabled = true;

            _mouseUp = new HotKey(Key.Up, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
            _mouseDown = new HotKey(Key.Down, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
            _mouseLeft = new HotKey(Key.Left, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
            _mouseRight = new HotKey(Key.Right, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);

        }

        private void AMouse_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            _mouseEnabled = false;

            MouseS_Slider.IsEnabled = false;

            _mouseUp.Dispose();
            _mouseDown.Dispose();
            _mouseLeft.Dispose();
            _mouseRight.Dispose();

            _mouseUp.ClearHotkeys();
            _mouseDown.ClearHotkeys();
            _mouseLeft.ClearHotkeys();
            _mouseRight.ClearHotkeys();

        }

        #endregion

    }

}
