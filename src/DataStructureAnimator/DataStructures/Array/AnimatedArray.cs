using Avalonia.Media;
using DataStructureAnimator.Animations;
using DataStructureAnimator.Models;

namespace DataStructureAnimator.DataStructures.Array;

public class AnimatedArray : AnimatableDataStructureBase
{
    private readonly List<ArrayCellElement> _cells = new();
    public IReadOnlyList<ArrayCellElement> Cells => _cells;
    public int Count => _cells.Count;

    public void Initialize(IEnumerable<int> values)
    {
        Clear();
        int index = 0;
        foreach (var value in values)
        {
            var cell = new ArrayCellElement(value, index++);
            _cells.Add(cell);
            _elements.Add(cell);
        }
        OnVisualsChanged();
    }

    public override void LayoutElements(double canvasWidth, double canvasHeight)
    {
        if (_cells.Count == 0) return;
        var cellWidth = 60; var cellHeight = 60; var spacing = 10;
        var totalWidth = _cells.Count * (cellWidth + spacing) - spacing;
        var startX = (canvasWidth - totalWidth) / 2;
        var startY = canvasHeight / 2 - cellHeight / 2;

        for (int i = 0; i < _cells.Count; i++)
        {
            _cells[i].X = startX + i * (cellWidth + spacing);
            _cells[i].Y = startY;
            _cells[i].Width = cellWidth;
            _cells[i].Height = cellHeight;
        }
        OnVisualsChanged();
    }

    public override void Clear() { _cells.Clear(); base.Clear(); }

