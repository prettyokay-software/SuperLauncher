﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Input;
using System.Timers;
using System.Runtime.InteropServices;

namespace SuperLauncher
{
    public partial class ModernLauncherIcon : UserControl
    {
        public string rFilePath;
        public bool rFilterFocus = false;
        public ModernLauncherBadge Badge;
        public bool IsMouseDown = false;
        public string FileName;
        public Timer BadgeTimer = new()
        {
            Interval = 1000,
            Enabled = true,
            AutoReset = false
        };
        private ModernLauncher PWindow;
        public string FilePath
        {
            get
            {
                return rFilePath;
            }
            set
            {
                rFilePath = value;
                FileInfo fi = new(rFilePath);
                Icon icon = Icon.ExtractAssociatedIcon(rFilePath);
                FileName = fi.Name;
                NameText.Text = ExtRemover(FileName);
                LIcon.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }
        public bool FilterFocus
        {
            get
            {
                return rFilterFocus;
            }
            set
            {
                rFilterFocus = value;
                if (value)
                {
                    Highlight.BeginAnimation(OpacityProperty, To1);
                }
                else
                {
                    Highlight.BeginAnimation(OpacityProperty, To0);
                }
            }
        }
        public ModernLauncherIcon(string FilePath)
        {
            InitializeComponent();
            this.FilePath = FilePath;
        }
        private readonly DoubleAnimation To1 = new()
        {
            To = 1,
            Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100))
        };
        private readonly DoubleAnimation To0 = new()
        {
            To = 0,
            Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100))
        };
        private readonly DoubleAnimation To0_5 = new()
        {
            To = 0.5,
            Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100))
        };
        private readonly DoubleAnimation To0_9 = new()
        {
            To = 0.9,
            Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100))
        };
        private void UserControl_MouseEnter(object sender, object e)
        {
            StartBadgeTimer();
            Highlight.BeginAnimation(OpacityProperty, To1);
        }
        private void UserControl_MouseLeave(object sender, object e)
        {
            if (Badge != null) Badge.Close();
            Highlight.BeginAnimation(OpacityProperty, To0);
            IconScale.BeginAnimation(ScaleTransform.ScaleXProperty, To1);
            IconScale.BeginAnimation(ScaleTransform.ScaleYProperty, To1);
        }
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                IsMouseDown = true;
                Highlight.BeginAnimation(OpacityProperty, To0_5);
                IconScale.BeginAnimation(ScaleTransform.ScaleXProperty, To0_9);
                IconScale.BeginAnimation(ScaleTransform.ScaleYProperty, To0_9);
            }
        }
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (IsMouseDown)
                {
                    StartProgram();
                }
                IsMouseDown = false;
                Highlight.BeginAnimation(OpacityProperty, To1);
                IconScale.BeginAnimation(ScaleTransform.ScaleXProperty, To1);
                IconScale.BeginAnimation(ScaleTransform.ScaleYProperty, To1);
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                PWindow.IgnoreDeactivation = true;
                ModernLauncherContextMenu menu = new();
                ModernLauncherContextMenuIcon menuItems = new(this);
                menu.Frame.Content = menuItems;
                Win32Interop.GetCursorPos(out Win32Interop.POINT point);
                menu.Left = ModernLauncher.DPI.ScalePixelsDown(point.x);
                menu.Top = ModernLauncher.DPI.ScalePixelsDown(point.y) - 100;
                menuItems.MouseUp += Menu_MouseUp;
                menu.Show();
                PWindow.IgnoreDeactivation = false;
            }
        }
        private void Menu_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ModernLauncherContextMenuIcon menu = (ModernLauncherContextMenuIcon)sender;
            if (e.Source == menu.BtnUnpin)
            {
                PWindow.Focus();
            }
            else
            {
                PWindow.CloseWindow();
            }
        }
        private static string ExtRemover(string FileName)
        {
            string[] parts = FileName.Split('.');
            if (parts.Length > 1)
            {
                string[] newParts = new string[parts.Length - 1];
                for (int i = 0; i < newParts.Length; i++) newParts[i] = parts[i];
                return string.Join('.', newParts);
            }
            return FileName;
        }
        public void StartBadgeTimer()
        {
            BadgeTimer.Stop();
            BadgeTimer.Start();
        }
        private void BadgeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                if (!IsMouseOver) return;
                if (Badge != null) Badge.Close();
                Badge = new(FileName);
                Badge.Show();
            });
        }
        public void StartProgram()
        {
            ((ModernLauncher)Window.GetWindow(this)).CloseWindow();
            Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = FilePath,
                        UseShellExecute = true,
                        WorkingDirectory = Environment.GetEnvironmentVariable("USERPROFILE")
                    });
                }
                catch { }
            });
        }
        private void UserControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Highlight.BeginAnimation(OpacityProperty, To1);
        }
        private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Highlight.BeginAnimation(OpacityProperty, To0);
        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                Highlight.BeginAnimation(OpacityProperty, To0_5);
                IconScale.BeginAnimation(ScaleTransform.ScaleXProperty, To0_9);
                IconScale.BeginAnimation(ScaleTransform.ScaleYProperty, To0_9);
            }
        }
        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                StartProgram();
                Highlight.BeginAnimation(OpacityProperty, To1);
                IconScale.BeginAnimation(ScaleTransform.ScaleXProperty, To1);
                IconScale.BeginAnimation(ScaleTransform.ScaleYProperty, To1);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PWindow = (ModernLauncher)Window.GetWindow(this);
            BadgeTimer.Elapsed += BadgeTimer_Elapsed;
        }
    }
}
