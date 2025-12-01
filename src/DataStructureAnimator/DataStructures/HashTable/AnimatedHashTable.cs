using Avalonia;
using Avalonia.Media;
using DataStructureAnimator.Animations;
using DataStructureAnimator.Models;

namespace DataStructureAnimator.DataStructures.HashTable;

/// <summary>
/// Animated hash table with chaining for collision resolution
/// </summary>
public class AnimatedHashTable : AnimatableDataStructureBase
{
    private readonly int _bucketCount;
    private readonly List<HashBucketElement> _buckets = new();
    private int _itemCount;

    public int BucketCount => _bucketCount;
    public int ItemCount => _itemCount;
    public IReadOnlyList<HashBucketElement> Buckets => _buckets;

    public AnimatedHashTable(int bucketCount = 10)
    {
        _bucketCount = bucketCount;
        InitializeBuckets();
    }

    private void InitializeBuckets()
    {
        for (int i = 0; i < _bucketCount; i++)
        {
            var bucket = new HashBucketElement(i)
            {
                Label = $"[{i}]"
            };
            _buckets.Add(bucket);
            _elements.Add(bucket);
        }
    }

    public void Initialize(IEnumerable<int> values)
    {
        // Clear existing items but keep buckets
        foreach (var bucket in _buckets)
        {
            bucket.Chain.Clear();
            bucket.HasCollision = false;
        }
        _itemCount = 0;
        _connections.Clear();

        // Add values
        foreach (var value in values)
        {
            InsertDirect(value);
        }

        OnVisualsChanged();
    }

    private void InsertDirect(int value)
    {
        int index = GetHash(value);
        var bucket = _buckets[index];

        var node = new NodeElement(value);
        
        if (bucket.Chain.Count > 0)
        {
            bucket.HasCollision = true;
        }
        
        bucket.Chain.Add(node);
        _elements.Add(node);
        _itemCount++;
    }

    private int GetHash(int key)
    {
        return Math.Abs(key) % _bucketCount;
    }

    public override void Clear()
    {
        foreach (var bucket in _buckets)
        {
            bucket.Chain.Clear();
            bucket.HasCollision = false;
        }
        _itemCount = 0;
        
        // Clear elements but re-add buckets
        _elements.Clear();
        _elements.AddRange(_buckets);
        _connections.Clear();
        
        OnVisualsChanged();
    }

    public override void LayoutElements(double canvasWidth, double canvasHeight)
    {
        var bucketWidth = 80;
        var bucketHeight = 40;
        var bucketSpacing = 10;
        var chainNodeSize = 50;
        var chainSpacing = 60;

        var totalHeight = _bucketCount * (bucketHeight + bucketSpacing);
        var startY = (canvasHeight - totalHeight) / 2;
        var startX = 100;

        for (int i = 0; i < _buckets.Count; i++)
        {
            var bucket = _buckets[i];
            bucket.X = startX;
            bucket.Y = startY + i * (bucketHeight + bucketSpacing);
            bucket.Width = bucketWidth;
            bucket.Height = bucketHeight;

            // Layout chain nodes
            var chainX = startX + bucketWidth + 30;
            for (int j = 0; j < bucket.Chain.Count; j++)
            {
                var node = bucket.Chain[j];
                node.X = chainX + j * chainSpacing;
                node.Y = bucket.Y - (chainNodeSize - bucketHeight) / 2;
                node.Width = chainNodeSize;
                node.Height = chainNodeSize;
            }
        }

        UpdateConnections();
        OnVisualsChanged();
    }

    private void UpdateConnections()
    {
        _connections.Clear();

        foreach (var bucket in _buckets)
        {
            if (bucket.Chain.Count > 0)
            {
                // Connection from bucket to first chain node
                var firstNode = bucket.Chain[0];
                _connections.Add(new ConnectionElement
                {
                    StartPoint = new Point(bucket.X + bucket.Width, bucket.Y + bucket.Height / 2),
                    EndPoint = new Point(firstNode.X, firstNode.Y + firstNode.Height / 2)
                });

                // Connections between chain nodes
                for (int i = 0; i < bucket.Chain.Count - 1; i++)
                {
                    var current = bucket.Chain[i];
                    var next = bucket.Chain[i + 1];
                    _connections.Add(new ConnectionElement
                    {
                        StartPoint = new Point(current.X + current.Width, current.Y + current.Height / 2),
                        EndPoint = new Point(next.X, next.Y + next.Height / 2)
                    });
                }
            }
        }
    }

