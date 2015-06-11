using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using PerfectCamera.Filters.FilterControls;
using PerfectCamera.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PerfectCamera.Filters.Artistic
{
    class ColorSwapWrapperFilter: AbstractFilter
    {
        private ColorSwapFilter _filter;
        private Windows.UI.Color _sourceColor;
        private Windows.UI.Color _swapColor;

        public ColorSwapWrapperFilter(): base()
        {
            Name = "Color Swap";
            ShortDescription = "Color Swap";

            _sourceColor = new Windows.UI.Color()
            {
                A = 255,
                R = 255,
                G = 0,
                B = 0
            };

            _swapColor = new Windows.UI.Color()
            {
                A = 255,
                R = 255,
                G = 0,
                B = 0
            };

            _filter = new ColorSwapFilter(_sourceColor, _swapColor, 0.08, false, false);
        }

        protected override void SetFilters(Lumia.Imaging.FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _filter};
        }

        public override bool AttachControl(FilterControls.FilterPropertiesControl control)
        {
            Control = control;

            var colorSwapControl = new ColorSwapFilterControl();
            control.ControlsContainer.Children.Add(colorSwapControl);

            colorSwapControl.DidChangeSourceColor = colorSwapControl_DidChangeSourceColor;
            colorSwapControl.DidChangeSwapColor = colorSwapControl_DidChangeSwapColor;
            colorSwapControl.DidCheckMonoColor = colorSwapControl_DidCheckMonoColor;
            colorSwapControl.DidCheckSwapLuminance = colorSwapControl_DidCheckSwapLuminance;
            colorSwapControl.DidChangeColorDistance = colorSwapControl_DidChangeColorDistance;

            return true;
        }

        protected void colorSwapControl_DidChangeSourceColor(Windows.UI.Color sourceColor)
        {
            Changes.Add(() => { _filter.SourceColor = sourceColor; });
            Apply();
            Control.NotifyManipulated();
        }

        protected void colorSwapControl_DidChangeSwapColor(Windows.UI.Color swapColor)
        {
            Changes.Add(() => { _filter.SwapColor = swapColor; });
            Apply();
            Control.NotifyManipulated();
        }

        protected void colorSwapControl_DidCheckMonoColor(bool check)
        {
            Changes.Add(() => { _filter.IsMonoColor = check; });
            Apply();
            Control.NotifyManipulated();
        }

        protected void colorSwapControl_DidCheckSwapLuminance(bool check)
        {
            Changes.Add(() => { _filter.SwapLuminance = check; });
            Apply();
            Control.NotifyManipulated();
        }

        protected void colorSwapControl_DidChangeColorDistance(double distance)
        {
            Changes.Add(() => { _filter.ColorDistance = distance; });
            Apply();
            Control.NotifyManipulated();
        }
    }
}
