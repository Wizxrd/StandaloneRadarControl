using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Client.Models;

namespace Client.Controls
{
    public partial class Styles : ResourceDictionary
    {
        private void RenameTextBoxFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ContentControl contentControl)
            {
                var textBox = FindChild<TextBox>(contentControl);
                if (textBox != null)
                {
                    textBox.CaretIndex = textBox.Text.Length;
                    textBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        textBox.SelectAll();
                    }));
                }
            }
        }

        private static Type FindChild<Type>(DependencyObject parent) where Type : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Type typedChild)
                {
                    return typedChild;
                }

                var result = FindChild<Type>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static string GetContentControl(ToggleButton toggleButton)
        {
            if (toggleButton.Content is Grid grid)
            {
                foreach (UIElement element in grid.Children)
                {
                    if (element is ContentControl contentControl)
                    {
                        return contentControl.Content?.ToString() ?? "No Content";
                    }
                }
            }
            return string.Empty;
        }
    }
}
