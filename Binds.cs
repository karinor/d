using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace MarcoMachine
{
    /*
    1. Усовершенствовать отсеивание чтобы оно не задевало бинд когда горячая клавиша есть в бинде.
    2. Почекать че с выбором потока (когда 2 раз выбираешь спамит месседжбокс)
    3. Подумать над выбором другого метода имитации
    */
    public enum EventType
    {
        KeyUp,
        KeyDown,
        MouseUp,
        MouseDown,
        MouseMove,
        Delay
    }

    public static class WindowsApi
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern void keybd_event(Key bVk, byte bScan, UInt32 dwFlags, IntPtr dwExtraInfo);

        public const UInt32 KEYEVENTF_EXTENDEDKEY = 1;
        public const UInt32 KEYEVENTF_KEYUP = 2;
        const int
            WM_LBUTTONDOWN = 513,
            WM_LBUTTONUP = 514,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_KEYDOWN = 256,
            WM_CHAR = 258,
            WM_KEYUP = 257,
            WM_SETFOCUS = 7,
            WM_SYSCOMMAND = 274,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_CLEAR = 0x303,
            WM_PAINT = 15,
            WM_SETCURSOR = 32,
            WM_KILLFOCUS = 8,
            WM_NCHITTEST = 132,
            WM_USER = 1024,
            WM_MOUSEACTIVATE = 33,
            WM_MOUSEMOVE = 512,
            WM_LBUTTONDBLCLK = 515,
            WM_COMMAND = 273,
            VK_DOWN = 0x28,
            VK_RETURN = 0x0D,
            BM_SETSTATE = 243,
            BM_CLICK = 0x00F5,
            SW_HIDE = 0,
            SW_MAXIMIZE = 3,
            SW_MINIMIZE = 6,
            SW_RESTORE = 9,
            SW_SHOW = 5,
            SW_SHOWDEFAULT = 10,
            SW_SHOWMAXIMIZED = 3,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOWNORMAL = 1,
            SC_MINIMIZE = 32,
            EM_SETSEL = 0x00B1,
            CAPACITY = 256,
            CB_SETCURSEL = 0x014E;
        public static class Keyboard
        {
            public static void SendKeyPress(Key key)
            {
                keybd_event((Key)KeyInterop.VirtualKeyFromKey(key), (byte)key, KEYEVENTF_EXTENDEDKEY | 0, IntPtr.Zero);
            }
            public static void SendKeyRelease(Key key)
            {
                keybd_event((Key)KeyInterop.VirtualKeyFromKey(key), (byte)key, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, IntPtr.Zero);
            }
        }
        class Mouse
        {
            public void ClickToCoordinate(IntPtr handle, int x, int y)
            {
                SendMessage(handle, WM_LBUTTONDOWN, IntPtr.Zero, new IntPtr(y * 0x10000 + x));
                SendMessage(handle, WM_LBUTTONUP, IntPtr.Zero, new IntPtr(y * 0x10000 + x));
            }
        }
    }

    public class MouseCords
    {
        public int X;
        public int Y;
        public MouseCords(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    #region 
    public class Event
    {
        public EventType eventType;
        Key ActionKey;
        MouseCords ActionClickCords;
        int Delay;
        string EventTypeString;
        string DisplayEvent;
        public Event(EventType type, Key ActionKey)
        {
            this.ActionKey = ActionKey;
            eventType = type;
            DisplayEvent = "Нажатие клавиши" + ActionKey.ToString();
            EventTypeString = eventType.ToString();
        }
        public Event(EventType type, MouseCords Cords)
        {
            eventType = type;
            ActionClickCords = Cords;
            DisplayEvent = $"Клик по координатам ({Cords.X}, {Cords.Y})";
            EventTypeString = eventType.ToString();
        }
        public Event(EventType type, int Delay)
        {
            eventType = type;
            this.Delay = Delay;
            DisplayEvent = $"Задержка {Delay} мс";
            EventTypeString = eventType.ToString();
        }

 

        public void Run()
        {
            switch (eventType)
            {
                case EventType.KeyUp:
                    WindowsApi.Keyboard.SendKeyPress(ActionKey);
                    break;
                case EventType.KeyDown:
                    WindowsApi.Keyboard.SendKeyPress(ActionKey);
                    break;
                case EventType.Delay:
                    System.Threading.Thread.Sleep(Delay);
                    break;
                case EventType.MouseDown:
                    //...do something
                    break;
                case EventType.MouseMove:
                    //...do something
                    break;
                case EventType.MouseUp:
                    //...do something
                    break;
            }
        }
    }

    public class Hook
    {
        private uint pid;
        const int WH_KEYBOARD_LL = 13;
        const int HC_ACTION = 0;
        const int WM_KEYDOWN = 0x0100;
        const uint VK_CAPITAL = 0x14;
        static uint hookProcessID;
        static IntPtr hookID;
        static KeyboardProc Callback = KeyboardHookCallback;
        public static BindExecutor BindExec = new BindExecutor();
        static Dictionary<Key, Bind> BindList;
        [Flags]
        public enum KBDLLHOOKSTRUCTFlags : uint
        {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80,
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        public Hook(uint pid, Dictionary<Key, Bind> bl)
        {
            this.pid = pid;
            BindList = bl;
        }

        public void BindListUpdate(Dictionary<Key, Bind> bl)
        {
            BindList = bl;
        }

        public void CalculatedBindDelay()
        {

        }
        
        public void SetHook()
        {
            hookProcessID = pid;
            hookID = SetWindowsHookEx(WH_KEYBOARD_LL, Callback, IntPtr.Zero, 0);
            if (hookID == IntPtr.Zero) MessageBox.Show("Не удалось подключиться к окну :c");
            else 
                MessageBox.Show("Подключено.");
        }

        static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                KBDLLHOOKSTRUCT kbd = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                int vkCode = (int)kbd.vkCode;
                uint pid = 0;
                GetWindowThreadProcessId(GetForegroundWindow(), out pid);

                if (pid == hookProcessID)
                {
                    if (kbd.flags.HasFlag(KBDLLHOOKSTRUCTFlags.LLKHF_INJECTED))
                    {
                        //Если это фек нажатие просто паслать нахуй!1!
                        if (!BindRuntimeInfo.BindRunning)
                        {
                            if (BindList.ContainsKey(KeyInterop.KeyFromVirtualKey(vkCode))) return (IntPtr)1;
                        }
                    }
                    else
                    {
                        BindExec.isHotKey(KeyInterop.KeyFromVirtualKey(vkCode), BindList);
                        if (BindList.ContainsKey(KeyInterop.KeyFromVirtualKey(vkCode))) return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

    }

    public class Bind
    {
        public string Name { get; set; }
        public Key HotKey
        {
            get
            {
                return HotKey;
            }
            set
            {
                HotKey = value;
                HKString = HotKey.ToString();
            }
        }
        public string HKString { get; set; }
        public List<Event> Events = new List<Event>();

        public Bind()
        {

        }

        public Bind(string Name, Key HotKey)
        {
            this.Name = Name;
            this.HotKey = HotKey;
            this.HKString = HotKey.ToString();
        }

        public void Execute()
        {
            BindRuntimeInfo.BindRunning = true;
            foreach(Event @event in Events)
            {
                @event.Run();
            }
            BindRuntimeInfo.BindRunning = false;
        }

    }
    public class BindRuntimeInfo
    {
        public static Dictionary<Key, Bind> BindList = new Dictionary<Key, Bind>();
        public static bool BindRunning = false;
        public static List<Bind> bindObjects = new List<Bind>();
        public static MainWindow link { private get; set; }

        public static void AddBind(Bind bind)
        {
            bindObjects.Add(bind);
            link.UpdateBindList(bindObjects.ToArray());
        }

    }

    public class BindExecutor
    {
        

        public void UpdateBindList(Bind[] binds, Dictionary<Key, Bind> BindList)
        {
            BindList.Clear();
            foreach (Bind bind in binds)
                BindList[bind.HotKey] = bind;
        }

        public void isHotKey(Key key, Dictionary<Key, Bind> BindList)
        {
            if (BindList.ContainsKey(key))
            {
                Task.Factory.StartNew(() => BindList[key].Execute());
            }
        }
    }
    #endregion


}
