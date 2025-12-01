using Avalonia;
using Avalonia.Media;
using DataStructureAnimator.Animations;
using DataStructureAnimator.Models;

namespace DataStructureAnimator.DataStructures.LinkedList;

/// <summary>
/// Animated linked list data structure
/// </summary>
public class AnimatedLinkedList : AnimatableDataStructureBase
{
    private NodeElement? _head;
    private NodeElement? _tail;
    private int _count;

    public NodeElement? Head => _head;
    public NodeElement? Tail => _tail;
    public int Count => _count;

    public AnimatedLinkedList() { }

    public AnimatedLinkedList(IEnumerable<int> values)
    {
        foreach (var value in values)
        {
            AddLast(value);
        }
    }

    public void Initialize(IEnumerable<int> values)
    {
        Clear();
        foreach (var value in values)
        {
            AddLast(value);
        }
    }

    private void AddLast(int value)
    {
        var newNode = new NodeElement(value) { Index = _count };
        
        if (_head == null)
        {
            _head = newNode;
            _tail = newNode;
        }
        else
        {
            _tail!.Next = newNode;
            newNode.Previous = _tail;
            _tail = newNode;
        }

        _elements.Add(newNode);
        _count++;
        UpdateConnections();
        OnVisualsChanged();
    }

    public override void Clear()
    {
        _head = null;
        _tail = null;
        _count = 0;
        base.Clear();
    }

    public override void LayoutElements(double canvasWidth, double canvasHeight)
    {
        if (_head == null) return;

        var nodeWidth = 60;
        var nodeHeight = 60;
        var spacing = 80; // Extra space for arrows
        var totalWidth = _count * nodeWidth + (_count - 1) * spacing;
        var startX = (canvasWidth - totalWidth) / 2;
        var startY = canvasHeight / 2 - nodeHeight / 2;

        var current = _head;
        int index = 0;
        while (current != null)
        {
            current.X = startX + index * (nodeWidth + spacing);
            current.Y = startY;
            current.Width = nodeWidth;
            current.Height = nodeHeight;
            current = current.Next;
            index++;
        }

        UpdateConnections();
        OnVisualsChanged();
    }

    private void UpdateConnections()
    {
        _connections.Clear();
        
        var current = _head;
        while (current?.Next != null)
        {
            var connection = new ConnectionElement
            {
                SourceNode = current,
                TargetNode = current.Next,
                StartPoint = new Point(current.X + current.Width, current.Y + current.Height / 2),
                EndPoint = new Point(current.Next.X, current.Next.Y + current.Next.Height / 2)
            };
            _connections.Add(connection);
            current = current.Next;
        }
    }

    #region Operations with Animation Steps

