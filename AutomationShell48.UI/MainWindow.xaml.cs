using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using AutomationShell48.UI.ViewModels;

namespace AutomationShell48.UI
{
    public partial class MainWindow : Window
    {
        private const int WmGetMinMaxInfo = 0x0024;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var source = (HwndSource)PresentationSource.FromVisual(this);
            source?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmGetMinMaxInfo)
            {
                WmGetMinMaxInfoHandler(hwnd, lParam);
                handled = true;
            }

            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfoHandler(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = Marshal.PtrToStructure<MinMaxInfo>(lParam);
            var monitor = MonitorFromWindow(hwnd, 0x00000002);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MonitorInfo();
                monitorInfo.Size = Marshal.SizeOf(typeof(MonitorInfo));
                GetMonitorInfo(monitor, ref monitorInfo);

                var workArea = monitorInfo.WorkArea;
                var monitorArea = monitorInfo.MonitorArea;
                mmi.MaxPosition.X = Math.Abs(workArea.Left - monitorArea.Left);
                mmi.MaxPosition.Y = Math.Abs(workArea.Top - monitorArea.Top);
                mmi.MaxSize.X = Math.Abs(workArea.Right - workArea.Left);
                mmi.MaxSize.Y = Math.Abs(workArea.Bottom - workArea.Top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
                return;
            }

            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaxRestore_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                return;
            }

            const double minWidth = 960;
            const double minHeight = 640;

            var thumb = (Thumb)sender;
            var horizontal = e.HorizontalChange;
            var vertical = e.VerticalChange;

            switch (thumb.Name)
            {
                case "ResizeLeft":
                    if (Width - horizontal > minWidth)
                    {
                        Left += horizontal;
                        Width -= horizontal;
                    }
                    break;
                case "ResizeRight":
                    if (Width + horizontal > minWidth)
                    {
                        Width += horizontal;
                    }
                    break;
                case "ResizeTop":
                    if (Height - vertical > minHeight)
                    {
                        Top += vertical;
                        Height -= vertical;
                    }
                    break;
                case "ResizeBottom":
                    if (Height + vertical > minHeight)
                    {
                        Height += vertical;
                    }
                    break;
                case "ResizeTopLeft":
                    if (Width - horizontal > minWidth)
                    {
                        Left += horizontal;
                        Width -= horizontal;
                    }
                    if (Height - vertical > minHeight)
                    {
                        Top += vertical;
                        Height -= vertical;
                    }
                    break;
                case "ResizeTopRight":
                    if (Width + horizontal > minWidth)
                    {
                        Width += horizontal;
                    }
                    if (Height - vertical > minHeight)
                    {
                        Top += vertical;
                        Height -= vertical;
                    }
                    break;
                case "ResizeBottomLeft":
                    if (Width - horizontal > minWidth)
                    {
                        Left += horizontal;
                        Width -= horizontal;
                    }
                    if (Height + vertical > minHeight)
                    {
                        Height += vertical;
                    }
                    break;
                case "ResizeBottomRight":
                    if (Width + horizontal > minWidth)
                    {
                        Width += horizontal;
                    }
                    if (Height + vertical > minHeight)
                    {
                        Height += vertical;
                    }
                    break;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
            {
                return;
            }

            if (DataContext is ShellViewModel vm && vm.IsDialogOpen && vm.CloseDialogCommand.CanExecute(null))
            {
                vm.CloseDialogCommand.Execute(null);
                e.Handled = true;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MinMaxInfo
        {
            public Point Reserved;
            public Point MaxSize;
            public Point MaxPosition;
            public Point MinTrackSize;
            public Point MaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfo
        {
            public int Size;
            public RectStruct MonitorArea;
            public RectStruct WorkArea;
            public int Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RectStruct
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
