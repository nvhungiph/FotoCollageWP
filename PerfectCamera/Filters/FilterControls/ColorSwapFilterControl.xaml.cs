using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Media;

namespace PerfectCamera.Filters.FilterControls
{
    public partial class ColorSwapFilterControl : UserControl
    {
        public Action<Windows.UI.Color> DidChangeSourceColor;
        public Action<Windows.UI.Color> DidChangeSwapColor;
        public Action<bool> DidCheckMonoColor;
        public Action<bool> DidCheckSwapLuminance;
        public Action<double> DidChangeColorDistance;

        public ColorSwapFilterControl()
        {
            InitializeComponent();

            SourceColorPicker.Color = Colors.Red;
            SwapColorPicker.Color = Colors.Red;
        }

        private void SwapColorPicker_ColorChanged(object sender, System.Windows.Media.Color color)
        {
            if (DidChangeSwapColor != null)
            {
                DidChangeSwapColor(Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }

        private void SourceColorPicker_ColorChanged(object sender, System.Windows.Media.Color color)
        {
            if (DidChangeSourceColor != null)
            {
                DidChangeSourceColor(Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }

        private void MonoColorCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (DidCheckMonoColor != null)
            {
                DidCheckMonoColor(true);
            }
        }

        private void MonoColorCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DidCheckMonoColor != null)
            {
                DidCheckMonoColor(false);
            }
        }

        private void SwapLuminanceCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (DidCheckSwapLuminance != null)
            {
                DidCheckSwapLuminance(true);
            }
        }

        private void SwapLuminanceCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DidCheckSwapLuminance != null)
            {
                DidCheckSwapLuminance(false);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DidChangeColorDistance != null)
            {
                DidChangeColorDistance(e.NewValue);
            }
        }
    }
}
