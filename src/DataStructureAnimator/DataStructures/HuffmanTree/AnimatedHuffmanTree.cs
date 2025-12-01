using Avalonia;
using Avalonia.Media;
using DataStructureAnimator.Animations;
using DataStructureAnimator.Models;

namespace DataStructureAnimator.DataStructures.HuffmanTree;

/// <summary>
/// Represents a node in the Huffman tree
/// </summary>
public class HuffmanNode : NodeElement
{
    public char? Character { get; set; }
    public int Frequency { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsLeaf => Character.HasValue;

    public HuffmanNode(char? character, int frequency)
    {
        Character = character;
        Frequency = frequency;
        Value = frequency;
        Label = character.HasValue ? $"{character}:{frequency}" : frequency.ToString();
    }
}

/// <summary>
/// Animated Huffman tree for compression visualization
/// </summary>
public class AnimatedHuffmanTree : AnimatableDataStructureBase
{
    private HuffmanNode? _root;
    private readonly Dictionary<char, string> _codes = new();
    private readonly List<HuffmanNode> _allNodes = new();

    public HuffmanNode? Root => _root;
    public IReadOnlyDictionary<char, string> Codes => _codes;

    public AnimatedHuffmanTree() { }

    public void Initialize(string text)
    {
        Clear();
        
        // Calculate frequencies
        var frequencies = text
            .GroupBy(c => c)
            .ToDictionary(g => g.Key, g => g.Count());

        BuildTree(frequencies);
        OnVisualsChanged();
    }

    public void Initialize(Dictionary<char, int> frequencies)
    {
        Clear();
        BuildTree(frequencies);
        OnVisualsChanged();
    }

    private void BuildTree(Dictionary<char, int> frequencies)
    {
        if (frequencies.Count == 0) return;

        var nodes = frequencies
            .Select(kvp => new HuffmanNode(kvp.Key, kvp.Value))
            .ToList();

        _allNodes.AddRange(nodes);
        _elements.AddRange(nodes);

        // Build the tree using a priority queue simulation
        var queue = new List<HuffmanNode>(nodes);

        while (queue.Count > 1)
        {
            queue = queue.OrderBy(n => n.Frequency).ToList();

            var left = queue[0];
            var right = queue[1];
            queue.RemoveRange(0, 2);

            var parent = new HuffmanNode(null, left.Frequency + right.Frequency)
            {
                Left = left,
                Right = right
            };
            left.Parent = parent;
            right.Parent = parent;

            _allNodes.Add(parent);
            _elements.Add(parent);

            queue.Add(parent);
        }

        _root = queue.FirstOrDefault();

        // Generate codes
        GenerateCodes(_root, "");
        UpdateConnections();
    }

    private void GenerateCodes(HuffmanNode? node, string code)
    {
        if (node == null) return;

        node.Code = code;

        if (node.IsLeaf && node.Character.HasValue)
        {
            _codes[node.Character.Value] = code;
            node.Label = $"{node.Character}:{node.Frequency}\n[{code}]";
        }

        GenerateCodes(node.Left as HuffmanNode, code + "0");
        GenerateCodes(node.Right as HuffmanNode, code + "1");
    }

    public override void Clear()
    {
        _root = null;
        _codes.Clear();
        _allNodes.Clear();
        base.Clear();
    }

    public override void LayoutElements(double canvasWidth, double canvasHeight)
    {
        if (_root == null) return;

        double nodeSize = 50;
        int depth = GetTreeDepth(_root);
        double verticalSpacing = (canvasHeight - 100) / Math.Max(depth, 1);

        LayoutNode(_root, canvasWidth / 2, 50, canvasWidth / 4, nodeSize, verticalSpacing);
        UpdateConnections();
        OnVisualsChanged();
    }

    private void LayoutNode(HuffmanNode node, double x, double y, double horizontalSpread, double nodeSize, double verticalSpacing)
    {
        node.X = x - nodeSize / 2;
        node.Y = y;
        node.Width = nodeSize;
        node.Height = nodeSize;

        if (node.Left is HuffmanNode left)
        {
            LayoutNode(left, x - horizontalSpread, y + verticalSpacing, horizontalSpread / 2, nodeSize, verticalSpacing);
        }

        if (node.Right is HuffmanNode right)
        {
            LayoutNode(right, x + horizontalSpread, y + verticalSpacing, horizontalSpread / 2, nodeSize, verticalSpacing);
        }
    }

    private int GetTreeDepth(HuffmanNode? node)
    {
        if (node == null) return 0;
        return 1 + Math.Max(
            GetTreeDepth(node.Left as HuffmanNode),
            GetTreeDepth(node.Right as HuffmanNode));
    }

    private void UpdateConnections()
    {
        _connections.Clear();

        foreach (var node in _allNodes)
        {
            if (node.Left is HuffmanNode left)
            {
                var connection = new ConnectionElement
                {
                    SourceNode = node,
                    TargetNode = left,
                    StartPoint = new Point(node.X + node.Width / 2, node.Y + node.Height),
                    EndPoint = new Point(left.X + left.Width / 2, left.Y),
                    Label = "0"
                };
                _connections.Add(connection);
            }

            if (node.Right is HuffmanNode right)
            {
                var connection = new ConnectionElement
                {
                    SourceNode = node,
                    TargetNode = right,
                    StartPoint = new Point(node.X + node.Width / 2, node.Y + node.Height),
                    EndPoint = new Point(right.X + right.Width / 2, right.Y),
                    Label = "1"
                };
                _connections.Add(connection);
            }
        }
    }

