using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace DrawingTest
{
    public class DesignerCanvas : Canvas
    {
        private List<ISelectable> selectedItems;

        public List<ISelectable> SelectedItems
        {
            get { return selectedItems; }
            set { selectedItems = value; }
        }


        public DesignerCanvas()
        {
            selectedItems = new List<ISelectable>();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Source == this)
            {
                foreach (ISelectable item in SelectedItems)
                    item.IsSelected = false;
                selectedItems.Clear();

                e.Handled = true;
            }
        }
    }
}
