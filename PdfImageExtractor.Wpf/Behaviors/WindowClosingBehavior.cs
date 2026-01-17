using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace PdfImageExtractor.Wpf.Behaviors
{
    public class WindowClosingBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Closing += OnClosing;
            AssociatedObject.StateChanged += OnStateChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Closing -= OnClosing;
            AssociatedObject.StateChanged -= OnStateChanged;
            base.OnDetaching();
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            // Instead of closing, hide to tray
            e.Cancel = true;
            AssociatedObject.Hide();
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            if (AssociatedObject.WindowState == WindowState.Minimized)
            {
                AssociatedObject.Hide();
            }
        }
    }
}
