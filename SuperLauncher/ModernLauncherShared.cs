﻿using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SuperLauncher
{
    public static class Shared
    {
        public static double ScalePixelsUp(this DpiScale DPI, double Pixels)
        {
            return Pixels * DPI.DpiScaleX;
        }
        public static double ScalePixelsDown(this DpiScale DPI, double Pixels)
        {
            return Pixels / DPI.DpiScaleX;
        }
        public static string GetArugement(string ArgumentName)
        {
            string invokerArg = Array.Find(Program.Arguments, (value) => { return value.StartsWith("/" + ArgumentName + ":"); });
            if (invokerArg != null) return invokerArg.Substring(ArgumentName.Length + 2);
            return null;
        }
        public static Color GetColorizationColor()
        {
            try
            {
                RegistryKey key = Registry.Users.OpenSubKey(RunAsHelper.GetOriginalInvokerSID().ToString() + @"\Software\Microsoft\Windows\DWM");
                int themeColor = (int)key.GetValue("ColorizationColor");
                key.Close();
                byte[] colorBytes = BitConverter.GetBytes(themeColor);
                return Color.FromArgb(colorBytes[3], colorBytes[2], colorBytes[1], colorBytes[0]);
            }
            catch
            {
                return Colors.Black;
            }
        }
        public static byte AddAndClip(byte Original, byte Add)
        {
            if (Original + Add > 0xFF) return 0xFF;
            Original += Add;
            return Original;
        }
        public static Color MakeLighterNoAlpha(Color Color, byte Amount)
        {
            Color colorNoAlphaLighter = Color;
            colorNoAlphaLighter.A = 0xFF;
            colorNoAlphaLighter.R = AddAndClip(colorNoAlphaLighter.R, Amount);
            colorNoAlphaLighter.G = AddAndClip(colorNoAlphaLighter.G, Amount);
            colorNoAlphaLighter.B = AddAndClip(colorNoAlphaLighter.B, Amount);
            return colorNoAlphaLighter;
        }
        public static void SetWindowColor(Window Window)
        {
            Color color = GetColorizationColor();
            Window.Resources["AccentColor"] = MakeLighterNoAlpha(color, 0x65);
            Window.Resources["DarkerAccentColor"] = MakeLighterNoAlpha(color, 0x20);
        }
        public static void EnableAcrylic(Window Window)
        {
            Window.Background = Brushes.Transparent;
            IntPtr handle = new WindowInteropHelper(Window).Handle;
            IntPtr dwmDmVal = Marshal.AllocHGlobal(32);
            Marshal.WriteInt32(dwmDmVal, 1);
            Win32Interop.DwmSetWindowAttribute(handle, Win32Interop.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, dwmDmVal, 32);
            Marshal.FreeHGlobal(dwmDmVal);
            IntPtr dwmBdVal = Marshal.AllocHGlobal(32);
            Marshal.WriteInt32(dwmBdVal, 3);
            Win32Interop.DwmSetWindowAttribute(handle, Win32Interop.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, dwmBdVal, 32);
            Marshal.FreeHGlobal(dwmBdVal);
        }
    }
}