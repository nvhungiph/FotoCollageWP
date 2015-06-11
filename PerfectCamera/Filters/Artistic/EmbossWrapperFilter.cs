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
    class EmbossWrapperFilter: AbstractFilter
    {
        private EmbossFilter _filter;
        public EmbossWrapperFilter(): base ()
        {
            Name = "Emboss";
            ShortDescription = "Emboss";
            _filter = new EmbossFilter(0.5);
        }

        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _filter };
        }

        public override bool AttachControl(FilterPropertiesControl control)
        {
            Control = control;

            var grid = new Grid();
            int rowIndex = 0;

            TextBlock levelText = new TextBlock()
            {
                Text = "Level"
            };
            Grid.SetRow(levelText, rowIndex++);

            Slider levelSlider = new Slider() { Minimum = 0.0, Maximum = 1.0, Value = _filter.Level};
            levelSlider.ValueChanged += levelSlider_ValueChanged;
            Grid.SetRow(levelSlider, rowIndex++);

            for (int i = 0; i < rowIndex; ++i)
            {
                RowDefinition rd = new RowDefinition();
                grid.RowDefinitions.Add(rd);
            }

            grid.Children.Add(levelText);
            grid.Children.Add(levelSlider);

            control.ControlsContainer.Children.Add(grid);

            return true;
        }

        void levelSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Changes.Add(() => { _filter.Level = e.NewValue; });
            Apply();
            Control.NotifyManipulated();
        }
    }
}
