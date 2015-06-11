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

namespace PerfectCamera
{
    public partial class SettingMenu : UserControl
    {
        public Action NavigateSettingPage { get; set; }
        public Action<int> TimerChanged { get; set; }
        public Action<CameraRatio> RatioChanged { get; set; }

        int _timeOut = 0;
        CameraRatio _ratio = CameraRatio.Ratio_16x9;

        public SettingMenu()
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
            var origin = transform.Transform(new Point(0,0));
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

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            var p = this.Parent as Popup;
            if (p != null)
            {
                p.Child = null;
                p.IsOpen = false;
            }

            if (NavigateSettingPage != null)
            {
                NavigateSettingPage();
            }
        }

        private void TimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (TimerChanged != null)
            {

            }
        }

        public void SetRatio(CameraRatio ratio)
        {
            _ratio = ratio;
            if (_ratio == CameraRatio.Ratio_16x9)
            {
                RatioTextBlock.Text = "16:9";
            }
            else
            {
                RatioTextBlock.Text = "4:3";
            }
        }

        private void RatioButton_Click(object sender, RoutedEventArgs e)
        {
            if (RatioChanged != null)
            {
                if (_ratio == CameraRatio.Ratio_16x9)
                {
                    _ratio = CameraRatio.Ratio_4x3;
                    RatioTextBlock.Text = "4:3";
                }
                else
                {
                    _ratio = CameraRatio.Ratio_16x9;
                    RatioTextBlock.Text = "16:9";
                }

                RatioChanged(_ratio);
            }
        }
    }
}