    #region Operations with Animation Steps

    public List<AnimationStep> GenerateInsertSteps(int value)
    {
        var steps = new List<AnimationStep>();
        int hash = GetHash(value);
        var bucket = _buckets[hash];

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = bucket,
            Description = $"Computing hash: {value} mod {_bucketCount} = {hash}"
        });

        var newNode = new NodeElement(value);

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Insert,
            Target = newNode,
            Description = $"Creating new node with value {value}"
        });

        if (bucket.Chain.Count > 0)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = bucket,
                Description = $"Collision detected at bucket {hash}!",
                CustomAction = async () =>
                {
                    bucket.Fill = Brushes.Orange;
                }
            });

            // Traverse chain
            foreach (var node in bucket.Chain)
            {
                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Compare,
                    Target = node,
                    Description = $"Traversing chain... current value: {node.Value}"
                });
            }
        }

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = newNode,
            Description = $"Inserting {value} into bucket {hash}",
            CustomAction = async () =>
            {
                bucket.Chain.Add(newNode);
                bucket.HasCollision = bucket.Chain.Count > 1;
                _elements.Add(newNode);
                _itemCount++;
                UpdateConnections();
                OnVisualsChanged();
            }
        });

        return steps;
    }

    public List<AnimationStep> GenerateSearchSteps(int value)
    {
        var steps = new List<AnimationStep>();
        int hash = GetHash(value);
        var bucket = _buckets[hash];

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = bucket,
            Description = $"Computing hash: {value} mod {_bucketCount} = {hash}"
        });

        if (bucket.Chain.Count == 0)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = bucket,
                Description = $"Bucket {hash} is empty. Value {value} not found."
            });
            return steps;
        }

        bool found = false;
        foreach (var node in bucket.Chain)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Compare,
                Target = node,
                Description = $"Comparing with {node.Value}..."
            });

            if ((int)node.Value! == value)
            {
                steps.Add(new AnimationStep
                {
                    Type = AnimationType.Highlight,
                    Target = node,
                    Description = $"Found {value}!",
                    Duration = TimeSpan.FromMilliseconds(1000)
                });
                found = true;
                break;
            }
        }

        if (!found)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = bucket,
                Description = $"Value {value} not found in bucket {hash}"
            });
        }

        return steps;
    }

    public List<AnimationStep> GenerateDeleteSteps(int value)
    {
        var steps = new List<AnimationStep>();
        int hash = GetHash(value);
        var bucket = _buckets[hash];

        steps.Add(new AnimationStep
        {
            Type = AnimationType.Highlight,
            Target = bucket,
            Description = $"Computing hash: {value} mod {_bucketCount} = {hash}"
        });

        NodeElement? toDelete = null;
        foreach (var node in bucket.Chain)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Compare,
                Target = node,
                Description = $"Comparing with {node.Value}..."
            });

            if ((int)node.Value! == value)
            {
                toDelete = node;
                break;
            }
        }

        if (toDelete != null)
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Delete,
                Target = toDelete,
                Description = $"Deleting {value} from bucket {hash}",
                CustomAction = async () =>
                {
                    bucket.Chain.Remove(toDelete);
                    _elements.Remove(toDelete);
                    bucket.HasCollision = bucket.Chain.Count > 1;
                    _itemCount--;
                    UpdateConnections();
                    OnVisualsChanged();
                }
            });
        }
        else
        {
            steps.Add(new AnimationStep
            {
                Type = AnimationType.Highlight,
                Target = bucket,
                Description = $"Value {value} not found, nothing to delete"
            });
        }

        return steps;
    }

    #endregion
}
