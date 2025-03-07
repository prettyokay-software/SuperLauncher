﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SuperLauncherCommon
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        public string vMessageText = "";
        public bool AllowClose = true;
        public string MessageText { 
            get
            {
                return vMessageText;
            }
            set
            {
                vMessageText = value;
                Application.Current.Dispatcher.Invoke(() => {
                    Message.Content = vMessageText;
                });
            }
        }
        private readonly string Site = "";
        private bool fadeOutPlayed = false;
        public Splash()
        {
            InitializeComponent();
            Assembly self = Assembly.GetEntryAssembly()!;
            Copyright.Content = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(self, typeof(AssemblyCopyrightAttribute))!).Copyright;
            Author.Content = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(self, typeof(AssemblyCompanyAttribute))!).Company;
            Version.Content = "v" + ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(self, typeof(AssemblyInformationalVersionAttribute))!).InformationalVersion;
            Site = ((AssemblyMetadataAttribute)Attribute.GetCustomAttribute(self, typeof(AssemblyMetadataAttribute))!).Value!;
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            Opacity = 0;
            DoubleAnimation fadein = new()
            {
                Duration = TimeSpan.FromSeconds(0.3),
                From = 0,
                To = 1
            };
            DoubleAnimation scalein = new()
            {
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut },
                From = 0.9,
                To = 1
            };
            BeginAnimation(OpacityProperty, fadein);
            GridTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scalein);
            GridTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scalein);
            IEasingFunction se = new SineEase();
            ((SineEase)se).EasingMode = EasingMode.EaseInOut;
            ThicknessAnimationUsingKeyFrames horz = new()
            {
                Duration = TimeSpan.FromSeconds(3),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            horz.KeyFrames.Add(new EasingThicknessKeyFrame(new(3, -3, 0, 0), KeyTime.Uniform, se));
            horz.KeyFrames.Add(new EasingThicknessKeyFrame(new(3, 0, 0, 0), KeyTime.Uniform, se));
            horz.KeyFrames.Add(new EasingThicknessKeyFrame(new(-3, -3, 0, 0), KeyTime.Uniform, se));
            horz.KeyFrames.Add(new EasingThicknessKeyFrame(new(-3, 3, 0, 0), KeyTime.Uniform, se));
            horz.KeyFrames.Add(new EasingThicknessKeyFrame(new(0, -3, 0, 0), KeyTime.Uniform, se));
            horz.KeyFrames.Add(new EasingThicknessKeyFrame(new(-3, 0, 0, 0), KeyTime.Uniform, se));
            horz.KeyFrames.Add(new EasingThicknessKeyFrame(new(3, 3, 0, 0), KeyTime.Uniform, se));
            horz.KeyFrames.Add(new EasingThicknessKeyFrame(new(0, -3, 0, 0), KeyTime.Uniform, se));
            horz.KeyFrames.Add(new EasingThicknessKeyFrame(new(0, 0, 0, 0), KeyTime.Uniform, se));
            Ship.BeginAnimation(MarginProperty, horz);
            DoubleAnimationUsingKeyFrames flicker = new()
            {
                Duration = TimeSpan.FromSeconds(0.3),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            flicker.KeyFrames.Add(new EasingDoubleKeyFrame(0.3));
            flicker.KeyFrames.Add(new EasingDoubleKeyFrame(0));
            Flame.BeginAnimation(OpacityProperty, flicker);
        }
        private void Website_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            Process.Start("OpenWith.exe", Site);
        }
        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AllowClose) Close();
        }
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !fadeOutPlayed;
            fadeOutPlayed = true;
            DoubleAnimation fadeout = new()
            {
                Duration = TimeSpan.FromSeconds(0.3),
                From = 1,
                To = 0
            };
            DoubleAnimation scaleout = new()
            {
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseIn },
                From = 1,
                To = 0.9
            };
            BeginAnimation(OpacityProperty, fadeout);
            GridTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleout);
            GridTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleout);
            await Task.Delay(300);
            Close();
        }
    }
}