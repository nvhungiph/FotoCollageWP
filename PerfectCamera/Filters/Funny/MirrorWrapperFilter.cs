using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Transforms;
using PerfectCamera.Filters.FilterControls;
using PerfectCamera.Resources;
using System.Windows.Controls;
using System.Diagnostics;

namespace PerfectCamera.Filters.Funny
{
    public class MirrorLeftFilter: AbstractFilter
    {
        protected MirrorFilter _filter;
        public MirrorLeftFilter()
            : base()
        {
            Name = "L-Mirror";
            ShortDescription = "L-Mirror";

            _filter = new MirrorFilter();
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _filter };
        }
    }

    public class MirrorRightFilter: AbstractFilter
    {
        protected FlipFilter _flipFilter;
        protected MirrorFilter _mirrorFilter;

        public MirrorRightFilter(): base()
        {
            Name = "R-Mirror";
            ShortDescription = "R-Mirror";

            _flipFilter = new FlipFilter(FlipMode.Horizontal);
            _mirrorFilter = new MirrorFilter();
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _flipFilter, _mirrorFilter };
        }
    }

    public class MirrorAboveFilter: AbstractFilter
    {
        protected RotationFilter _rotateLeftFilter;
        protected MirrorFilter _mirrorFilter;
        protected RotationFilter _rotateRightFilter;

        public MirrorAboveFilter(): base()
        {
            Name = "Above";
            ShortDescription = "Above";

            _rotateLeftFilter = new RotationFilter(-90);
            _mirrorFilter = new MirrorFilter();
            _rotateRightFilter = new RotationFilter(90);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _rotateLeftFilter, _mirrorFilter, _rotateRightFilter };
        }
    }

    public class MirrorBelowFilter : AbstractFilter
    {
        protected RotationFilter _rotateLeftFilter;
        protected MirrorFilter _mirrorFilter;
        protected RotationFilter _rotateRightFilter;

        public MirrorBelowFilter()
            : base()
        {
            Name = "Below";
            ShortDescription = "Below";

            _rotateLeftFilter = new RotationFilter(90);
            _mirrorFilter = new MirrorFilter();
            _rotateRightFilter = new RotationFilter(-90);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _rotateLeftFilter, _mirrorFilter, _rotateRightFilter };
        }
    }

}
