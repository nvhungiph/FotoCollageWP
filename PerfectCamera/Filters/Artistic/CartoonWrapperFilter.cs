using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using PerfectCamera.Filters.FilterControls;
using PerfectCamera.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PerfectCamera.Filters.Artistic
{
    class CartoonWrapperFilter: AbstractFilter
    {
        private CartoonFilter _cartoonFilter;
        public CartoonWrapperFilter(): base ()
        {
            Name = "Cartoon";
            ShortDescription = "Cartoon";

            _cartoonFilter = new CartoonFilter();
        }


        protected override void SetFilters(FilterEffect effect)
        {
            effect.Filters = new List<IFilter>() { _cartoonFilter };
        }

        public override bool AttachControl(FilterPropertiesControl control)
        {
            Control = control;

            Grid grid = new Grid();
            int rowIndex = 0;

            CheckBox distinctEdgesCheckBox = new CheckBox();
            TextBlock textBlock = new TextBlock { Text = AppResources.DistinctEdges };
            distinctEdgesCheckBox.Content = textBlock;
            distinctEdgesCheckBox.IsChecked = _cartoonFilter.DistinctEdges;
            distinctEdgesCheckBox.Checked += distinctEdgesCheckBox_Checked;
            distinctEdgesCheckBox.Unchecked += distinctEdgesCheckBox_Unchecked;
            Grid.SetRow(distinctEdgesCheckBox, rowIndex++);

            for (int i = 0; i < rowIndex; ++i)
            {
                RowDefinition rd = new RowDefinition();
                grid.RowDefinitions.Add(rd);
            }

            grid.Children.Add(distinctEdgesCheckBox);

            control.ControlsContainer.Children.Add(grid);

            return true;
        }

        void distinctEdgesCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Changes.Add(() => { _cartoonFilter.DistinctEdges = true; });
            Apply();
            Control.NotifyManipulated();
        }

        void distinctEdgesCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Changes.Add(() => { _cartoonFilter.DistinctEdges = false; });
            Apply();
            Control.NotifyManipulated();
        }
    }
}
