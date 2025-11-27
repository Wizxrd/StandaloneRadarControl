using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Client.UI.Displays.Tactical.CommandComposition;

public partial class CommandCompositionView : UserControl
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    private bool isDragging;
    private Point dragStartPointHost;
    private Point skeletonStartTopLeft;
    private TranslateTransform controlTransform;
    private TranslateTransform skeletonTransform;
    private Rectangle dragSkeleton;
    private Panel hostPanel;

    public CommandCompositionView()
    {
        InitializeComponent();
        Loaded += CommandCompositionView_Loaded;
        PreviewMouseLeftButtonDown += CommandCompositionView_MouseDown;
        PreviewMouseMove += CommandCompositionView_MouseMove;
        PreviewMouseLeftButtonUp += CommandCompositionView_MouseUp;
    }

    private void CommandCompositionView_Loaded(object sender, RoutedEventArgs e)
    {
        hostPanel = VisualTreeHelper.GetParent(this) as Panel;
        if (hostPanel == null) return;

        if (RenderTransform is TranslateTransform tt)
            controlTransform = tt;
        else
        {
            controlTransform = new TranslateTransform();
            RenderTransform = controlTransform;
        }

        if (dragSkeleton == null)
        {
            dragSkeleton = new Rectangle
            {
                Stroke = Brushes.White,
                StrokeThickness = 1,
                Fill = Brushes.Transparent,
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false
            };

            skeletonTransform = new TranslateTransform();
            dragSkeleton.RenderTransform = skeletonTransform;

            hostPanel.Children.Add(dragSkeleton);
            Panel.SetZIndex(dragSkeleton, int.MaxValue);
        }
    }

    private void CommandCompositionView_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (hostPanel == null) return;

        isDragging = true;
        CaptureMouse();

        Point controlTopLeft = TransformToAncestor(hostPanel).Transform(new Point(0, 0));

        dragSkeleton.Width = ActualWidth;
        dragSkeleton.Height = ActualHeight;

        skeletonStartTopLeft = controlTopLeft;
        skeletonTransform.X = skeletonStartTopLeft.X;
        skeletonTransform.Y = skeletonStartTopLeft.Y;

        dragSkeleton.Visibility = Visibility.Visible;

        dragStartPointHost = skeletonStartTopLeft;

        Point screenTopLeft = PointToScreen(new Point(0, 0));
        SetCursorPos((int)screenTopLeft.X, (int)screenTopLeft.Y);
    }

    private void CommandCompositionView_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isDragging) return;
        if (hostPanel == null) return;

        Point current = e.GetPosition(hostPanel);
        Vector offset = current - dragStartPointHost;

        double desiredLeft = skeletonStartTopLeft.X + offset.X;
        double desiredTop = skeletonStartTopLeft.Y + offset.Y;

        double maxLeft = hostPanel.ActualWidth - dragSkeleton.Width;
        double maxTop = hostPanel.ActualHeight - dragSkeleton.Height;

        if (maxLeft < 0) maxLeft = 0;
        if (maxTop < 0) maxTop = 0;

        double newLeft = Math.Max(0, Math.Min(desiredLeft, maxLeft));
        double newTop = Math.Max(0, Math.Min(desiredTop, maxTop));

        skeletonTransform.X = newLeft;
        skeletonTransform.Y = newTop;

        bool clampedX = !desiredLeft.Equals(newLeft);
        bool clampedY = !desiredTop.Equals(newTop);

        if (clampedX || clampedY)
        {
            Point borderCursorHost = new Point(
                dragStartPointHost.X + (newLeft - skeletonStartTopLeft.X),
                dragStartPointHost.Y + (newTop - skeletonStartTopLeft.Y));

            dragStartPointHost = borderCursorHost;
            skeletonStartTopLeft = new Point(newLeft, newTop);

            Point cursorScreen = hostPanel.PointToScreen(borderCursorHost);
            SetCursorPos((int)cursorScreen.X, (int)cursorScreen.Y);
        }
    }

    private void CommandCompositionView_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!isDragging) return;
        if (hostPanel == null) return;

        isDragging = false;
        ReleaseMouseCapture();

        double finalLeft = skeletonTransform.X;
        double finalTop = skeletonTransform.Y;

        Point currentControlTopLeft = TransformToAncestor(hostPanel).Transform(new Point(0, 0));

        double deltaX = finalLeft - currentControlTopLeft.X;
        double deltaY = finalTop - currentControlTopLeft.Y;

        controlTransform.X += deltaX;
        controlTransform.Y += deltaY;

        dragSkeleton.Visibility = Visibility.Collapsed;
    }
}
