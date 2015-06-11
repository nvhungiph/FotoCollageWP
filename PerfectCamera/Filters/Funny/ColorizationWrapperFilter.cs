using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Transforms;
using PerfectCamera.Filters.FilterControls;
using PerfectCamera.Resources;
using System.Windows.Controls;
using System.Diagnostics;

namespace PerfectCamera.Filters.Funny
{
    class OrangeWrapperFilter: AbstractFilter
    {
        protected StampFilter _stampFilter;
        protected ColorSwapFilter _colorSwapFilter;
        public OrangeWrapperFilter()
            : base()
        {
            Name = "Orange";
            ShortDescription = "Orange";

            _stampFilter = new StampFilter(3, 0.3);
            _colorSwapFilter = new ColorSwapFilter(Windows.UI.Color.FromArgb(255, 255, 255, 255), Windows.UI.Color.FromArgb(255, 255, 127, 0), 0.1, false, false);

        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _stampFilter, _colorSwapFilter };
        }
    }

    class RoseWrapperFilter : AbstractFilter
    {
        protected StampFilter _stampFilter;
        protected ColorSwapFilter _colorSwapFilter;
        public RoseWrapperFilter()
            : base()
        {
            Name = "Rose";
            ShortDescription = "Rose";

            _stampFilter = new StampFilter(3, 0.3);
            _colorSwapFilter = new ColorSwapFilter(Windows.UI.Color.FromArgb(255, 255, 255, 255), Windows.UI.Color.FromArgb(255, 255, 0, 127), 0.1, false, false);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _stampFilter, _colorSwapFilter };
        }
    }

    class GreenWrapperFilter : AbstractFilter
    {
        protected StampFilter _stampFilter;
        protected ColorSwapFilter _colorSwapFilter;
        public GreenWrapperFilter()
            : base()
        {
            Name = "Green";
            ShortDescription = "Green";

            _stampFilter = new StampFilter(3, 0.3);
            _colorSwapFilter = new ColorSwapFilter(Windows.UI.Color.FromArgb(255, 255, 255, 255), Windows.UI.Color.FromArgb(255, 0, 255, 0), 0.1, false, false);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _stampFilter, _colorSwapFilter };
        }
    }

    class BlueWrapperFilter : AbstractFilter
    {
        protected StampFilter _stampFilter;
        protected ColorSwapFilter _colorSwapFilter;
        public BlueWrapperFilter()
            : base()
        {
            Name = "Blue";
            ShortDescription = "Blue";

            _stampFilter = new StampFilter(3, 0.3);
            _colorSwapFilter = new ColorSwapFilter(Windows.UI.Color.FromArgb(255, 255, 255, 255), Windows.UI.Color.FromArgb(255, 0, 0, 255), 0.1, false, false);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _stampFilter, _colorSwapFilter };
        }
    }
}