    public List<AnimationStep> GenerateInsertAtHeadSteps(int value)
    {
        var steps = new List<AnimationStep>();
        var newNode = new NodeElement(value) { Index = 0 };

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Insert,
            Target = newNode,
            Description = $"Creating new node with value {value}"
        });

        if (_head != null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Connect,
                Target = newNode,
                SecondaryTarget = _head,
                Description = $"Connecting new node to current head ({_head.Value})"
            });
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = newNode,
            Description = "Setting new node as head",
            CustomAction = async () =>
            {
                newNode.Next = _head;
                if (_head != null) _head.Previous = newNode;
                _head = newNode;
                if (_tail == null) _tail = newNode;
                _elements.Insert(0, newNode);
                _count++;
                UpdateConnections();
                OnVisualsChanged();
            }
        });

        return steps;
    }

    public List<AnimationStep> GenerateInsertAtTailSteps(int value)
    {
        var steps = new List<AnimationStep>();
        var newNode = new NodeElement(value) { Index = _count };

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Insert,
            Target = newNode,
            Description = $"Creating new node with value {value}"
        });

        if (_tail != null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = _tail,
                Description = $"Finding current tail ({_tail.Value})"
            });

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Connect,
                Target = _tail,
                SecondaryTarget = newNode,
                Description = $"Connecting current tail to new node"
            });
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = newNode,
            Description = "Setting new node as tail",
            CustomAction = async () =>
            {
                if (_tail != null)
                {
                    _tail.Next = newNode;
                    newNode.Previous = _tail;
                }
                else
                {
                    _head = newNode;
                }
                _tail = newNode;
                _elements.Add(newNode);
                _count++;
                UpdateConnections();
                OnVisualsChanged();
            }
        });

        return steps;
    }

    public List<AnimationStep> GenerateInsertAtIndexSteps(int index, int value)
    {
        var steps = new List<AnimationStep>();

        if (index < 0 || index > _count)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Description = $"Invalid index: {index}"
            });
            return steps;
        }

        if (index == 0) return GenerateInsertAtHeadSteps(value);
        if (index == _count) return GenerateInsertAtTailSteps(value);

        var newNode = new NodeElement(value);

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Insert,
            Target = newNode,
            Description = $"Creating new node with value {value}"
        });

        // Traverse to find position
        var current = _head;
        for (int i = 0; i < index - 1 && current != null; i++)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = current,
                Description = $"Traversing... at index {i}"
            });
            current = current.Next;
        }

        if (current != null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = current,
                Description = $"Found insertion point after node {current.Value}"
            });

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Connect,
                Target = newNode,
                SecondaryTarget = current.Next,
                Description = "Updating connections",
                CustomAction = async () =>
                {
                    newNode.Next = current.Next;
                    newNode.Previous = current;
                    if (current.Next != null) current.Next.Previous = newNode;
                    current.Next = newNode;
                    newNode.Index = index;
                    _elements.Insert(index, newNode);
                    _count++;
                    UpdateConnections();
                    OnVisualsChanged();
                }
            });
        }

        return steps;
    }

    public List<AnimationStep> GenerateDeleteAtHeadSteps()
    {
        var steps = new List<AnimationStep>();

        if (_head == null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Description = "List is empty, nothing to delete"
            });
            return steps;
        }

        var toDelete = _head;

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = toDelete,
            Description = $"Marking head node ({toDelete.Value}) for deletion"
        });

        if (_head.Next != null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = _head.Next,
                Description = $"Next node ({_head.Next.Value}) will become new head"
            });
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Delete,
            Target = toDelete,
            Description = $"Deleting node {toDelete.Value}",
            CustomAction = async () =>
            {
                _elements.Remove(toDelete);
                _head = _head!.Next;
                if (_head != null) _head.Previous = null;
                else _tail = null;
                _count--;
                UpdateConnections();
                OnVisualsChanged();
            }
        });

        return steps;
    }

    public List<AnimationStep> GenerateSearchSteps(int target)
    {
        var steps = new List<AnimationStep>();
        var current = _head;
        int index = 0;

        while (current != null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Compare,
                Target = current,
                Description = $"Checking node at index {index}: {current.Value}"
            });

            if ((int)current.Value! == target)
            {
                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Highlight,
                    Target = current,
                    Description = $"Found {target} at index {index}!",
                    Duration = TimeSpan.FromMilliseconds(1000)
                });
                break;
            }

            current = current.Next;
            index++;
        }

        if (current == null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Description = $"Value {target} not found in the list"
            });
        }

        return steps;
    }

    public List<AnimationStep> GenerateReverseSteps()
    {
        var steps = new List<AnimationStep>();
        
        if (_head == null || _head.Next == null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Description = "List has 0 or 1 elements, no reversal needed"
            });
            return steps;
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = _head,
            Description = "Starting list reversal"
        });

        NodeElement? prev = null;
        var current = _head;
        _tail = _head;

        while (current != null)
        {
            var next = current.Next;

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = current,
                Description = $"Reversing pointer for node {current.Value}"
            });

            current.Next = prev;
            current.Previous = next;
            prev = current;
            current = next;
        }

        _head = prev;

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = _head,
            Description = "Reversal complete! New head: " + _head?.Value,
            CustomAction = async () =>
            {
                UpdateConnections();
                OnVisualsChanged();
            }
        });

        return steps;
    }

    #endregion
}
