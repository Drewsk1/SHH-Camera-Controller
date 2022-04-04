using Drew;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace SHH_Camera_Controller
{
    public partial class MainWindow : Window
    {

        #region Fields

        private const float Radians2Degrees = 57.29578f;

        private Thread? _guiThread;
        private Thread? _injectThread;

        private int _recIndex;
        private ScreenRecorder _screenRecorder;

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
        private HotKey _screenRecord;

        private float _cameraSpeed;
        private float _cameraX;
        private float _cameraY;
        private float _cameraZ;

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
        private bool _recordEnabled;

        private bool _isAttached;
        private bool _isInjected;
        private bool _isRecording;

        private bool _running;

        private bool _error;

        private double _errorOpacity;

        private Process[] _process;
        private ProcessModule _procMod;

        private IntPtr _cameraCoordinateAddress;
        private IntPtr _cameraYawAddress;
        private IntPtr _cameraPitchAddress;

        private IntPtr _cameraNewXAddress;

        private IntPtr _cameraXAssemblyAddress;
        private IntPtr _cameraYAssemblyAddress;

        #endregion

        public MainWindow()
        {

            InitializeComponent();

            _cameraCoordinateAddress = (IntPtr)0x1158F1C0;
            _cameraYawAddress = (IntPtr)0x1158F190;
            _cameraPitchAddress = (IntPtr)0x1158F1A4;
            _cameraNewXAddress = (IntPtr)0x1158F188;

            _cameraXAssemblyAddress = (IntPtr)0x1053CC8A; // 66 0F D6 42 30 // 66 0F D6 42 F8 // send x to unused address
            _cameraYAssemblyAddress = (IntPtr)0x1053CC94; // 66 0F D6 42 38 // 90 90 90 90 90

            _recIndex = 0;

            _cameraSpeed = 0.5f;

            _cameraX = 0f;
            _cameraY = 0f;
            _cameraZ = 0f;

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
            _recordEnabled = false;

            _isAttached = false;
            _isInjected = false;
            _isRecording = false;

            _error = false;

            _errorOpacity = 0d;

            AttachButton.IsEnabled = true;
            InjectButton.IsEnabled = false;

            _running = true;

            _guiThread = new Thread(new ThreadStart(GUIThread));
            _guiThread.Start();
            while (!_guiThread.IsAlive) ;

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

            while (_running)
            {

                if (_isInjected)
                {

                    float newValue = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraNewXAddress, 4, out bytesRead));

                    if (newValue != 99999f)
                    {

                        MemoryUtility.WriteMemory(_process[0], _cameraNewXAddress, 99999f, out bytesWritten);


                        if (_cameraHorizontalInput != 0 || _cameraForwardInput != 0 || _cameraVerticalInput != 0)
                        {

                            _cameraYawSine = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraYawAddress, 4, out bytesRead));
                            _cameraYawCosine = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraYawAddress + 32, 4, out bytesRead));

                            _cameraPitchSine = BitConverter.ToSingle(MemoryUtility.ReadMemory(_process[0], _cameraPitchAddress + 16, 4, out bytesRead));


                            if (_cameraForwardInput != 0)
                            {

                                _cameraY += (_cameraYawSine * _cameraSpeed) * (float)_cameraForwardInput;
                                _cameraX += (_cameraYawCosine * _cameraSpeed) * (float)_cameraForwardInput;
                                _cameraZ += (_cameraPitchSine * _cameraSpeed) * (float)_cameraForwardInput;

                            }
                            else if (_cameraHorizontalInput != 0)
                            {

                                _cameraX += (_cameraYawSine * _cameraSpeed) * (float)_cameraHorizontalInput;
                                _cameraY += (-_cameraYawCosine * _cameraSpeed) * (float)_cameraHorizontalInput;

                            }

                            if (_cameraVerticalInput != 0)
                            {

                                _cameraZ += (float)_cameraVerticalInput * _cameraSpeed;

                            }

                            MemoryUtility.WriteMemory(_process[0], _cameraCoordinateAddress, _cameraX, out bytesWritten);
                            MemoryUtility.WriteMemory(_process[0], _cameraCoordinateAddress + 4, _cameraZ, out bytesWritten);
                            MemoryUtility.WriteMemory(_process[0], _cameraCoordinateAddress + 8, _cameraY, out bytesWritten);

                        }

                        if (_mouseEnabled)
                        {

                            if (_mouseHorizontalInput != 0)
                                MouseUtility.MoveMouseRelative(_mouseHorizontalInput * _mouseSensitivity, 0);

                            if (_mouseVerticalInput != 0)
                                MouseUtility.MoveMouseRelative(0, _mouseVerticalInput * _mouseSensitivity);

                        }

                        if (_isRecording)
                        {

                            Thread.Sleep(50); //Gives game enough time to draw frame will look choppy in-game but smooth in video. This recording style is specific to a video project I'm making.
                                              //This screen recording implementation will hopefully be smoothed out in the future.

                            _screenRecorder.CaptureFrame();

                        }

                    }
                    
                    Thread.Sleep(1);

                }
                else
                    Thread.Sleep(500);


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
                case Key.R:

                    if(_recordEnabled)
                        if (!_isRecording)
                        {

                            RecordIndicator.Fill = Brushes.Red;

                            _screenRecorder = new ScreenRecorder("recording" + _recIndex++.ToString() + ".avi", 30, 70, 30);

                            _isRecording = true;

                        }
                        else
                        {

                            _isRecording = false;

                            RecordIndicator.Fill = Brushes.Black;

                            _screenRecorder.Dispose();

                            Thread.Sleep(100);

                        }

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

                            _injectThread = new Thread(new ThreadStart(InjectThread));
                            _injectThread.Start();
                            while (!_injectThread.IsAlive) ;

                            break;

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


                    MemoryUtility.WriteMemory(_process[0], _cameraXAssemblyAddress, new byte[] { 0x66, 0x0F, 0xD6, 0x42, 0xF8 }, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraYAssemblyAddress, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }, out bytesWritten);

                    _isInjected = true;

                    InjectButton.Content = "UNINJECT";

                    AMouse_CheckBox.IsEnabled = true;

                    ScreenRec_CheckBox.IsEnabled = true;

                }
                else
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

                    }

                    if (_recordEnabled)
                    {

                        if (_isRecording)
                        {

                            _screenRecorder.Dispose();

                            _isRecording = false;

                        }

                        _screenRecord.Dispose();
                        _screenRecord.ClearHotkeys();

                        ScreenRec_CheckBox.IsChecked = false;
                        ScreenRec_CheckBox.IsEnabled = false;

                    }
                    
                    MemoryUtility.WriteMemory(_process[0], _cameraXAssemblyAddress, new byte[] { 0x66, 0x0F, 0xD6, 0x42, 0x30 }, out bytesWritten);
                    MemoryUtility.WriteMemory(_process[0], _cameraYAssemblyAddress, new byte[] { 0x66, 0x0F, 0xD6, 0x42, 0x38 }, out bytesWritten);

                    _isInjected = false;

                    InjectButton.Content = "INJECT";


                    Thread.Sleep(500);

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

            if(_injectThread != null)
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

            _mouseUp = new HotKey(Key.Up, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
            _mouseDown = new HotKey(Key.Down, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
            _mouseLeft = new HotKey(Key.Left, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);
            _mouseRight = new HotKey(Key.Right, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);

        }

        private void AMouse_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            _mouseEnabled = false;

            _mouseUp.Dispose();
            _mouseDown.Dispose();
            _mouseLeft.Dispose();
            _mouseRight.Dispose();

            _mouseUp.ClearHotkeys();
            _mouseDown.ClearHotkeys();
            _mouseLeft.ClearHotkeys();
            _mouseRight.ClearHotkeys();

            Thread.Sleep(500);

        }

        private void ScreenRec_CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            _recordEnabled = true;

            _screenRecord = new HotKey(Key.R, ModifierKeys.None, new Action<HotKey>(OnHotKeyHandler), true);

        }

        private void ScreenRec_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            _recordEnabled = false;
            _isRecording = false;

            RecordIndicator.Fill = Brushes.Black;

            _screenRecord.Dispose();
            _screenRecord.ClearHotkeys();

            Thread.Sleep(500);

        }

        #endregion

    }

}
