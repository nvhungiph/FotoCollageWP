using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using PerfectCamera.Filters.FilterControls;
using PerfectCamera.Resources;

namespace PerfectCamera.Filters.Artistic
{
    class GrayscaleNegativeWrapperFilter: AbstractFilter
    {
        private GrayscaleNegativeFilter _filter;
        public GrayscaleNegativeWrapperFilter(): base ()
        {
            Name = "Grayscale Negative";
            ShortDescription = "Grayscale Negative";

            _filter = new GrayscaleNegativeFilter();
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _filter };    
        }
    }
}
