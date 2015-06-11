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
using Lumia.Imaging.Custom;
using Windows.Storage.Streams;
using Windows.Foundation;
using System.Windows.Media.Imaging;

namespace PerfectCamera.Filters.Retro
{
    public class VintageFilter: AbstractFilter
    {
        protected LevelsFilter _levelFilter;
        protected CurvesFilter _curvesFilter;
        protected VignettingFilter _vignettingFilter;
        protected HueSaturationFilter _hueSaturationFilter;

        public VintageFilter(): base()
        {
            Name = "Vintage";
            ShortDescription = "Vintage";

            _levelFilter = new LevelsFilter(0.9, 0.6, 0.0);

            _curvesFilter = new CurvesFilter();
            Curve red = new Curve();
            red.SetPoint(127, 100);
            red.SetPoint(235, 255);

            Curve green = new Curve();
            green.SetPoint(97, 72);
            green.SetPoint(177, 189);

            Curve blue = new Curve();
            blue.SetPoint(0, 34);
            blue.SetPoint(255, 220);
            
            _curvesFilter.Red = red;
            _curvesFilter.Green = green;
            _curvesFilter.Blue = blue;

            _vignettingFilter = new VignettingFilter(0.5, new Windows.UI.Color { R = 104, G = 103, B = 71 });
            //_vignettingFilter = new VignettingFilter(0.3, new Windows.UI.Color { R = 255, G = 0, B = 0 });

            _hueSaturationFilter = new HueSaturationFilter(5.0 / 255.0, -40.0 / 255.0);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _levelFilter, _curvesFilter, _vignettingFilter, _hueSaturationFilter };
        }
    }

    public class RetroPurpleFilter: AbstractFilter
    {
        protected LevelsFilter _levelFilter;
        protected CurvesFilter _curvesFilter;
        protected VignettingFilter _vignettingFilter;
        protected HueSaturationFilter _hueSaturationFilter;
        protected ColorAdjustFilter _colorAdjustFilter;

        public RetroPurpleFilter()
            : base()
        {
            Name = "Purple";
            ShortDescription = "Purple";

            _levelFilter = new LevelsFilter(0.9, 0.6, 0.0);

            _curvesFilter = new CurvesFilter();
            Curve red = new Curve();
            red.SetPoint(127, 100);
            red.SetPoint(235, 255);

            Curve green = new Curve();
            green.SetPoint(97, 72);
            green.SetPoint(177, 189);

            Curve blue = new Curve();
            blue.SetPoint(0, 34);
            blue.SetPoint(255, 220);
            
            _curvesFilter.Red = red;
            _curvesFilter.Green = green;
            _curvesFilter.Blue = blue;

            _vignettingFilter = new VignettingFilter(0.5, new Windows.UI.Color { R = 104, G = 103, B = 71 });
            //_vignettingFilter = new VignettingFilter(0.3, new Windows.UI.Color { R = 255, G = 0, B = 0 });

            _hueSaturationFilter = new HueSaturationFilter(5.0 / 255.0, -40.0 / 255.0);

            _colorAdjustFilter = new ColorAdjustFilter(35.0 / 255.0, -31.0 / 255.0, 39.0 / 255.0);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _levelFilter, _curvesFilter, _vignettingFilter, _hueSaturationFilter, _colorAdjustFilter };
        }
    }

    public class RetroBloomFilter : AbstractFilter
    {
        protected LevelsFilter _levelFilter;
        protected CurvesFilter _curvesFilter;
        protected VignettingFilter _vignettingFilter;
        protected HueSaturationFilter _hueSaturationFilter;
        protected ColorAdjustFilter _colorAdjustFilter;

        public RetroBloomFilter()
            : base()
        {
            Name = "Bloom";
            ShortDescription = "Bloom";

            _levelFilter = new LevelsFilter(0.9, 0.6, 0.0);

            _curvesFilter = new CurvesFilter();
            Curve red = new Curve();
            red.SetPoint(127, 100);
            red.SetPoint(235, 255);

            Curve green = new Curve();
            green.SetPoint(97, 72);
            green.SetPoint(177, 189);

            Curve blue = new Curve();
            blue.SetPoint(0, 34);
            blue.SetPoint(255, 220);

            _curvesFilter.Red = red;
            _curvesFilter.Green = green;
            _curvesFilter.Blue = blue;

            _vignettingFilter = new VignettingFilter(0.5, new Windows.UI.Color { R = 104, G = 103, B = 71 });
            //_vignettingFilter = new VignettingFilter(0.3, new Windows.UI.Color { R = 255, G = 0, B = 0 });

            _hueSaturationFilter = new HueSaturationFilter(5.0 / 255.0, -40.0 / 255.0);
            _colorAdjustFilter = new ColorAdjustFilter(45.0 / 255.0, -41.0 / 255.0, -89.0 / 255.0);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _levelFilter, _curvesFilter, _vignettingFilter, _hueSaturationFilter, _colorAdjustFilter };
        }
    }

    public class RetroLightFilter : AbstractFilter
    {
        protected LevelsFilter _levelFilter;
        protected CurvesFilter _curvesFilter;
        protected VignettingFilter _vignettingFilter;
        protected HueSaturationFilter _hueSaturationFilter;
        protected ColorAdjustFilter _colorAdjustFilter;

        public RetroLightFilter()
            : base()
        {
            Name = "Light";
            ShortDescription = "Light";

            _levelFilter = new LevelsFilter(0.9, 0.6, 0.0);

            _curvesFilter = new CurvesFilter();
            Curve red = new Curve();
            red.SetPoint(127, 100);
            red.SetPoint(235, 255);

            Curve green = new Curve();
            green.SetPoint(97, 72);
            green.SetPoint(177, 189);

            Curve blue = new Curve();
            blue.SetPoint(0, 34);
            blue.SetPoint(255, 220);

            _curvesFilter.Red = red;
            _curvesFilter.Green = green;
            _curvesFilter.Blue = blue;

            _vignettingFilter = new VignettingFilter(0.5, new Windows.UI.Color { R = 104, G = 103, B = 71 });
            //_vignettingFilter = new VignettingFilter(0.3, new Windows.UI.Color { R = 255, G = 0, B = 0 });

            _hueSaturationFilter = new HueSaturationFilter(5.0 / 255.0, -40.0 / 255.0);
            _colorAdjustFilter = new ColorAdjustFilter(-13.0 / 255.0, -15.0 / 255.0, 17.0 / 255.0);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _levelFilter, _curvesFilter, _vignettingFilter, _hueSaturationFilter, _colorAdjustFilter };
        }
    }

    public class RetroGoldenFilter : AbstractFilter
    {
        protected LevelsFilter _levelFilter;
        protected CurvesFilter _curvesFilter;
        protected VignettingFilter _vignettingFilter;
        protected HueSaturationFilter _hueSaturationFilter;
        protected ColorAdjustFilter _colorAdjustFilter;

        public RetroGoldenFilter()
            : base()
        {
            Name = "Golden";
            ShortDescription = "Golden";

            _levelFilter = new LevelsFilter(0.9, 0.6, 0.0);

            _curvesFilter = new CurvesFilter();
            Curve red = new Curve();
            red.SetPoint(127, 100);
            red.SetPoint(235, 255);

            Curve green = new Curve();
            green.SetPoint(97, 72);
            green.SetPoint(177, 189);

            Curve blue = new Curve();
            blue.SetPoint(0, 34);
            blue.SetPoint(255, 220);

            _curvesFilter.Red = red;
            _curvesFilter.Green = green;
            _curvesFilter.Blue = blue;

            _vignettingFilter = new VignettingFilter(0.5, new Windows.UI.Color { R = 104, G = 103, B = 71 });
            //_vignettingFilter = new VignettingFilter(0.3, new Windows.UI.Color { R = 255, G = 0, B = 0 });

            _hueSaturationFilter = new HueSaturationFilter(5.0 / 255.0, -40.0 / 255.0);
            _colorAdjustFilter = new ColorAdjustFilter(11.0 / 255.0, -41.0 / 255.0, -100.0 / 255.0);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _levelFilter, _curvesFilter, _vignettingFilter, _hueSaturationFilter, _colorAdjustFilter };
        }
    }

    public class RetroCleanFilter : AbstractFilter
    {
        protected LevelsFilter _levelFilter;
        protected CurvesFilter _curvesFilter;
        protected VignettingFilter _vignettingFilter;
        protected HueSaturationFilter _hueSaturationFilter;
        protected ColorAdjustFilter _colorAdjustFilter;

        public RetroCleanFilter()
            : base()
        {
            Name = "Clean";
            ShortDescription = "Clean";

            _levelFilter = new LevelsFilter(0.9, 0.6, 0.0);

            _curvesFilter = new CurvesFilter();
            Curve red = new Curve();
            red.SetPoint(127, 100);
            red.SetPoint(235, 255);

            Curve green = new Curve();
            green.SetPoint(97, 72);
            green.SetPoint(177, 189);

            Curve blue = new Curve();
            blue.SetPoint(0, 34);
            blue.SetPoint(255, 220);

            _curvesFilter.Red = red;
            _curvesFilter.Green = green;
            _curvesFilter.Blue = blue;

            _vignettingFilter = new VignettingFilter(0.5, new Windows.UI.Color { R = 104, G = 103, B = 71 });
            //_vignettingFilter = new VignettingFilter(0.3, new Windows.UI.Color { R = 255, G = 0, B = 0 });

            _hueSaturationFilter = new HueSaturationFilter(5.0 / 255.0, -40.0 / 255.0);
            _colorAdjustFilter = new ColorAdjustFilter(35.0 / 255.0, -31.0 / 255.0, 39.0 / 255.0);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _levelFilter, _curvesFilter, _vignettingFilter, _hueSaturationFilter, _colorAdjustFilter };
        }
    }

    public class RetroFadedFilter : AbstractFilter
    {
        protected LevelsFilter _levelFilter;
        protected CurvesFilter _curvesFilter;
        protected VignettingFilter _vignettingFilter;
        protected HueSaturationFilter _hueSaturationFilter;
        protected ColorAdjustFilter _colorAdjustFilter;

        public RetroFadedFilter()
            : base()
        {
            Name = "Faded";
            ShortDescription = "Faded";

            _levelFilter = new LevelsFilter(0.9, 0.6, 0.0);

            _curvesFilter = new CurvesFilter();
            Curve red = new Curve();
            red.SetPoint(127, 100);
            red.SetPoint(235, 255);

            Curve green = new Curve();
            green.SetPoint(97, 72);
            green.SetPoint(177, 189);

            Curve blue = new Curve();
            blue.SetPoint(0, 34);
            blue.SetPoint(255, 220);

            _curvesFilter.Red = red;
            _curvesFilter.Green = green;
            _curvesFilter.Blue = blue;

            _vignettingFilter = new VignettingFilter(0.5, new Windows.UI.Color { R = 104, G = 103, B = 71 });
            //_vignettingFilter = new VignettingFilter(0.3, new Windows.UI.Color { R = 255, G = 0, B = 0 });

            _hueSaturationFilter = new HueSaturationFilter(5.0 / 255.0, -40.0 / 255.0);
            _colorAdjustFilter = new ColorAdjustFilter(0.0 / 255.0, -20.0 / 255.0, -63.0 / 255.0);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _levelFilter, _curvesFilter, _vignettingFilter, _hueSaturationFilter, _colorAdjustFilter };
        }
    }

    public class RetroEmeraldFilter : AbstractFilter
    {
        protected LevelsFilter _levelFilter;
        protected CurvesFilter _curvesFilter;
        protected VignettingFilter _vignettingFilter;
        protected HueSaturationFilter _hueSaturationFilter;
        protected ColorAdjustFilter _colorAdjustFilter;

        public RetroEmeraldFilter()
            : base()
        {
            Name = "Emerald";
            ShortDescription = "Emerald";

            _levelFilter = new LevelsFilter(0.9, 0.6, 0.0);

            _curvesFilter = new CurvesFilter();
            Curve red = new Curve();
            red.SetPoint(127, 100);
            red.SetPoint(235, 255);

            Curve green = new Curve();
            green.SetPoint(97, 72);
            green.SetPoint(177, 189);

            Curve blue = new Curve();
            blue.SetPoint(0, 34);
            blue.SetPoint(255, 220);

            _curvesFilter.Red = red;
            _curvesFilter.Green = green;
            _curvesFilter.Blue = blue;

            _vignettingFilter = new VignettingFilter(0.5, new Windows.UI.Color { R = 104, G = 103, B = 71 });
            //_vignettingFilter = new VignettingFilter(0.3, new Windows.UI.Color { R = 255, G = 0, B = 0 });

            _hueSaturationFilter = new HueSaturationFilter(5.0 / 255.0, -40.0 / 255.0);
            _colorAdjustFilter = new ColorAdjustFilter(22.0 / 255.0, 13.0 / 255.0, -24.0 / 255.0);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _levelFilter, _curvesFilter, _vignettingFilter, _hueSaturationFilter, _colorAdjustFilter };
        }
    }

    public class RetroDiffuseFilter : AbstractFilter
    {
        protected LevelsFilter _levelFilter;
        protected CurvesFilter _curvesFilter;
        protected VignettingFilter _vignettingFilter;
        protected HueSaturationFilter _hueSaturationFilter;
        protected ColorAdjustFilter _colorAdjustFilter;

        public RetroDiffuseFilter()
            : base()
        {
            Name = "Diffuse";
            ShortDescription = "Diffuse";

            _levelFilter = new LevelsFilter(0.9, 0.6, 0.0);

            _curvesFilter = new CurvesFilter();
            Curve red = new Curve();
            red.SetPoint(127, 100);
            red.SetPoint(235, 255);

            Curve green = new Curve();
            green.SetPoint(97, 72);
            green.SetPoint(177, 189);

            Curve blue = new Curve();
            blue.SetPoint(0, 34);
            blue.SetPoint(255, 220);

            _curvesFilter.Red = red;
            _curvesFilter.Green = green;
            _curvesFilter.Blue = blue;

            _vignettingFilter = new VignettingFilter(0.5, new Windows.UI.Color { R = 104, G = 103, B = 71 });
            //_vignettingFilter = new VignettingFilter(0.3, new Windows.UI.Color { R = 255, G = 0, B = 0 });

            _hueSaturationFilter = new HueSaturationFilter(5.0 / 255.0, -40.0 / 255.0);
            _colorAdjustFilter = new ColorAdjustFilter(-23.0 / 255.0, 13.0 / 255.0, -16.0 / 255.0);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _levelFilter, _curvesFilter, _vignettingFilter, _hueSaturationFilter, _colorAdjustFilter };
        }
    }
}