    public List<AnimationStep> GenerateBubbleSortSteps()
    {
        var steps = new List<AnimationStep>();
        var n = _cells.Count;
        var values = _cells.Select(c => (int)c.Value!).ToArray();
        var cells = _cells.ToArray();

        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                steps.Add(new AnimationStep { Type = AnimationType.Compare, Target = cells[j], SecondaryTarget = cells[j + 1], Description = $"Comparing {values[j]} and {values[j + 1]}" });
                if (values[j] > values[j + 1])
                {
                    steps.Add(new AnimationStep { Type = AnimationType.Swap, Target = cells[j], SecondaryTarget = cells[j + 1], Description = $"Swapping {values[j]} and {values[j + 1]}" });
                    (values[j], values[j + 1]) = (values[j + 1], values[j]);
                    (cells[j], cells[j + 1]) = (cells[j + 1], cells[j]);
                }
            }
            var idx = n - 1 - i;
            steps.Add(new AnimationStep { Type = AnimationType.Highlight, Target = cells[idx], Description = $"Element {values[idx]} is sorted", CustomAction = async () => { cells[idx].IsSorted = true; cells[idx].Fill = Brushes.LimeGreen; } });
        }
        return steps;
    }

    public List<AnimationStep> GenerateSelectionSortSteps()
    {
        var steps = new List<AnimationStep>();
        var n = _cells.Count;
        var values = _cells.Select(c => (int)c.Value!).ToArray();
        var cells = _cells.ToArray();

        for (int i = 0; i < n - 1; i++)
        {
            int minIdx = i;
            steps.Add(new AnimationStep { Type = AnimationType.Highlight, Target = cells[i], Description = $"Finding minimum from index {i}" });
            for (int j = i + 1; j < n; j++)
            {
                steps.Add(new AnimationStep { Type = AnimationType.Compare, Target = cells[j], SecondaryTarget = cells[minIdx], Description = $"Comparing {values[j]} with min {values[minIdx]}" });
                if (values[j] < values[minIdx]) { minIdx = j; steps.Add(new AnimationStep { Type = AnimationType.Highlight, Target = cells[minIdx], Description = $"New minimum: {values[minIdx]}" }); }
            }
            if (minIdx != i)
            {
                steps.Add(new AnimationStep { Type = AnimationType.Swap, Target = cells[i], SecondaryTarget = cells[minIdx], Description = $"Swapping {values[i]} with {values[minIdx]}" });
                (values[i], values[minIdx]) = (values[minIdx], values[i]);
                (cells[i], cells[minIdx]) = (cells[minIdx], cells[i]);
            }
        }
        return steps;
    }

    public List<AnimationStep> GenerateInsertionSortSteps()
    {
        var steps = new List<AnimationStep>();
        var n = _cells.Count;
        var values = _cells.Select(c => (int)c.Value!).ToArray();
        var cells = _cells.ToArray();

        for (int i = 1; i < n; i++)
        {
            var key = values[i]; var keyCell = cells[i]; int j = i - 1;
            steps.Add(new AnimationStep { Type = AnimationType.Highlight, Target = keyCell, Description = $"Inserting {key} into sorted portion" });
            while (j >= 0 && values[j] > key)
            {
                steps.Add(new AnimationStep { Type = AnimationType.Compare, Target = cells[j], SecondaryTarget = keyCell, Description = $"Comparing {values[j]} with key {key}" });
                steps.Add(new AnimationStep { Type = AnimationType.Move, Target = cells[j], Description = $"Moving {values[j]} right", Parameters = new Dictionary<string, object> { { "TargetX", cells[j].X + cells[j].Width + 10 } } });
                values[j + 1] = values[j]; cells[j + 1] = cells[j]; j--;
            }
            values[j + 1] = key; cells[j + 1] = keyCell;
        }
        return steps;
    }

    public List<AnimationStep> GenerateQuickSortSteps()
    {
        var steps = new List<AnimationStep>();
        var values = _cells.Select(c => (int)c.Value!).ToArray();
        var cells = _cells.ToArray();
        QuickSortHelper(steps, values, cells, 0, values.Length - 1);
        return steps;
    }

    private void QuickSortHelper(List<AnimationStep> steps, int[] values, ArrayCellElement[] cells, int low, int high)
    {
        if (low < high)
        {
            steps.Add(new AnimationStep { Type = AnimationType.Highlight, Target = cells[high], Description = $"Pivot: {values[high]}" });
            int pi = Partition(steps, values, cells, low, high);
            QuickSortHelper(steps, values, cells, low, pi - 1);
            QuickSortHelper(steps, values, cells, pi + 1, high);
        }
    }

    private int Partition(List<AnimationStep> steps, int[] values, ArrayCellElement[] cells, int low, int high)
    {
        int pivot = values[high]; int i = low - 1;
        for (int j = low; j < high; j++)
        {
            steps.Add(new AnimationStep { Type = AnimationType.Compare, Target = cells[j], SecondaryTarget = cells[high], Description = $"Comparing {values[j]} with pivot {pivot}" });
            if (values[j] < pivot)
            {
                i++;
                if (i != j)
                {
                    steps.Add(new AnimationStep { Type = AnimationType.Swap, Target = cells[i], SecondaryTarget = cells[j], Description = $"Swapping {values[i]} and {values[j]}" });
                    (values[i], values[j]) = (values[j], values[i]);
                    (cells[i], cells[j]) = (cells[j], cells[i]);
                }
            }
        }
        if (i + 1 != high)
        {
            steps.Add(new AnimationStep { Type = AnimationType.Swap, Target = cells[i + 1], SecondaryTarget = cells[high], Description = $"Placing pivot {pivot} at correct position" });
            (values[i + 1], values[high]) = (values[high], values[i + 1]);
            (cells[i + 1], cells[high]) = (cells[high], cells[i + 1]);
        }
        return i + 1;
    }

    public List<AnimationStep> GenerateSearchSteps(int target)
    {
        var steps = new List<AnimationStep>();
        for (int i = 0; i < _cells.Count; i++)
        {
            steps.Add(new AnimationStep { Type = AnimationType.Compare, Target = _cells[i], Description = $"Checking index {i}: {_cells[i].Value}" });
            if ((int)_cells[i].Value! == target)
            {
                steps.Add(new AnimationStep { Type = AnimationType.Highlight, Target = _cells[i], Description = $"Found {target} at index {i}!", Duration = TimeSpan.FromMilliseconds(1000) });
                break;
            }
        }
        return steps;
    }

    public List<AnimationStep> GenerateBinarySearchSteps(int target)
    {
        var steps = new List<AnimationStep>();
        int left = 0, right = _cells.Count - 1;
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            var midValue = (int)_cells[mid].Value!;
            steps.Add(new AnimationStep { Type = AnimationType.Highlight, Target = _cells[mid], Description = $"Checking mid at index {mid}: {midValue}" });
            if (midValue == target) { steps.Add(new AnimationStep { Type = AnimationType.Highlight, Target = _cells[mid], Description = $"Found {target}!", Duration = TimeSpan.FromMilliseconds(1000) }); break; }
            else if (midValue < target) { steps.Add(new AnimationStep { Type = AnimationType.Compare, Target = _cells[mid], Description = $"{midValue} < {target}, search right" }); left = mid + 1; }
            else { steps.Add(new AnimationStep { Type = AnimationType.Compare, Target = _cells[mid], Description = $"{midValue} > {target}, search left" }); right = mid - 1; }
        }
        return steps;
    }
}
