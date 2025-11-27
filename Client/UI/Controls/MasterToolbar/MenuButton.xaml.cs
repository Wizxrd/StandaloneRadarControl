using Client.UI.Displays.Tactical.MasterToolbar.VideoMaps;
using Client.UI.Displays.Tactical.ToolbarControl;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Client.UI.Controls.MasterToolbar
{
    public partial class MenuButton : UserControl
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        public static bool DeleteModeEnabled { get; set; }

        private static FrameworkElement tearOffInstance;
        private static bool hasCloned;

        private bool isTearOffInstance;
        private bool isDragging;

        private Point dragStartPointHost;
        private Point skeletonStartTopLeft;

        private TranslateTransform controlTransform;
        private TranslateTransform skeletonTransform;

        private Rectangle dragSkeleton;
        private Panel hostPanel;

        private FrameworkElement originalTargetElement;
        private FrameworkElement dragTargetElement;

        public static readonly DependencyProperty MenuButtonLine1Property =
            DependencyProperty.Register(
                nameof(MenuButtonLine1),
                typeof(string),
                typeof(MenuButton),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MenuButtonLine2Property =
            DependencyProperty.Register(
                nameof(MenuButtonLine2),
                typeof(string),
                typeof(MenuButton),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(MenuButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DragTargetProperty =
            DependencyProperty.Register(
                nameof(DragTarget),
                typeof(FrameworkElement),
                typeof(MenuButton),
                new PropertyMetadata(null));

        public string MenuButtonLine1
        {
            get => (string)GetValue(MenuButtonLine1Property);
            set => SetValue(MenuButtonLine1Property, value);
        }

        public string MenuButtonLine2
        {
            get => (string)GetValue(MenuButtonLine2Property);
            set => SetValue(MenuButtonLine2Property, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public FrameworkElement DragTarget
        {
            get => (FrameworkElement)GetValue(DragTargetProperty);
            set => SetValue(DragTargetProperty, value);
        }

        private bool IsMoveOnlyTarget => DragTarget is ToolbarControlView;

        public MenuButton()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            PreviewMouseLeftButtonDown += OnMouseDown;
            PreviewMouseMove += OnMouseMove;
            PreviewMouseLeftButtonUp += OnMouseUp;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (hostPanel == null)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                    hostPanel = window.FindName("RootGrid") as Panel;
            }

            if (hostPanel == null)
                return;

            if (DragTarget == null)
            {
                dragTargetElement = this;
                EnsureTransform(dragTargetElement);
            }

            if (dragSkeleton != null)
                return;

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

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (hostPanel == null) return;

            var originalSource = e.OriginalSource as DependencyObject;
            var hitOnHandle = IsFromDragHandle(originalSource);

            if (DeleteModeEnabled &&
                tearOffInstance != null &&
                !hitOnHandle &&
                IsInsideTearOff(this))
            {
                var window = Window.GetWindow(this) ?? Application.Current.MainWindow;

                hostPanel.Children.Remove(tearOffInstance);

                tearOffInstance = null;
                hasCloned = false;

                if (window != null)
                {
                    foreach (var mb in FindVisualChildren<MenuButton>(window))
                    {
                        if (!mb.isTearOffInstance && mb.DragHandle != null)
                            mb.DragHandle.IsEnabled = true;
                    }
                }

                e.Handled = true;
                return;
            }

            if (DeleteModeEnabled && hitOnHandle)
            {
                e.Handled = true;
                return;
            }

            if (DeleteModeEnabled)
            {
                e.Handled = true;
                return;
            }

            if (!hitOnHandle)
                return;

            originalTargetElement = DragTarget ?? (FrameworkElement)this;

            if (DragTarget != null)
            {
                if (IsMoveOnlyTarget)
                {
                    dragTargetElement = DragTarget;
                    EnsureTransform(dragTargetElement);
                }
                else if (hasCloned && tearOffInstance != null)
                {
                    dragTargetElement = tearOffInstance;
                    EnsureTransform(dragTargetElement);
                }
                else
                {
                    dragTargetElement = originalTargetElement;
                    if (DragTarget == null)
                        EnsureTransform(dragTargetElement);
                }
            }
            else
            {
                dragTargetElement = originalTargetElement;
                EnsureTransform(dragTargetElement);
            }

            isDragging = true;
            CaptureMouse();

            var positionSource =
                IsMoveOnlyTarget && DragTarget != null
                    ? DragTarget
                    : (DragTarget != null && hasCloned && tearOffInstance != null
                        ? tearOffInstance
                        : originalTargetElement);

            var controlTopLeft = positionSource
                .TransformToAncestor(hostPanel)
                .Transform(new Point(0, 0));

            dragSkeleton.Width = 105;
            dragSkeleton.Height = 40;

            skeletonStartTopLeft = controlTopLeft;
            skeletonTransform.X = skeletonStartTopLeft.X;
            skeletonTransform.Y = skeletonStartTopLeft.Y;

            dragSkeleton.Visibility = Visibility.Visible;

            dragStartPointHost = skeletonStartTopLeft;

            var screenTopLeft = positionSource.PointToScreen(new Point(0, 0));
            SetCursorPos((int)screenTopLeft.X, (int)screenTopLeft.Y);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;
            if (hostPanel == null) return;

            var current = e.GetPosition(hostPanel);
            var offset = current - dragStartPointHost;

            var desiredLeft = skeletonStartTopLeft.X + offset.X;
            var desiredTop = skeletonStartTopLeft.Y + offset.Y;

            var maxLeft = hostPanel.ActualWidth - dragSkeleton.Width;
            var maxTop = hostPanel.ActualHeight - dragSkeleton.Height;

            if (maxLeft < 0) maxLeft = 0;
            if (maxTop < 0) maxTop = 0;

            var newLeft = Math.Max(0, Math.Min(desiredLeft, maxLeft));
            var newTop = Math.Max(0, Math.Min(desiredTop, maxTop));

            skeletonTransform.X = newLeft;
            skeletonTransform.Y = newTop;

            var clampedX = !desiredLeft.Equals(newLeft);
            var clampedY = !desiredTop.Equals(newTop);

            if (!(clampedX || clampedY))
                return;

            var borderCursorHost = new Point(
                dragStartPointHost.X + (newLeft - skeletonStartTopLeft.X),
                dragStartPointHost.Y + (newTop - skeletonStartTopLeft.Y));

            dragStartPointHost = borderCursorHost;
            skeletonStartTopLeft = new Point(newLeft, newTop);

            var cursorScreen = hostPanel.PointToScreen(borderCursorHost);
            SetCursorPos((int)cursorScreen.X, (int)cursorScreen.Y);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isDragging) return;
            if (hostPanel == null) return;

            isDragging = false;
            ReleaseMouseCapture();

            var finalLeft = skeletonTransform.X;
            var finalTop = skeletonTransform.Y;

            if (DragTarget != null)
            {
                if (IsMoveOnlyTarget)
                {
                    dragTargetElement = DragTarget;
                    EnsureTransform(dragTargetElement);

                    var currentTopLeft = dragTargetElement
                        .TransformToAncestor(hostPanel)
                        .Transform(new Point(0, 0));

                    var deltaX = finalLeft - currentTopLeft.X;
                    var deltaY = finalTop - currentTopLeft.Y;

                    controlTransform.X += deltaX;
                    controlTransform.Y += deltaY;
                }
                else
                {
                    if (!hasCloned)
                    {
                        var source = originalTargetElement ?? (DragTarget ?? (FrameworkElement)this);

                        var clone = Activator.CreateInstance(source.GetType()) as FrameworkElement;
                        if (clone != null)
                        {
                            if (source is VideoMapsView)
                            {
                                clone.DataContext = new VideoMapsViewModel
                                {
                                    BorderVisibility = Visibility.Collapsed
                                };
                            }
                            else
                            {
                                var sourceVm = source.DataContext;
                                if (sourceVm is ICloneable cloneableVm)
                                    clone.DataContext = cloneableVm.Clone();
                                else
                                    clone.DataContext = sourceVm;
                            }

                            clone.Width = source.ActualWidth;
                            clone.Height = 84;

                            clone.Margin = new Thickness(0);
                            clone.HorizontalAlignment = HorizontalAlignment.Left;
                            clone.VerticalAlignment = VerticalAlignment.Top;

                            hostPanel.Children.Add(clone);
                            Panel.SetZIndex(clone, int.MaxValue - 1);

                            dragTargetElement = clone;
                            EnsureTransform(dragTargetElement);

                            controlTransform.X = finalLeft;
                            controlTransform.Y = finalTop;

                            tearOffInstance = clone;
                            hasCloned = true;

                            foreach (var mb in FindVisualChildren<MenuButton>(clone))
                            {
                                mb.isTearOffInstance = true;

                                if (mb.DragHandle != null)
                                    mb.DragHandle.IsEnabled = false;
                            }

                            if (DragHandle != null)
                                DragHandle.IsEnabled = false;
                        }
                    }
                    else
                    {
                        dragTargetElement = tearOffInstance ?? (FrameworkElement)this;
                        EnsureTransform(dragTargetElement);

                        var currentTopLeft = dragTargetElement
                            .TransformToAncestor(hostPanel)
                            .Transform(new Point(0, 0));

                        var deltaX = finalLeft - currentTopLeft.X;
                        var deltaY = finalTop - currentTopLeft.Y;

                        controlTransform.X += deltaX;
                        controlTransform.Y += deltaY;
                    }
                }
            }
            else
            {
                if (dragTargetElement == null)
                    dragTargetElement = (FrameworkElement)this;

                EnsureTransform(dragTargetElement);

                var currentTopLeft = dragTargetElement
                    .TransformToAncestor(hostPanel)
                    .Transform(new Point(0, 0));

                var deltaX = finalLeft - currentTopLeft.X;
                var deltaY = finalTop - currentTopLeft.Y;

                controlTransform.X += deltaX;
                controlTransform.Y += deltaY;
            }

            dragSkeleton.Visibility = Visibility.Collapsed;
        }

        private void EnsureTransform(FrameworkElement element)
        {
            if (element.RenderTransform is TranslateTransform tt)
                controlTransform = tt;
            else
            {
                controlTransform = new TranslateTransform();
                element.RenderTransform = controlTransform;
            }
        }

        private bool IsInsideTearOff(DependencyObject element)
        {
            if (tearOffInstance == null || element == null)
                return false;

            var current = element;
            while (current != null)
            {
                if (ReferenceEquals(current, tearOffInstance))
                    return true;

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private bool IsFromDragHandle(DependencyObject? source)
        {
            while (source != null)
            {
                if (ReferenceEquals(source, DragHandle))
                    return true;

                source = source is Visual or System.Windows.Media.Media3D.Visual3D
                    ? VisualTreeHelper.GetParent(source)
                    : LogicalTreeHelper.GetParent(source);
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
    }
}
