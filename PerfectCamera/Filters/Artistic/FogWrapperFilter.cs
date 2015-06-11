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
    class FogWrapperFilter: AbstractFilter
    {
        private FogFilter _filter;
        public FogWrapperFilter(): base ()
        {
            Name = "Fog";
            ShortDescription = "Fog";
            _filter = new FogFilter();
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _filter };
        }
    }
}
