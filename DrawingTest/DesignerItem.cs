using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingTest
{
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public class DesignerItem : ContentControl
    {
        public static readonly DependencyProperty DragThumbTemplateProperty =
            DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public DesignerItem()
        {
            this.Loaded += new RoutedEventHandler(DesignerItem_Loaded);
        }

        public static ControlTemplate GetDragThumbTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(DragThumbTemplateProperty);
        }
        void DesignerItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (Template == null) return;
            ContentPresenter? contentPresenter =
                Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
            if (contentPresenter == null) return;
            UIElement? contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
            if (contentVisual == null) return;
            DragThumb? thumb = Template.FindName("PART_DragThumb", this) as DragThumb;

            if (thumb == null) return;
            ControlTemplate template = GetDragThumbTemplate(contentVisual);
            if (template == null) return; // use default rectangle
            thumb.Template = template;
        }
    }
}
