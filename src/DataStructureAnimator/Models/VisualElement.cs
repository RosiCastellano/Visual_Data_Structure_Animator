using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DataStructureAnimator.Models;

/// <summary>
/// Base class for all visual elements in the data structure animator
/// </summary>
public partial class VisualElement : ObservableObject
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private double _width = 60;

    [ObservableProperty]
    private double _height = 60;

    [ObservableProperty]
    private double _opacity = 1.0;

    [ObservableProperty]
    private double _scale = 1.0;

    [ObservableProperty]
    private IBrush _fill = Brushes.DodgerBlue;

    [ObservableProperty]
    private IBrush _stroke = Brushes.White;

    [ObservableProperty]
    private double _strokeThickness = 2;

    [ObservableProperty]
    private bool _isHighlighted;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string _label = string.Empty;

    public Point Position => new(X, Y);
    public Point Center => new(X + Width / 2, Y + Height / 2);
}

/// <summary>
/// Represents a node element (for linked lists, trees, etc.)
/// </summary>
public partial class NodeElement : VisualElement
{
    [ObservableProperty]
    private object? _value;

    [ObservableProperty]
    private int _index = -1;

    [ObservableProperty]
    private NodeElement? _next;

    [ObservableProperty]
    private NodeElement? _previous;

    [ObservableProperty]
    private NodeElement? _left;

    [ObservableProperty]
    private NodeElement? _right;

    [ObservableProperty]
    private NodeElement? _parent;

    public NodeElement()
    {
        Width = 60;
        Height = 60;
    }

    public NodeElement(object? value) : this()
    {
        Value = value;
        Label = value?.ToString() ?? "";
    }
}

/// <summary>
/// Represents an arrow/connection between nodes
/// </summary>
public partial class ConnectionElement : VisualElement
{
    [ObservableProperty]
    private Point _startPoint;

    [ObservableProperty]
    private Point _endPoint;

    [ObservableProperty]
    private bool _isBidirectional;

    [ObservableProperty]
    private bool _isCurved;

    [ObservableProperty]
    private NodeElement? _sourceNode;

    [ObservableProperty]
    private NodeElement? _targetNode;

    public ConnectionElement()
    {
        Fill = Brushes.Gray;
        Stroke = Brushes.Gray;
    }
}

/// <summary>
/// Represents an array cell
/// </summary>
public partial class ArrayCellElement : VisualElement
{
    [ObservableProperty]
    private object? _value;

    [ObservableProperty]
    private int _arrayIndex;

    [ObservableProperty]
    private bool _isBeingCompared;

    [ObservableProperty]
    private bool _isBeingSwapped;

    [ObservableProperty]
    private bool _isSorted;

    public ArrayCellElement()
    {
        Width = 50;
        Height = 50;
    }

    public ArrayCellElement(object? value, int index) : this()
    {
        Value = value;
        ArrayIndex = index;
        Label = value?.ToString() ?? "";
    }
}

/// <summary>
/// Represents a hash table bucket
/// </summary>
public partial class HashBucketElement : VisualElement
{
    [ObservableProperty]
    private int _bucketIndex;

    [ObservableProperty]
    private List<NodeElement> _chain = new();

    [ObservableProperty]
    private bool _hasCollision;

    public HashBucketElement(int index)
    {
        BucketIndex = index;
        Width = 80;
        Height = 40;
    }
}
