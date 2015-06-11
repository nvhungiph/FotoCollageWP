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

namespace PerfectCamera.Filters.Artistic
{
    class MilkyWrapperFilter: AbstractFilter
    {
        private MilkyFilter _filter;

        public MilkyWrapperFilter(): base()
        {
            Name = "Milky";
            ShortDescription = "Milky";

            _filter = new MilkyFilter();
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _filter };
        }
    }
}
