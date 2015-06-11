using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using PerfectCamera.Filters.FilterControls;
using PerfectCamera.Resources;
using System.Windows.Controls;
using System.Diagnostics;

namespace PerfectCamera.Filters.Lomo
{
    public class LomoWrapperFilter: AbstractFilter
    {
        protected LomoFilter _filter;
        protected String _lomoVignettingGroup = "CarShowLomoVignetting";

        public LomoWrapperFilter()
            : base()
        {
            
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _filter };
        }

        public override bool AttachControl(FilterPropertiesControl control)
        {
            Control = control;

            Grid grid = new Grid();
            int rowIndex = 0;

            TextBlock brightnessText = new TextBlock { Text = AppResources.Brightness };
            Grid.SetRow(brightnessText, rowIndex++);

            Slider brightnessSlider = new Slider { Minimum = 0.0, Maximum = 1.0, Value = _filter.Brightness };
            brightnessSlider.ValueChanged += brightnessSlider_ValueChanged;
            Grid.SetRow(brightnessSlider, rowIndex++);

            TextBlock saturationText = new TextBlock { Text = AppResources.Saturation };
            Grid.SetRow(saturationText, rowIndex++);

            Slider saturationSlider = new Slider { Minimum = 0.0, Maximum = 1.0, Value = _filter.Saturation };
            saturationSlider.ValueChanged += saturationSlider_ValueChanged;
            Grid.SetRow(saturationSlider, rowIndex++);

            TextBlock lomoVignettingText = new TextBlock { Text = AppResources.LomoVignetting };
            Grid.SetRow(lomoVignettingText, rowIndex++);

            RadioButton highRadioButton = new RadioButton { GroupName = _lomoVignettingGroup };
            TextBlock textBlock = new TextBlock { Text = AppResources.High };
            highRadioButton.Content = textBlock;
            highRadioButton.Checked += highRadioButton_Checked;
            Grid.SetRow(highRadioButton, rowIndex++);

            RadioButton medRadioButton = new RadioButton { GroupName = _lomoVignettingGroup };
            textBlock = new TextBlock { Text = AppResources.Medium };
            medRadioButton.Content = textBlock;
            medRadioButton.Checked += medRadioButton_Checked;
            Grid.SetRow(medRadioButton, rowIndex++);

            RadioButton lowRadioButton = new RadioButton { GroupName = _lomoVignettingGroup };
            textBlock = new TextBlock { Text = AppResources.Low };
            lowRadioButton.Content = textBlock;
            lowRadioButton.Checked += lowRadioButton_Checked;
            Grid.SetRow(lowRadioButton, rowIndex++);

            switch (_filter.LomoVignetting)
            {
                case LomoVignetting.Low: lowRadioButton.IsChecked = true; break;
                case LomoVignetting.Medium: medRadioButton.IsChecked = true; break;
                case LomoVignetting.High: highRadioButton.IsChecked = true; break;
            }

            for (int i = 0; i < rowIndex; ++i)
            {
                RowDefinition rd = new RowDefinition();
                grid.RowDefinitions.Add(rd);
            }

            grid.Children.Add(brightnessText);
            grid.Children.Add(brightnessSlider);
            grid.Children.Add(saturationText);
            grid.Children.Add(saturationSlider);
            grid.Children.Add(lomoVignettingText);
            grid.Children.Add(lowRadioButton);
            grid.Children.Add(medRadioButton);
            grid.Children.Add(highRadioButton);

            control.ControlsContainer.Children.Add(grid);

            return true;
        }

        protected void brightnessSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.WriteLine("Changing brightness to " + (1.0 - e.NewValue));
            Changes.Add(() => { _filter.Brightness = 1.0 - e.NewValue; });
            Apply();
            Control.NotifyManipulated();
        }

        protected void saturationSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.WriteLine("Changing saturation changed to " + e.NewValue);
            Changes.Add(() => { _filter.Saturation = e.NewValue; });
            Apply();
            Control.NotifyManipulated();
        }

        protected void lowRadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Changes.Add(() => { _filter.LomoVignetting = LomoVignetting.Low; });
            Apply();
            Control.NotifyManipulated();
        }

        protected void medRadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Changes.Add(() => { _filter.LomoVignetting = LomoVignetting.Medium; });
            Apply();
            Control.NotifyManipulated();
        }

        protected void highRadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Changes.Add(() => { _filter.LomoVignetting = LomoVignetting.High; });
            Apply();
            Control.NotifyManipulated();
        }
    }

    public class LomoNeutralFilter: LomoWrapperFilter
    {
        public LomoNeutralFilter(): base()
        {
            Name = "Neutral";
            ShortDescription = "Neutral";

            _filter = new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Neutral);
        }
    }

    public class LomoRedFilter: LomoWrapperFilter
    {
        public LomoRedFilter(): base()
        {
            Name = "Red";
            ShortDescription = "Red";

            _filter = new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Red);
        }
    }

    public class LomoGreenFilter : LomoWrapperFilter
    {
        public LomoGreenFilter(): base()
        {
            Name = "Green";
            ShortDescription = "Green";

            _filter = new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Green);
        }
    }

    public class LomoBlueFilter : LomoWrapperFilter
    {
        public LomoBlueFilter()
            : base()
        {
            Name = "Blue";
            ShortDescription = "Blue";

            _filter = new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Blue);
        }
    }

    public class LomoYellowFilter : LomoWrapperFilter
    {
        public LomoYellowFilter(): base()
        {
            Name = "Yellow";
            ShortDescription = "Yellow";

            _filter = new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Yellow);
        }
    }
}
