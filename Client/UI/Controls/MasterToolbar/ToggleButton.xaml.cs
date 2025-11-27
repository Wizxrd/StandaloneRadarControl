using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Common.Utils;

namespace Client.UI.Controls.MasterToolbar
{
    public partial class ToggleButton : UserControl
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        public static bool DeleteModeEnabled { get; set; }

        private static readonly Dictionary<string, ToggleButton> TearOffMap = new();

        private bool isTearOffInstance;
        private bool isDragging;

        private Panel hostPanel;
        private Rectangle dragSkeleton;

        private TranslateTransform controlTransform;
        private TranslateTransform skeletonTransform;

        private FrameworkElement dragTargetElement;
        private Point dragStartPointHost;
        private Point skeletonStartTopLeft;

        public static readonly DependencyProperty CloneGroupProperty =
            DependencyProperty.Register(
                nameof(CloneGroup),
                typeof(string),
                typeof(ToggleButton));

        public string CloneGroup
        {
            get => (string)GetValue(CloneGroupProperty);
            set => SetValue(CloneGroupProperty, value);
        }

        internal string CloneKey =>
            string.IsNullOrEmpty(CloneGroup)
                ? (ToggleButtonLine1 ?? string.Empty) + "|" + (ToggleButtonLine2 ?? string.Empty)
                : CloneGroup;

        public static readonly DependencyProperty ToggleButtonLine1Property =
            DependencyProperty.Register(
                nameof(ToggleButtonLine1),
                typeof(string),
                typeof(ToggleButton));

