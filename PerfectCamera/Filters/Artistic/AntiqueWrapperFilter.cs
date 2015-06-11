using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectCamera.Filters.Artistic
{
    class AntiqueWrapperFilter: AbstractFilter
    {
        private AntiqueFilter _filter;
        public AntiqueWrapperFilter() : base ()
        {
            Name = "Antique";
            ShortDescription = "Antique";

            _filter = new AntiqueFilter();
        }

        protected override void SetFilters(Lumia.Imaging.FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _filter };
        }
    }
}
