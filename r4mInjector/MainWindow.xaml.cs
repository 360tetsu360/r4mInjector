using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Windows.Media.Animation;
using System.Windows.Interop;
using System.Drawing.Printing;

namespace r4mInjector
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [DllImport("User32")]
        public static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        [DllImport("user32")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT { };
            public RECT rcWork = new RECT { };
            public int dwFlags = 0;
        }

        public struct RECT
        {
            public int left, top, right, bottom;

            public RECT(int Left, int Top, int Right, int Bottom)
            {
                this.left = Left;
                this.top = Top;
                this.right = Right;
                this.bottom = Bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return left; }
                set { right -= (left - value); left = value; }
            }

            public int Y
            {
                get { return top; }
                set { bottom -= (top - value); top = value; }
            }

            public int Height
            {
                get { return bottom - top; }
                set { bottom = value + top; }
            }

            public int Width
            {
                get { return right - left; }
                set { right = value + left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(left, top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.left, r.top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.left == left && r.top == top && r.right == right && r.bottom == bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }
        }

        bool IsClosing = false;
        bool CanClose = false;
        private void Window_StateChanged(object sender, EventArgs e)
        {
            this.WindowControlButton_Maximize_Refresh();
        }
        private void WindowControlButton_Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void WindowControlButton_Maximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
        private void WindowControlButton_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void WindowControlButton_Maximize_Refresh()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                WindowBorderBorder.Visibility = Visibility.Hidden;
                this.WindowControlButton_Maximize.Visibility = Visibility.Collapsed;
                this.WindowControlButton_MaximizeRestore.Visibility = Visibility.Visible;
            }
            else
            {
                WindowBorderBorder.Visibility = Visibility.Visible;
                this.WindowControlButton_Maximize.Visibility = Visibility.Visible;
                this.WindowControlButton_MaximizeRestore.Visibility = Visibility.Collapsed;
            }
        }
        private void CONSTRUCT()
        {
            WindowControlButton_Maximize_Refresh();

            #region MaximizingFix
            SourceInitialized += (s, e) =>
            {
                WindowCompositionTarget = PresentationSource.FromVisual(this).CompositionTarget;
                System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(this).Handle).AddHook(WindowProc);
            };
            #endregion
        }
        public MainWindow()
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            CONSTRUCT();
        }

        private ObservableCollection<Dto> _dtos = new ObservableCollection<Dto>();
        ConsoleWindow console;
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Dlllist.ItemsSource = _dtos;
            console = new ConsoleWindow();
            console.Hide();

            StringCollection files = Properties.Settings.Default.RecentFiles;
            if(files == null)
            {
                Properties.Settings.Default.RecentFiles= new StringCollection();
            }
            if(files != null)
            {
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    if (fileInfo.Directory.Exists)
                    {
                        string fileName = Path.GetFileName(file);
                        _dtos.Add(new Dto(fileName, file));
                    }
                }
            }
            this.Icon = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "";
            ofd.DefaultExt = "*.*";
            ofd.Filter = "DLL|*.dll";
            if (ofd.ShowDialog() == true)
            {
                string fileName = Path.GetFileName(ofd.FileName);
                _dtos.Insert(0,new Dto(fileName, ofd.FileName));
                bool isinprop = false;
                StringCollection files = Properties.Settings.Default.RecentFiles;
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        if (fileName == file)
                        {
                            isinprop = true;
                        }
                    }
                }
                if (isinprop)
                {
                    Properties.Settings.Default.RecentFiles.Remove(fileName);
                }
                Properties.Settings.Default.RecentFiles.Insert(0,ofd.FileName);
                Properties.Settings.Default.Save();
            }
        }

        private void injectClick(object sender, RoutedEventArgs e)
        {
            if(Dlllist.SelectedItem != null)
            {
                Injector.awaitProcess("Minecraft.Windows");
                string dllpath = (Dlllist.SelectedItem as Dto).FilePath;
                if (File.Exists(dllpath))
                {
                    Injector.applyAppPackages(dllpath);
                    Injector.InjectDll(dllpath);
                }
                else
                {
                    MessageBox.Show("Select a dll");
                }
            }
            else
            {
                MessageBox.Show("Select a dll");
            }
        }

        private void OpenLogger(object sender, RoutedEventArgs e)
        {
            console.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CanClose == true)
            {
                return;
            }
            if (IsClosing == false)
            {
                var ta = new DoubleAnimation();
                ta.Duration = TimeSpan.FromSeconds(0.2);
                QuadraticEase EasingFunction = new QuadraticEase();
                EasingFunction.EasingMode = EasingMode.EaseOut;
                ta.EasingFunction = EasingFunction;
                ta.To = 0;
                ta.Completed += (object sendera, EventArgs e1) => { CanClose = true; this.Close(); };
                this.BeginAnimation(OpacityProperty, ta);
                IsClosing = true;
                e.Cancel = true;
            }
            else
            {
                e.Cancel = true;
            }

            Data.run = false;
            if(console != null)
            {
                console.kill();
            }
        }

        CompositionTarget WindowCompositionTarget { get; set; }

        double CachedMinWidth { get; set; }

        double CachedMinHeight { get; set; }

        POINT CachedMinTrackSize { get; set; }

        IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    MINMAXINFO mmi = (MINMAXINFO)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
                    IntPtr monitor = MonitorFromWindow(hwnd, 0x00000002 /*MONITOR_DEFAULTTONEAREST*/);
                    if (monitor != IntPtr.Zero)
                    {
                        MONITORINFO monitorInfo = new MONITORINFO { };
                        GetMonitorInfo(monitor, monitorInfo);
                        RECT rcWorkArea = monitorInfo.rcWork;
                        RECT rcMonitorArea = monitorInfo.rcMonitor;
                        mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                        mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                        mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                        mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                        if (!CachedMinTrackSize.Equals(mmi.ptMinTrackSize) || CachedMinHeight != MinHeight && CachedMinWidth != MinWidth)
                        {
                            mmi.ptMinTrackSize.x = (int)((CachedMinWidth = MinWidth) * WindowCompositionTarget.TransformToDevice.M11);
                            mmi.ptMinTrackSize.y = (int)((CachedMinHeight = MinHeight) * WindowCompositionTarget.TransformToDevice.M22);
                            CachedMinTrackSize = mmi.ptMinTrackSize;
                        }
                    }
                    System.Runtime.InteropServices.Marshal.StructureToPtr(mmi, lParam, true);
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Data.run = false;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Data.run = false;

        }
    }
    public static class DwmDropShadow
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        /// <summary>
        /// Drops a standard shadow to a WPF Window, even if the window is borderless. Only works with DWM (Windows Vista or newer).
        /// This method is much more efficient than setting AllowsTransparency to true and using the DropShadow effect,
        /// as AllowsTransparency involves a huge performance issue (hardware acceleration is turned off for all the window).
        /// </summary>
        /// <param name="window">Window to which the shadow will be applied</param>
        public static void DropShadowToWindow(Window window)
        {
            if (!DropShadow(window))
            {
                window.SourceInitialized += new EventHandler(window_SourceInitialized);
            }
        }

        private static void window_SourceInitialized(object sender, EventArgs e)
        {
            Window window = (Window)sender;

            DropShadow(window);

            window.SourceInitialized -= new EventHandler(window_SourceInitialized);
        }

        /// <summary>
        /// The actual method that makes API calls to drop the shadow to the window
        /// </summary>
        /// <param name="window">Window to which the shadow will be applied</param>
        /// <returns>True if the method succeeded, false if not</returns>
        private static bool DropShadow(Window window)
        {
            try
            {
                WindowInteropHelper helper = new WindowInteropHelper(window);
                int val = 2;
                int ret1 = DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

                if (ret1 == 0)
                {
                    Margins m = new Margins { Bottom = 0, Left = 0, Right = 0, Top = 0 };
                    int ret2 = DwmExtendFrameIntoClientArea(helper.Handle, ref m);
                    return ret2 == 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Probably dwmapi.dll not found (incompatible OS)
                return false;
            }
        }
    }

    public sealed class Dto
    {
        public Dto(string name,string path)
        {
            Name = name;
            FilePath = path;
        }

        public string Name { get; set; }
        public string FilePath { get; set; }
    }
}
