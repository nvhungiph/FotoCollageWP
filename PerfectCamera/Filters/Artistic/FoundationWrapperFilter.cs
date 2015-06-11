using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using PerfectCamera.Filters;
using PerfectCamera.Filters.FilterControls;
using System.Windows.Controls;

namespace PerfectCamera.Filters.Artistic
{
    class FoundationWrapperFilter: AbstractFilter
    {
        private FoundationFilter _fitler;
        public FoundationWrapperFilter(): base ()
        {
            Name = "Foundation";
            ShortDescription = "Foundation";

            _fitler = new FoundationFilter();
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _fitler };
        }

        public override bool AttachControl(FilterPropertiesControl control)
        {
            Control = control;

            var cropRegionControl = new CropRegionControl()
            {
                Width = 400,
                Height = 500
            };

            cropRegionControl.DidUpdateCropRect = cropRegionControl_DidUpdateCropRect;

            control.ControlsContainer.Children.Add(cropRegionControl);

            return true;
        }

        protected void cropRegionControl_DidUpdateCropRect()
        {
            Control.NotifyManipulated();
        }
    }
}
