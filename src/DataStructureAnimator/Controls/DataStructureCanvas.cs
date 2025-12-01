using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DataStructureAnimator.DataStructures;
using DataStructureAnimator.Models;

namespace DataStructureAnimator.Controls;

/// <summary>
/// Custom canvas control for rendering data structures
/// </summary>
public class DataStructureCanvas : Control
{
    public static readonly StyledProperty<IAnimatableDataStructure?> DataStructureProperty =
        AvaloniaProperty.Register<DataStructureCanvas, IAnimatableDataStructure?>(nameof(DataStructure));

    public IAnimatableDataStructure? DataStructure
    {
        get => GetValue(DataStructureProperty);
        set => SetValue(DataStructureProperty, value);
    }

    private IAnimatableDataStructure? _subscribedStructure;

    static DataStructureCanvas()
    {
        AffectsRender<DataStructureCanvas>(DataStructureProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DataStructureProperty)
        {
            // Unsubscribe from old structure
            if (_subscribedStructure != null)
            {
                _subscribedStructure.VisualsChanged -= OnVisualsChanged;
            }

            // Subscribe to new structure
            _subscribedStructure = DataStructure;
            if (_subscribedStructure != null)
            {
                _subscribedStructure.VisualsChanged += OnVisualsChanged;
            }

            InvalidateVisual();
        }
    }

    private void OnVisualsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(InvalidateVisual);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Draw background
        context.DrawRectangle(
            new SolidColorBrush(Color.Parse("#1E1E2E")),
            null,
            new Rect(0, 0, Bounds.Width, Bounds.Height));

        if (DataStructure == null) return;

        // Draw connections first (behind nodes)
        foreach (var connection in DataStructure.GetConnections())
        {
            DrawConnection(context, connection);
        }

        // Draw elements
        foreach (var element in DataStructure.GetVisualElements())
        {
            DrawElement(context, element);
        }
    }

    private void DrawElement(DrawingContext context, VisualElement element)
    {
        if (element.Opacity <= 0) return;

        var rect = new Rect(element.X, element.Y, element.Width * element.Scale, element.Height * element.Scale);

        // Apply scaling (center origin)
        if (element.Scale != 1.0)
        {
            var centerX = element.X + element.Width / 2;
            var centerY = element.Y + element.Height / 2;
            var scaledWidth = element.Width * element.Scale;
            var scaledHeight = element.Height * element.Scale;
            rect = new Rect(
                centerX - scaledWidth / 2,
                centerY - scaledHeight / 2,
                scaledWidth,
                scaledHeight);
        }

        // Create opacity brush
        var fillBrush = element.Fill;
        var strokeBrush = element.Stroke;

        if (element.Opacity < 1.0 && fillBrush is SolidColorBrush solidFill)
        {
            var color = solidFill.Color;
            fillBrush = new SolidColorBrush(Color.FromArgb((byte)(color.A * element.Opacity), color.R, color.G, color.B));
        }

        // Highlight effect
        if (element.IsHighlighted)
        {
            var glowRect = rect.Inflate(5);
            context.DrawRectangle(
                new SolidColorBrush(Color.FromArgb(100, 255, 215, 0)),
                null,
                glowRect,
                8, 8);
        }

        // Draw based on element type
        if (element is NodeElement || element is ArrayCellElement)
        {
            // Draw as rounded rectangle
            context.DrawRectangle(fillBrush, new Pen(strokeBrush, element.StrokeThickness), rect, 8, 8);
        }
        else if (element is HashBucketElement)
        {
            // Draw as rectangle with arrow indicator
            context.DrawRectangle(fillBrush, new Pen(strokeBrush, element.StrokeThickness), rect, 4, 4);
        }
        else
        {
            context.DrawRectangle(fillBrush, new Pen(strokeBrush, element.StrokeThickness), rect, 4, 4);
        }

        // Draw label
        if (!string.IsNullOrEmpty(element.Label))
        {
            var formattedText = new FormattedText(
                element.Label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter", FontStyle.Normal, FontWeight.Bold),
                14,
                Brushes.White);

            var textX = rect.X + (rect.Width - formattedText.Width) / 2;
            var textY = rect.Y + (rect.Height - formattedText.Height) / 2;

            context.DrawText(formattedText, new Point(textX, textY));
        }
    }

    private void DrawConnection(DrawingContext context, ConnectionElement connection)
    {
        var pen = new Pen(connection.Stroke ?? Brushes.Gray, 2);

        var start = connection.StartPoint;
        var end = connection.EndPoint;

        // Update points from source/target nodes if available
        if (connection.SourceNode != null)
        {
            start = new Point(
                connection.SourceNode.X + connection.SourceNode.Width,
                connection.SourceNode.Y + connection.SourceNode.Height / 2);
        }

        if (connection.TargetNode != null)
        {
            end = new Point(
                connection.TargetNode.X,
                connection.TargetNode.Y + connection.TargetNode.Height / 2);
        }

        // Draw line
        context.DrawLine(pen, start, end);

        // Draw arrowhead
        DrawArrowhead(context, start, end, pen.Brush);

        // Draw label if present
        if (!string.IsNullOrEmpty(connection.Label))
        {
            var midX = (start.X + end.X) / 2;
            var midY = (start.Y + end.Y) / 2 - 10;

            var formattedText = new FormattedText(
                connection.Label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter"),
                12,
                Brushes.LightGray);

            context.DrawText(formattedText, new Point(midX - formattedText.Width / 2, midY));
        }
    }

    private void DrawArrowhead(DrawingContext context, Point start, Point end, IBrush? brush)
    {
        var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
        var arrowLength = 10;
        var arrowAngle = Math.PI / 6;

        var p1 = new Point(
            end.X - arrowLength * Math.Cos(angle - arrowAngle),
            end.Y - arrowLength * Math.Sin(angle - arrowAngle));

        var p2 = new Point(
            end.X - arrowLength * Math.Cos(angle + arrowAngle),
            end.Y - arrowLength * Math.Sin(angle + arrowAngle));

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(end, true);
            ctx.LineTo(p1);
            ctx.LineTo(p2);
            ctx.EndFigure(true);
        }

        context.DrawGeometry(brush, null, geometry);
    }
}
