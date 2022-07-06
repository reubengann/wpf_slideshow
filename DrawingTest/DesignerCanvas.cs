using System.Collections.Generic;
using System.Windows.Controls;

namespace DrawingTest
{
    public class DesignerCanvas : Canvas
    {
        private List<ISelectable> selectedItmes;

        public List<ISelectable> SelectedItems
        {
            get { return selectedItmes; }
            set { selectedItmes = value; }
        }


        public DesignerCanvas()
        {
            selectedItmes = new List<ISelectable>();
        }
    }
}
