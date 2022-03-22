using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace Drew
{
    public class HotKey : IDisposable
    {

		#region DLL Imports

		[DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		#endregion

		private static Dictionary<int, HotKey> _dictHotKeyToCalBackProc;

		public const int WmHotKey = 786;

		private bool _disposed;

		public Action<HotKey> Action
		{
			get;
			private set;
		}

		public int Id
		{
			get;
			set;
		}

		public Key Key
		{
			get;
			set;
		}

		public ModifierKeys mKeys
		{
			get;
			private set;
		}

		public HotKey(Key k, ModifierKeys keyModifiers, Action<HotKey> action, bool register = true)
		{
			Key = k;
			this.mKeys = keyModifiers;
			Action = action;
			if (register)
			{
				Register();
			}
		}

		public void ClearHotkeys()
		{
			_dictHotKeyToCalBackProc.Clear();
		}

		private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
		{
			HotKey hotKey;
			if (!handled && msg.message == 786 && _dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out hotKey))
			{
				if (hotKey.Action != null)
				{
					hotKey.Action(hotKey);
				}
				handled = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Unregister();
				}
				_disposed = true;
			}
		}

		public bool Register()
		{
			int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
			Id = virtualKeyCode + (int)mKeys * 65536;
			bool flag = RegisterHotKey(IntPtr.Zero, Id, (uint)mKeys, (uint)virtualKeyCode);
			if (_dictHotKeyToCalBackProc == null)
			{
				_dictHotKeyToCalBackProc = new Dictionary<int, HotKey>();
				ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
			}
			_dictHotKeyToCalBackProc.Add(Id, this);
			return flag;
		}

		public void Unregister()
		{
			HotKey hotKey;
			if (_dictHotKeyToCalBackProc.TryGetValue(Id, out hotKey))
			{
				UnregisterHotKey(IntPtr.Zero, Id);
			}
		}

	}

	//Don't forget to add key handler method where ever this is used.
}
