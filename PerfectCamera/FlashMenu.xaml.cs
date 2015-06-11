using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Controls.Primitives;
using Windows.Phone.Media.Capture;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PerfectCamera
{
    public partial class FlashMenu : UserControl
    {
        public Action<FlashState> FlashStateChanged { get; set; }
        FlashState _flashState = FlashState.Auto;
        public FlashMenu()
        {
            InitializeComponent();

            LayoutRoot.Width = Application.Current.Host.Content.ActualWidth;
            LayoutRoot.Height = Application.Current.Host.Content.ActualHeight;

            LayoutRoot.MouseLeftButtonDown += LayoutRoot_MouseLeftButtonDown;
        }

        void LayoutRoot_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);
            var transform = ContentLayout.TransformToVisual(LayoutRoot);
            var origin = transform.Transform(new Point(0, 0));
            Rect r = new Rect(origin.X, origin.Y, ContentLayout.Width, ContentLayout.Height);
            if (!r.Contains(pos))
            {
                var p = this.Parent as Popup;
                if (p != null)
                {
                    p.Child = null;
                    p.IsOpen = false;
                }
            }
        }

        public void SetCurrentFlashMode(FlashState mode)
        {
            _flashState = mode;
            switch (mode)
            {
                case FlashState.Auto:
                    {
                        ModeAuto();
                        break;
                    }
                case FlashState.On:
                    {
                        ModeOn();
                        break;
                    }
                case FlashState.Off:
                    {
                        ModeOff();
                        break;
                    }
            }
        }

        private void FlashAutoButton_Click(object sender, RoutedEventArgs e)
        {
            _flashState = FlashState.Auto;
            ModeAuto();
            if (FlashStateChanged != null)
            {
                FlashStateChanged(_flashState);
            }
        }

        private void FlashOnButton_Click(object sender, RoutedEventArgs e)
        {
            _flashState = FlashState.On;
            ModeOn();
            if (FlashStateChanged != null)
            {
                FlashStateChanged(_flashState);
            }
        }

        private void FlashOffButton_Click(object sender, RoutedEventArgs e)
        {
            _flashState = FlashState.Off;
            ModeOff();
            if (FlashStateChanged != null)
            {
                FlashStateChanged(_flashState);
            }
        }

        private void ModeAuto()
        {
            FlashAutoImage.Source = new BitmapImage(new Uri("/Assets/FlashAutoOn.png", UriKind.Relative));
            FlashAutoButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 249, 210, 100));

            FlashOnImage.Source = new BitmapImage(new Uri("/Assets/FlashOnOff.png", UriKind.Relative));
            FlashOnButton.Foreground = new SolidColorBrush(Colors.White);

            FlashOffImage.Source = new BitmapImage(new Uri("/Assets/FlashOffOff.png", UriKind.Relative));
            FlashOffButton.Foreground = new SolidColorBrush(Colors.White);
        }

        private void ModeOn()
        {
            FlashAutoImage.Source = new BitmapImage(new Uri("/Assets/FlashAutoOff.png", UriKind.Relative));
            FlashAutoButton.Foreground = new SolidColorBrush(Colors.White);

            FlashOnImage.Source = new BitmapImage(new Uri("/Assets/FlashOnOn.png", UriKind.Relative));
            FlashOnButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 249, 210, 100));

            FlashOffImage.Source = new BitmapImage(new Uri("/Assets/FlashOffOff.png", UriKind.Relative));
            FlashOffButton.Foreground = new SolidColorBrush(Colors.White);
        }

        private void ModeOff()
        {
            FlashAutoImage.Source = new BitmapImage(new Uri("/Assets/FlashAutoOff.png", UriKind.Relative));
            FlashAutoButton.Foreground = new SolidColorBrush(Colors.White);

            FlashOnImage.Source = new BitmapImage(new Uri("/Assets/FlashOnOff.png", UriKind.Relative));
            FlashOnButton.Foreground = new SolidColorBrush(Colors.White);

            FlashOffImage.Source = new BitmapImage(new Uri("/Assets/FlashOffOn.png", UriKind.Relative));
            FlashOffButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 249, 210, 100));
        }
    }
}