    #region Operations with Animation Steps

    public List<AnimationStep> GenerateBuildTreeSteps(Dictionary<char, int> frequencies)
    {
        var steps = new List<AnimationStep>();
        Clear();

        if (frequencies.Count == 0)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Description = "No frequencies provided"
            });
            return steps;
        }

        // Create initial leaf nodes
        var nodes = new List<HuffmanNode>();
        foreach (var kvp in frequencies.OrderBy(x => x.Value))
        {
            var node = new HuffmanNode(kvp.Key, kvp.Value);
            nodes.Add(node);
            _allNodes.Add(node);
            _elements.Add(node);

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Insert,
                Target = node,
                Description = $"Creating leaf node for '{kvp.Key}' with frequency {kvp.Value}"
            });
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = $"Created {nodes.Count} leaf nodes. Starting tree construction...",
            CustomAction = async () =>
            {
                OnVisualsChanged();
            }
        });

        // Build tree step by step
        var queue = new List<HuffmanNode>(nodes);

        while (queue.Count > 1)
        {
            queue = queue.OrderBy(n => n.Frequency).ToList();

            var left = queue[0];
            var right = queue[1];

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = left,
                SecondaryTarget = right,
                Description = $"Selecting two nodes with smallest frequencies: {left.Frequency} and {right.Frequency}"
            });

            var parent = new HuffmanNode(null, left.Frequency + right.Frequency)
            {
                Left = left,
                Right = right
            };
            left.Parent = parent;
            right.Parent = parent;

            _allNodes.Add(parent);
            _elements.Add(parent);

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Insert,
                Target = parent,
                Description = $"Creating parent node with combined frequency: {parent.Frequency}",
                CustomAction = async () =>
                {
                    UpdateConnections();
                    OnVisualsChanged();
                }
            });

            queue.RemoveRange(0, 2);
            queue.Add(parent);

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Description = $"Priority queue now has {queue.Count} node(s)"
            });
        }

        _root = queue.FirstOrDefault();

        // Generate and display codes
        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = _root,
            Description = "Huffman tree complete! Generating codes..."
        });

        GenerateCodes(_root, "");

        foreach (var kvp in _codes.OrderBy(x => x.Value.Length))
        {
            var leafNode = _allNodes.FirstOrDefault(n => n.Character == kvp.Key);
            if (leafNode != null)
            {
                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Highlight,
                    Target = leafNode,
                    Description = $"Code for '{kvp.Key}': {kvp.Value}"
                });
            }
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = "Huffman encoding complete!",
            CustomAction = async () =>
            {
                OnVisualsChanged();
            }
        });

        return steps;
    }

    public List<AnimationStep> GenerateEncodeSteps(string text)
    {
        var steps = new List<AnimationStep>();

        if (_root == null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Description = "Tree not built yet. Please build the tree first."
            });
            return steps;
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = $"Encoding text: \"{text}\""
        });

        var encoded = new System.Text.StringBuilder();

        foreach (char c in text)
        {
            if (_codes.TryGetValue(c, out string? code))
            {
                // Trace path through tree
                var current = _root;
                
                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Highlight,
                    Target = current,
                    Description = $"Encoding '{c}': Starting at root"
                });

                foreach (char bit in code)
                {
                    var next = bit == '0' ? current.Left as HuffmanNode : current.Right as HuffmanNode;
                    
                    if (next != null)
                    {
                        steps.Add(new AnimationStep
                        {
                            Type = AnimationType.Highlight,
                            Target = next,
                            Description = $"Following {bit} edge..."
                        });
                        current = next;
                    }
                }

                encoded.Append(code);

                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Highlight,
                    Target = current,
                    Description = $"'{c}' encoded as: {code} | Total so far: {encoded}"
                });
            }
            else
            {
                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Highlight,
                    Description = $"Character '{c}' not found in Huffman tree!"
                });
            }
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = $"Encoding complete: {encoded}"
        });

        return steps;
    }

    public List<AnimationStep> GenerateDecodeSteps(string encoded)
    {
        var steps = new List<AnimationStep>();

        if (_root == null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Description = "Tree not built yet. Please build the tree first."
            });
            return steps;
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = $"Decoding: {encoded}"
        });

        var decoded = new System.Text.StringBuilder();
        var current = _root;

        foreach (char bit in encoded)
        {
            if (bit != '0' && bit != '1')
            {
                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Highlight,
                    Description = $"Invalid bit: {bit}. Skipping..."
                });
                continue;
            }

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = current,
                Description = $"Reading bit: {bit}"
            });

            current = (bit == '0' ? current.Left : current.Right) as HuffmanNode;

            if (current == null)
            {
                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Highlight,
                    Description = "Error: Invalid encoding path!"
                });
                break;
            }

            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = current,
                Description = $"Moved to node: {current.Label}"
            });

            if (current.IsLeaf && current.Character.HasValue)
            {
                decoded.Append(current.Character.Value);

                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Highlight,
                    Target = current,
                    Description = $"Decoded character: '{current.Character.Value}' | Result so far: {decoded}"
                });

                current = _root;
            }
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Description = $"Decoding complete: \"{decoded}\""
        });

        return steps;
    }

    #endregion
}
