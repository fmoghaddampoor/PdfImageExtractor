using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace PdfImageExtractor.Wpf.Behaviors
{
    public class WindowDragBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                AssociatedObject.DragMove();
            }
        }
    }
}
