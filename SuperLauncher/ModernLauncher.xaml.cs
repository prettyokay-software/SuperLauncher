﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Controls;

namespace SuperLauncher
{
    /// <summary>
    /// Interaction logic for ModernLauncher.xaml
    /// </summary>
    public partial class ModernLauncher : Window
    {
        public static DpiScale DPI
        {
            get { return rDPI; }
            set { rDPI = value; }
        }
        public bool IgnoreDeactivation = false;
        private static DpiScale rDPI;
        private WindowInteropHelper WIH;
        private HwndSource HWND;
        private readonly DoubleAnimation RenderBoostAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(1),
            From = 1,
            To = 0
        };
        private readonly DoubleAnimation OpenTopAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.Stop
        };
        private readonly DoubleAnimation CloseTopAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseIn },
            FillBehavior = FillBehavior.Stop
        };
        private readonly DoubleAnimation OpenLeftAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.Stop
        };
        private readonly DoubleAnimation CloseLeftAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseIn },
            FillBehavior = FillBehavior.Stop
        };
        private bool Visible = false;
        public ModernLauncher()
        {
            InitializeComponent();
            Width = Settings.Default.Width;
            Height = Settings.Default.Height;
        }
        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312 && wParam.ToInt32() == 0) //WM_HOTKEY //ALT + S
            {
                OpenWindow(Center: true);
            }
            if (msg == 0x0312 && wParam.ToInt32() == 1) //WM_HOTKEY //ALT + E
            {
                new ShellHost().Show();
            }
            if (msg == 0x0312 && wParam.ToInt32() == 2) //WM_HOTKEY //ALT + R
            {
                new Run().Show();
            }
            return IntPtr.Zero;
        }
        private void InitializeNotifyIcon()
        {
            if (ModernLauncherNotifyIcon.Icon != null) ModernLauncherNotifyIcon.Icon.Dispose();
            ModernLauncherNotifyIcon.Initialize();
            ModernLauncherNotifyIcon.Icon.Click += Icon_Click;
        }
        private void Icon_Click(object sender, EventArgs e)
        {
            OpenWindow();
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (IgnoreDeactivation) return;
            if (Visible) CloseWindow();
        }
        private void UpdateAnimations(bool Center = false)
        {
            Win32Interop.MONITORINFO mi = new();
            mi.cbSize = (uint)Marshal.SizeOf(mi);
            IntPtr hMonitor = Win32Interop.MonitorFromWindow(WIH.Handle, Win32Interop.MonitorFromWindowFlags.MONITOR_DEFAULTTONEAREST);
            Win32Interop.GetMonitorInfo(hMonitor, out mi);
            OpenTopAnimation.From = CloseTopAnimation.From = Top;
            CloseTopAnimation.To = DPI.ScalePixelsDown(mi.rcMonitor.bottom);
            if (Center)
            {
                OpenLeftAnimation.To = ((DPI.ScalePixelsDown(mi.rcWork.right) / 2) - Width / 2);
                OpenTopAnimation.To = ((DPI.ScalePixelsDown(mi.rcWork.bottom) / 2) - Height / 2);
            }
            else
            {
                OpenLeftAnimation.To = ((DPI.ScalePixelsDown(mi.rcWork.right)) - Width - 8);
                OpenTopAnimation.To = (DPI.ScalePixelsDown(mi.rcWork.bottom) - Height - 10);
            }
        }
        private void SetElevateLabels()
        {
            ElevateUser.Content = RunAsHelper.GetCurrentDomainWithUserName();
            if (RunAsHelper.IsElevated())
            {
                ElevateIcon.Content = "";
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetPosition();
            WIH = new(this);
            HWND = HwndSource.FromHwnd(WIH.Handle);
            Program.ModernApplication.Exit += ModernApplication_Exit;
            Win32Interop.EnableBlur(WIH.Handle, 200, 0);
            InitializeNotifyIcon();
            Win32Interop.RegisterHotKey(WIH.Handle, 0, 0x1 | 0x4000, 0x53); //Register Hot Key ALT + S
            Win32Interop.RegisterHotKey(WIH.Handle, 1, 0x1 | 0x4000, 0x45); //Register Hot Key ALT + E
            Win32Interop.RegisterHotKey(WIH.Handle, 2, 0x1 | 0x4000, 0x52); //Register Hot Key ALT + R
            HWND.AddHook(HwndSourceHook);
            SetElevateLabels();
        }
        private void ModernApplication_Exit(object sender, ExitEventArgs e)
        {
            if (ModernLauncherNotifyIcon.Icon != null) ModernLauncherNotifyIcon.Icon.Dispose();
        }
        public async void OpenWindow(bool Center = false)
        {
            Visible = true;
            UpdateAnimations(Center: Center);
            Activate();
            _ = Win32Interop.SetWindowLong(WIH.Handle, Win32Interop.SetWindowLongIndex.GWL_EXSTYLE, Win32Interop.ExtendedWindowStyles.WS_EX_LAYERED);
            BeginAnimation(TopProperty, OpenTopAnimation);
            BeginAnimation(LeftProperty, OpenLeftAnimation);
            RenderBoost.BeginAnimation(OpacityProperty, RenderBoostAnimation);
            Filter.Text = "";
            Filter.Focus();
            await Task.Delay(310);
            Win32Interop.InvalidateRect(WIH.Handle, IntPtr.Zero, false);
        }
        public async void CloseWindow()
        {
            Visible = false;
            UpdateAnimations();
            Activate();
            BeginAnimation(TopProperty, CloseTopAnimation);
            BeginAnimation(LeftProperty, CloseLeftAnimation);
            RenderBoost.BeginAnimation(OpacityProperty, RenderBoostAnimation);
            await Task.Delay(310);
            SetTopPosition();
            _ = Win32Interop.SetWindowLong(WIH.Handle, Win32Interop.SetWindowLongIndex.GWL_EXSTYLE, Win32Interop.ExtendedWindowStyles.WS_EX_TOOLWINDOW);
            SaveSettingsIfSizeChanged();
        }
        private void SetPosition()
        {
            SetTopPosition();
            SetLeftPosition();
        }
        private void SaveSettingsIfSizeChanged()
        {
            if (
                Settings.Default.Width == (int)Width &&
                Settings.Default.Height == (int)Height
            ) {
                return;
            }
            Settings.Default.Width = (int)Width;
            Settings.Default.Height = (int)Height;
            Settings.Default.Save();
        }
        private void SetTopPosition()
        {
            Top = SystemParameters.PrimaryScreenHeight;
        }
        private void SetLeftPosition()
        {
            Left = SystemParameters.PrimaryScreenWidth - Width - 8;
        }
        private void Window_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            if (e.NewDpi.DpiScaleX != DPI.DpiScaleX)
            {
                DPI = e.NewDpi;
                InitializeNotifyIcon();
            }
        }
        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            MLI.Filter = Filter.Text;
        }
        private void Filter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) MoveFocus(new(FocusNavigationDirection.Next));
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Visible && e.Key == Key.Escape) CloseWindow();
        }
        private void BtnExplorer_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
            new ShellHost().Show();
        }
        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
            new Run().Show();
        }
        private void BtnMore_Click(object sender, RoutedEventArgs e)
        {
            IgnoreDeactivation = true;
            ModernLauncherContextMenu menu = new();
            menu.Frame.Content = new ModernLauncherContextMenuMain();
            menu.Closed += Menu_Closed;
            menu.MouseUp += Menu_MouseUp;
            menu.Top = Top + Height - 230;
            menu.Left = Left + Width - 180;
            menu.Show();
            IgnoreDeactivation = false;
        }
        private void Menu_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CloseWindow();
        }
        private void Menu_Closed(object sender, EventArgs e)
        {
            Activate();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new()
            {
                InitialDirectory = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
                Multiselect = true,
                Title = "Select the items you would like to pin to Super Launcher",
                DereferenceLinks = false
            };
            dialog.ShowDialog();
            foreach (string file in dialog.FileNames)
            {
                Settings.Default.FileList.Add(file);
            }
            Settings.Default.Save();
            ((ModernLauncher)Program.ModernApplication.MainWindow).MLI.PopulateIcons();
            ((ModernLauncher)Program.ModernApplication.MainWindow).OpenWindow();
        }
    }
} 