        public static readonly DependencyProperty ToggleButtonLine2Property =
            DependencyProperty.Register(
                nameof(ToggleButtonLine2),
                typeof(string),
                typeof(ToggleButton));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(ToggleButton));

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(ToggleButton),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string ToggleButtonLine1
        {
            get => (string)GetValue(ToggleButtonLine1Property);
            set => SetValue(ToggleButtonLine1Property, value);
        }

        public string ToggleButtonLine2
        {
            get => (string)GetValue(ToggleButtonLine2Property);
            set => SetValue(ToggleButtonLine2Property, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty DragTargetProperty =
            DependencyProperty.Register(
                nameof(DragTarget),
                typeof(FrameworkElement),
                typeof(ToggleButton));

        public FrameworkElement DragTarget
        {
            get => (FrameworkElement)GetValue(DragTargetProperty);
            set => SetValue(DragTargetProperty, value);
        }

        public ToggleButton()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            PreviewMouseLeftButtonDown += OnMouseDown;
            PreviewMouseMove += OnMouseMove;
            PreviewMouseLeftButtonUp += OnMouseUp;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            hostPanel = window?.FindName("RootGrid") as Panel;
            if (hostPanel == null) return;

            dragTargetElement = this;

            controlTransform = dragTargetElement.RenderTransform as TranslateTransform
                               ?? new TranslateTransform();

            dragTargetElement.RenderTransform = controlTransform;

            if (dragSkeleton == null)
            {
                dragSkeleton = new Rectangle
                {
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent,
                    Visibility = Visibility.Collapsed,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    IsHitTestVisible = false,
                    RadiusX = 3,
                    RadiusY = 3
                };

                skeletonTransform = new TranslateTransform();
                dragSkeleton.RenderTransform = skeletonTransform;

                hostPanel.Children.Add(dragSkeleton);
                Panel.SetZIndex(dragSkeleton, int.MaxValue);
            }

            if (TearOffMap.TryGetValue(CloneKey, out _))
            {
                if (!isTearOffInstance && DragHandle != null)
                    DragHandle.IsEnabled = false;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton != MouseButtonState.Pressed) return;
                if (hostPanel == null) return;

                var src = e.OriginalSource as DependencyObject;
                var hitHandle = IsHitOnDragHandle(src);

                if (DeleteModeEnabled && isTearOffInstance && !hitHandle)
                {
                    DeleteTearOff();
                    e.Handled = true;
                    return;
                }

                if (DeleteModeEnabled && hitHandle)
                {
                    e.Handled = true;
                    return;
                }

                if (DeleteModeEnabled)
                {
                    e.Handled = true;
                    return;
                }

                if (!hitHandle)
                    return;

                if (TearOffMap.TryGetValue(CloneKey, out var tearOff))
                {
                    dragTargetElement = tearOff;
                    controlTransform = tearOff.RenderTransform as TranslateTransform
                                       ?? new TranslateTransform();
                    tearOff.RenderTransform = controlTransform;
                }
                else
                {
                    dragTargetElement = this;
                }

                isDragging = true;
                CaptureMouse();

                if (dragSkeleton != null)
                    Panel.SetZIndex(dragSkeleton, int.MaxValue);

                var start = dragTargetElement.TransformToAncestor(hostPanel)
                                             .Transform(new Point(0, 0));

                skeletonStartTopLeft = start;
                skeletonTransform.X = start.X;
                skeletonTransform.Y = start.Y;

                dragSkeleton.Width = dragTargetElement.ActualWidth;
                dragSkeleton.Height = dragTargetElement.ActualHeight;
                dragSkeleton.Visibility = Visibility.Visible;

                dragStartPointHost = start;

                var screenPos = dragTargetElement.PointToScreen(new Point(0, 0));
                SetCursorPos((int)screenPos.X, (int)screenPos.Y);
            }
            catch (Exception ex)
            {
                Logger.Error("ToggleButton.OnMouseDown", ex.ToString());
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;
            if (hostPanel == null) return;

            var current = e.GetPosition(hostPanel);
            var offset = current - dragStartPointHost;

            var desiredLeft = skeletonStartTopLeft.X + offset.X;
            var desiredTop = skeletonStartTopLeft.Y + offset.Y;

            var clamped = ClampToHost(desiredLeft, desiredTop, dragSkeleton ?? dragTargetElement);
            var newLeft = clamped.X;
            var newTop = clamped.Y;

            skeletonTransform.X = newLeft;
            skeletonTransform.Y = newTop;

            var clampedX = !desiredLeft.Equals(newLeft);
            var clampedY = !desiredTop.Equals(newTop);

            if (clampedX || clampedY)
            {
                var borderCursorHost = new Point(
                    dragStartPointHost.X + (newLeft - skeletonStartTopLeft.X),
                    dragStartPointHost.Y + (newTop - skeletonStartTopLeft.Y));

                dragStartPointHost = borderCursorHost;
                skeletonStartTopLeft = new Point(newLeft, newTop);

                var cursorScreen = hostPanel.PointToScreen(borderCursorHost);
                SetCursorPos((int)cursorScreen.X, (int)cursorScreen.Y);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isDragging) return;
            if (hostPanel == null) return;

            isDragging = false;
            ReleaseMouseCapture();

            var finalLeft = skeletonTransform.X;
            var finalTop = skeletonTransform.Y;

            if (TearOffMap.TryGetValue(CloneKey, out var tearOff))
            {
                MoveElementTo(tearOff, finalLeft, finalTop);
                dragSkeleton.Visibility = Visibility.Collapsed;
                return;
            }

            CreateNewTearOff(finalLeft, finalTop);

            dragSkeleton.Visibility = Visibility.Collapsed;
        }

        private void CreateNewTearOff(double x, double y)
        {
            var clone = new ToggleButton
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                CloneGroup = CloneGroup
            };

            var line1Binding = BindingOperations.GetBinding(this, ToggleButtonLine1Property);
            if (line1Binding != null)
                clone.SetBinding(ToggleButtonLine1Property, line1Binding);
            else
                clone.ToggleButtonLine1 = ToggleButtonLine1;

            var line2Binding = BindingOperations.GetBinding(this, ToggleButtonLine2Property);
            if (line2Binding != null)
                clone.SetBinding(ToggleButtonLine2Property, line2Binding);
            else
                clone.ToggleButtonLine2 = ToggleButtonLine2;

            var commandBinding = BindingOperations.GetBinding(this, CommandProperty);
            if (commandBinding != null)
                clone.SetBinding(CommandProperty, commandBinding);
            else
                clone.Command = Command;

            var isCheckedBinding = BindingOperations.GetBinding(this, IsCheckedProperty);
            if (isCheckedBinding != null)
                clone.SetBinding(IsCheckedProperty, isCheckedBinding);
            else
                clone.IsChecked = IsChecked;

            clone.DataContext = DataContext;

            hostPanel.Children.Add(clone);
            Panel.SetZIndex(clone, int.MaxValue - 1);

            clone.controlTransform = new TranslateTransform();
            clone.RenderTransform = clone.controlTransform;

            clone.controlTransform.X = x;
            clone.controlTransform.Y = y;

            clone.isTearOffInstance = true;

            TearOffMap[CloneKey] = clone;

            DisableOriginalHandles();
        }

        private void MoveElementTo(FrameworkElement element, double x, double y)
        {
            var tt = element.RenderTransform as TranslateTransform
                     ?? new TranslateTransform();

            element.RenderTransform = tt;
            tt.X = x;
            tt.Y = y;
        }

        private void DeleteTearOff()
        {
            if (!isTearOffInstance) return;

            if (Parent is Panel parent)
                parent.Children.Remove(this);

            TearOffMap.Remove(CloneKey);

            EnableOriginalHandles();
        }

        private void DisableOriginalHandles()
        {
            var window = Application.Current.MainWindow;
            if (window == null) return;

            foreach (var t in FindVisualChildren<ToggleButton>(window))
            {
                if (!t.isTearOffInstance && t.CloneKey == CloneKey && t.DragHandle != null)
                    t.DragHandle.IsEnabled = false;
            }
        }

        private void EnableOriginalHandles()
        {
            var window = Application.Current.MainWindow;
            if (window == null) return;

            foreach (var t in FindVisualChildren<ToggleButton>(window))
            {
                if (!t.isTearOffInstance && t.CloneKey == CloneKey && t.DragHandle != null)
                    t.DragHandle.IsEnabled = true;
            }
        }

        internal static void ResetTearOffForKey(string cloneKey)
        {
            if (string.IsNullOrEmpty(cloneKey))
                return;

            TearOffMap.Remove(cloneKey);

            var window = Application.Current.MainWindow;
            if (window == null)
                return;

            foreach (var t in FindVisualChildren<ToggleButton>(window))
            {
                if (!t.isTearOffInstance && t.CloneKey == cloneKey && t.DragHandle != null)
                    t.DragHandle.IsEnabled = true;
            }
        }

        private bool IsHitOnDragHandle(DependencyObject src)
        {
            while (src != null)
            {
                if (ReferenceEquals(src, DragHandle))
                    return true;

                src = src is Visual or System.Windows.Media.Media3D.Visual3D
                    ? VisualTreeHelper.GetParent(src)
                    : LogicalTreeHelper.GetParent(src);
            }

            return false;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
            where T : DependencyObject
        {
            if (parent == null) yield break;

            var count = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    yield return t;

                foreach (var c in FindVisualChildren<T>(child))
                    yield return c;
            }
        }

        private Point ClampToHost(double x, double y, FrameworkElement element)
        {
            if (hostPanel == null || element == null)
                return new Point(x, y);

            var width = dragSkeleton?.Width > 0 ? dragSkeleton.Width : element.ActualWidth;
            var height = dragSkeleton?.Height > 0 ? dragSkeleton.Height : element.ActualHeight;

            var maxX = Math.Max(0, hostPanel.ActualWidth - width);
            var maxY = Math.Max(0, hostPanel.ActualHeight - height);

            x = Math.Max(0, Math.Min(x, maxX));
            y = Math.Max(0, Math.Min(y, maxY));

            return new Point(x, y);
        }
    }
}
