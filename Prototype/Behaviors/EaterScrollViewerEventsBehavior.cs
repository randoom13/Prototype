using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace Prototype.Main.Behaviors
{
    internal class EaterScrollViewerEventsBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseWheel += ScrollViewerPreviewMouseWheel;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
                AssociatedObject.PreviewMouseWheel -= ScrollViewerPreviewMouseWheel;
        }
        private void ScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

    }
}
