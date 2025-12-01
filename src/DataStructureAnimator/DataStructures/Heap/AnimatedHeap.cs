using Avalonia;
using Avalonia.Media;
using DataStructureAnimator.Animations;
using DataStructureAnimator.Models;

namespace DataStructureAnimator.DataStructures.Heap;

public enum HeapType
{
    MinHeap,
    MaxHeap
}

/// <summary>
/// Animated heap (binary heap) data structure
/// </summary>
public class AnimatedHeap : AnimatableDataStructureBase
{
    private readonly List<NodeElement> _nodes = new();
    private HeapType _heapType;

    public HeapType Type => _heapType;
    public int Count => _nodes.Count;
    public IReadOnlyList<NodeElement> Nodes => _nodes;

    public AnimatedHeap(HeapType type = HeapType.MinHeap)
    {
        _heapType = type;
    }

    public void Initialize(IEnumerable<int> values, HeapType type = HeapType.MinHeap)
    {
        Clear();
        _heapType = type;

        foreach (var value in values)
        {
            InsertDirect(value);
        }

        OnVisualsChanged();
    }

    private void InsertDirect(int value)
    {
        var node = new NodeElement(value) { Index = _nodes.Count };
        _nodes.Add(node);
        _elements.Add(node);
        HeapifyUp(_nodes.Count - 1);
        UpdateConnections();
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (ShouldSwap(_nodes[index], _nodes[parent]))
            {
                SwapNodes(index, parent);
                index = parent;
            }
            else
            {
                break;
            }
        }
    }

    private void HeapifyDown(int index)
    {
        int size = _nodes.Count;
        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int target = index;

            if (left < size && ShouldSwap(_nodes[left], _nodes[target]))
                target = left;
            if (right < size && ShouldSwap(_nodes[right], _nodes[target]))
                target = right;

            if (target != index)
            {
                SwapNodes(index, target);
                index = target;
            }
            else
            {
                break;
            }
        }
    }

    private bool ShouldSwap(NodeElement child, NodeElement parent)
    {
        int childVal = (int)child.Value!;
        int parentVal = (int)parent.Value!;

        return _heapType == HeapType.MinHeap
            ? childVal < parentVal
            : childVal > parentVal;
    }

    private void SwapNodes(int i, int j)
    {
        (_nodes[i], _nodes[j]) = (_nodes[j], _nodes[i]);
        _nodes[i].Index = i;
        _nodes[j].Index = j;
    }

    public override void Clear()
    {
        _nodes.Clear();
        base.Clear();
    }

    public override void LayoutElements(double canvasWidth, double canvasHeight)
    {
        if (_nodes.Count == 0) return;

        int levels = (int)Math.Ceiling(Math.Log2(_nodes.Count + 1));
        double nodeSize = 50;
        double verticalSpacing = 80;

        for (int i = 0; i < _nodes.Count; i++)
        {
            int level = (int)Math.Floor(Math.Log2(i + 1));
            int posInLevel = i - (int)Math.Pow(2, level) + 1;
            int nodesInLevel = (int)Math.Pow(2, level);

            double levelWidth = canvasWidth / (nodesInLevel + 1);
            double x = levelWidth * (posInLevel + 1) - nodeSize / 2;
            double y = 50 + level * verticalSpacing;

            _nodes[i].X = x;
            _nodes[i].Y = y;
            _nodes[i].Width = nodeSize;
            _nodes[i].Height = nodeSize;
        }

        UpdateConnections();
        OnVisualsChanged();
    }

    private void UpdateConnections()
    {
        _connections.Clear();

        for (int i = 0; i < _nodes.Count; i++)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < _nodes.Count)
            {
                _connections.Add(new ConnectionElement
                {
                    SourceNode = _nodes[i],
                    TargetNode = _nodes[left],
                    StartPoint = new Point(_nodes[i].X + _nodes[i].Width / 2, _nodes[i].Y + _nodes[i].Height),
                    EndPoint = new Point(_nodes[left].X + _nodes[left].Width / 2, _nodes[left].Y)
                });
            }

            if (right < _nodes.Count)
            {
                _connections.Add(new ConnectionElement
                {
                    SourceNode = _nodes[i],
                    TargetNode = _nodes[right],
                    StartPoint = new Point(_nodes[i].X + _nodes[i].Width / 2, _nodes[i].Y + _nodes[i].Height),
                    EndPoint = new Point(_nodes[right].X + _nodes[right].Width / 2, _nodes[right].Y)
                });
            }
        }
    }

    #region Operations with Animation Steps

    public List<AnimationStep> GenerateInsertSteps(int value)
    {
        var steps = new List<AnimationStep>();
        var newNode = new NodeElement(value) { Index = _nodes.Count };

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Insert,
            Target = newNode,
            Description = $"Inserting {value} at the end of the heap"
        });

        // Simulate insertion
        var tempNodes = _nodes.Select(n => (int)n.Value!).ToList();
        tempNodes.Add(value);
        int index = tempNodes.Count - 1;

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = newNode,
            Description = $"Added {value} at index {index}",
            CustomAction = async () =>
            {
                _nodes.Add(newNode);
                _elements.Add(newNode);
                UpdateConnections();
                OnVisualsChanged();
            }
        });

        // Heapify up with animation
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            bool shouldSwap = _heapType == HeapType.MinHeap
                ? tempNodes[index] < tempNodes[parent]
                : tempNodes[index] > tempNodes[parent];

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Compare,
                Target = _nodes.Count > parent ? _nodes[parent] : null,
                Description = $"Comparing {tempNodes[index]} with parent {tempNodes[parent]}"
            });

            if (shouldSwap)
            {
                int swapIndex = index;
                int swapParent = parent;

                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Swap,
                    Target = _nodes.Count > index ? _nodes[index] : null,
                    SecondaryTarget = _nodes.Count > parent ? _nodes[parent] : null,
                    Description = $"Swapping {tempNodes[index]} with {tempNodes[parent]}",
                    CustomAction = async () =>
                    {
                        if (_nodes.Count > swapIndex && _nodes.Count > swapParent)
                        {
                            SwapNodes(swapIndex, swapParent);
                            UpdateConnections();
                            OnVisualsChanged();
                        }
                    }
                });

                (tempNodes[index], tempNodes[parent]) = (tempNodes[parent], tempNodes[index]);
                index = parent;
            }
            else
            {
                break;
            }
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = $"Heap property restored. {value} is now at correct position."
        });

        return steps;
    }

    public List<AnimationStep> GenerateExtractRootSteps()
    {
        var steps = new List<AnimationStep>();

        if (_nodes.Count == 0)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Description = "Heap is empty, nothing to extract"
            });
            return steps;
        }

        var root = _nodes[0];
        string heapTypeStr = _heapType == HeapType.MinHeap ? "minimum" : "maximum";

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = root,
            Description = $"Extracting {heapTypeStr} value: {root.Value}"
        });

        if (_nodes.Count == 1)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Delete,
                Target = root,
                Description = $"Removing the only element",
                CustomAction = async () =>
                {
                    _nodes.Clear();
                    _elements.Remove(root);
                    _connections.Clear();
                    OnVisualsChanged();
                }
            });
            return steps;
        }

        var last = _nodes[^1];

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Move,
            Target = last,
            Description = $"Moving last element ({last.Value}) to root",
            CustomAction = async () =>
            {
                _nodes[0] = last;
                _nodes[0].Index = 0;
                _nodes.RemoveAt(_nodes.Count - 1);
                _elements.Remove(root);
                UpdateConnections();
                OnVisualsChanged();
            }
        });

        // Simulate heapify down
        var tempNodes = _nodes.Select(n => (int)n.Value!).ToList();
        tempNodes[0] = (int)last.Value!;
        tempNodes.RemoveAt(tempNodes.Count - 1);

        int index = 0;
        int size = tempNodes.Count;

        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int target = index;

            if (left < size)
            {
                bool leftBetter = _heapType == HeapType.MinHeap
                    ? tempNodes[left] < tempNodes[target]
                    : tempNodes[left] > tempNodes[target];
                if (leftBetter) target = left;
            }

            if (right < size)
            {
                bool rightBetter = _heapType == HeapType.MinHeap
                    ? tempNodes[right] < tempNodes[target]
                    : tempNodes[right] > tempNodes[target];
                if (rightBetter) target = right;
            }

            if (target != index)
            {
                int swapIndex = index;
                int swapTarget = target;

                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Compare,
                    Description = $"Comparing {tempNodes[index]} with children"
                });

                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Swap,
                    Description = $"Swapping {tempNodes[index]} with {tempNodes[target]}",
                    CustomAction = async () =>
                    {
                        if (_nodes.Count > swapIndex && _nodes.Count > swapTarget)
                        {
                            SwapNodes(swapIndex, swapTarget);
                            UpdateConnections();
                            OnVisualsChanged();
                        }
                    }
                });

                (tempNodes[index], tempNodes[target]) = (tempNodes[target], tempNodes[index]);
                index = target;
            }
            else
            {
                break;
            }
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = $"Heap property restored after extraction"
        });

        return steps;
    }

    public List<AnimationStep> GenerateBuildHeapSteps(IEnumerable<int> values)
    {
        var steps = new List<AnimationStep>();
        var valueList = values.ToList();

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = $"Building {_heapType} from {valueList.Count} elements"
        });

        // First, add all elements
        Clear();
        foreach (var value in valueList)
        {
            var node = new NodeElement(value) { Index = _nodes.Count };
            _nodes.Add(node);
            _elements.Add(node);
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = "All elements added, now heapifying...",
            CustomAction = async () =>
            {
                UpdateConnections();
                OnVisualsChanged();
            }
        });

        // Heapify from bottom up
        for (int i = _nodes.Count / 2 - 1; i >= 0; i--)
        {
            int current = i;
            
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = _nodes[current],
                Description = $"Heapifying subtree rooted at index {current} ({_nodes[current].Value})"
            });

            // Simulate heapify down for this subtree
            var tempNodes = _nodes.Select(n => (int)n.Value!).ToArray();
            int size = tempNodes.Length;

            while (true)
            {
                int left = 2 * current + 1;
                int right = 2 * current + 2;
                int target = current;

                if (left < size)
                {
                    bool leftBetter = _heapType == HeapType.MinHeap
                        ? tempNodes[left] < tempNodes[target]
                        : tempNodes[left] > tempNodes[target];
                    if (leftBetter) target = left;
                }

                if (right < size)
                {
                    bool rightBetter = _heapType == HeapType.MinHeap
                        ? tempNodes[right] < tempNodes[target]
                        : tempNodes[right] > tempNodes[target];
                    if (rightBetter) target = right;
                }

                if (target != current)
                {
                    int swapCurrent = current;
                    int swapTarget = target;

                    steps.Add(new AnimationStep
                    {
                        Type = AnimationType.Swap,
                        Target = _nodes[current],
                        SecondaryTarget = _nodes[target],
                        Description = $"Swapping {tempNodes[current]} with {tempNodes[target]}",
                        CustomAction = async () =>
                        {
                            SwapNodes(swapCurrent, swapTarget);
                            UpdateConnections();
                            OnVisualsChanged();
                        }
                    });

                    (tempNodes[current], tempNodes[target]) = (tempNodes[target], tempNodes[current]);
                    current = target;
                }
                else
                {
                    break;
                }
            }
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = $"{_heapType} construction complete!"
        });

        return steps;
    }

    #endregion
}
