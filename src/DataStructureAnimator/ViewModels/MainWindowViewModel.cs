using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataStructureAnimator.Animations;
using DataStructureAnimator.DataStructures;
using DataStructureAnimator.DataStructures.Array;
using DataStructureAnimator.DataStructures.LinkedList;
using DataStructureAnimator.DataStructures.HashTable;
using DataStructureAnimator.DataStructures.Heap;
using DataStructureAnimator.DataStructures.HuffmanTree;

namespace DataStructureAnimator.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly AnimationEngine _animationEngine = new();
    private IAnimatableDataStructure? _currentDataStructure;

    [ObservableProperty]
    private string _selectedDataStructure = "Array";

    [ObservableProperty]
    private string _selectedOperation = "";

    [ObservableProperty]
    private string _inputValue = "";

    [ObservableProperty]
    private string _inputValues = "5, 3, 8, 1, 9, 2, 7, 4, 6";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _animationDescription = "";

    [ObservableProperty]
    private double _animationSpeed = 1.0;

    [ObservableProperty]
    private bool _isAnimating;

    [ObservableProperty]
    private int _currentStep;

    [ObservableProperty]
    private int _totalSteps;

    [ObservableProperty]
    private IAnimatableDataStructure? _dataStructure;

    public ObservableCollection<string> DataStructures { get; } = new()
    {
        "Array",
        "Linked List",
        "Hash Table",
        "Min Heap",
        "Max Heap",
        "Huffman Tree"
    };

    public ObservableCollection<string> Operations { get; } = new();

    public MainWindowViewModel()
    {
        _animationEngine.StepStarted += (s, step) =>
        {
            CurrentStep = _animationEngine.CurrentStep + 1;
            AnimationDescription = step.Description;
        };

        _animationEngine.AnimationCompleted += (s, e) =>
        {
            IsAnimating = false;
            StatusMessage = "Animation completed";
        };

        _animationEngine.DescriptionChanged += (s, desc) =>
        {
            AnimationDescription = desc;
        };

        UpdateOperations();
        InitializeDataStructure();
    }

    partial void OnSelectedDataStructureChanged(string value)
    {
        UpdateOperations();
        InitializeDataStructure();
    }

    partial void OnAnimationSpeedChanged(double value)
    {
        _animationEngine.SpeedMultiplier = value;
    }

    private void UpdateOperations()
    {
        Operations.Clear();

        switch (SelectedDataStructure)
        {
            case "Array":
                Operations.Add("Bubble Sort");
                Operations.Add("Selection Sort");
                Operations.Add("Insertion Sort");
                Operations.Add("Quick Sort");
                Operations.Add("Linear Search");
                Operations.Add("Binary Search");
                break;

            case "Linked List":
                Operations.Add("Insert at Head");
                Operations.Add("Insert at Tail");
                Operations.Add("Delete Head");
                Operations.Add("Search");
                Operations.Add("Reverse");
                break;

            case "Hash Table":
                Operations.Add("Insert");
                Operations.Add("Search");
                Operations.Add("Delete");
                break;

            case "Min Heap":
            case "Max Heap":
                Operations.Add("Build Heap");
                Operations.Add("Insert");
                Operations.Add("Extract Root");
                break;

            case "Huffman Tree":
                Operations.Add("Build Tree");
                Operations.Add("Encode");
                Operations.Add("Decode");
                break;
        }

        SelectedOperation = Operations.FirstOrDefault() ?? "";
    }

    private void InitializeDataStructure()
    {
        _animationEngine.Clear();

        switch (SelectedDataStructure)
        {
            case "Array":
                var array = new AnimatedArray();
                array.Initialize(ParseIntValues());
                _currentDataStructure = array;
                break;

            case "Linked List":
                var linkedList = new AnimatedLinkedList();
                linkedList.Initialize(ParseIntValues());
                _currentDataStructure = linkedList;
                break;

            case "Hash Table":
                var hashTable = new AnimatedHashTable(7);
                hashTable.Initialize(ParseIntValues());
                _currentDataStructure = hashTable;
                break;

            case "Min Heap":
                var minHeap = new AnimatedHeap(HeapType.MinHeap);
                minHeap.Initialize(ParseIntValues(), HeapType.MinHeap);
                _currentDataStructure = minHeap;
                break;

            case "Max Heap":
                var maxHeap = new AnimatedHeap(HeapType.MaxHeap);
                maxHeap.Initialize(ParseIntValues(), HeapType.MaxHeap);
                _currentDataStructure = maxHeap;
                break;

            case "Huffman Tree":
                var huffman = new AnimatedHuffmanTree();
                var text = InputValues.Replace(",", "").Replace(" ", "");
                if (string.IsNullOrEmpty(text)) text = "hello world";
                huffman.Initialize(text);
                _currentDataStructure = huffman;
                break;
        }

        DataStructure = _currentDataStructure;
        StatusMessage = $"{SelectedDataStructure} initialized";
    }

    private IEnumerable<int> ParseIntValues()
    {
        return InputValues
            .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out int val) ? val : 0)
            .Where(v => v != 0 || InputValues.Contains("0"));
    }

    [RelayCommand]
    private void Initialize()
    {
        InitializeDataStructure();
        _currentDataStructure?.LayoutElements(800, 400);
    }

    [RelayCommand]
    private async Task RunAnimation()
    {
        if (_currentDataStructure == null) return;

        _animationEngine.Clear();
        var steps = GenerateSteps();

        if (steps.Count == 0)
        {
            StatusMessage = "No steps to animate";
            return;
        }

        _animationEngine.EnqueueSteps(steps);
        TotalSteps = steps.Count;
        CurrentStep = 0;
        IsAnimating = true;
        StatusMessage = "Animating...";

        await _animationEngine.PlayAsync();
    }

    private List<AnimationStep> GenerateSteps()
    {
        int.TryParse(InputValue, out int value);

        switch (SelectedDataStructure)
        {
            case "Array" when _currentDataStructure is AnimatedArray array:
                return SelectedOperation switch
                {
                    "Bubble Sort" => array.GenerateBubbleSortSteps(),
                    "Selection Sort" => array.GenerateSelectionSortSteps(),
                    "Insertion Sort" => array.GenerateInsertionSortSteps(),
                    "Quick Sort" => array.GenerateQuickSortSteps(),
                    "Linear Search" => array.GenerateSearchSteps(value),
                    "Binary Search" => array.GenerateBinarySearchSteps(value),
                    _ => new List<AnimationStep>()
                };

            case "Linked List" when _currentDataStructure is AnimatedLinkedList list:
                return SelectedOperation switch
                {
                    "Insert at Head" => list.GenerateInsertAtHeadSteps(value),
                    "Insert at Tail" => list.GenerateInsertAtTailSteps(value),
                    "Delete Head" => list.GenerateDeleteAtHeadSteps(),
                    "Search" => list.GenerateSearchSteps(value),
                    "Reverse" => list.GenerateReverseSteps(),
                    _ => new List<AnimationStep>()
                };

            case "Hash Table" when _currentDataStructure is AnimatedHashTable hash:
                return SelectedOperation switch
                {
                    "Insert" => hash.GenerateInsertSteps(value),
                    "Search" => hash.GenerateSearchSteps(value),
                    "Delete" => hash.GenerateDeleteSteps(value),
                    _ => new List<AnimationStep>()
                };

            case "Min Heap" or "Max Heap" when _currentDataStructure is AnimatedHeap heap:
                return SelectedOperation switch
                {
                    "Build Heap" => heap.GenerateBuildHeapSteps(ParseIntValues()),
                    "Insert" => heap.GenerateInsertSteps(value),
                    "Extract Root" => heap.GenerateExtractRootSteps(),
                    _ => new List<AnimationStep>()
                };

            case "Huffman Tree" when _currentDataStructure is AnimatedHuffmanTree huffman:
                var frequencies = InputValues.Replace(",", "").Replace(" ", "")
                    .GroupBy(c => c)
                    .ToDictionary(g => g.Key, g => g.Count());
                return SelectedOperation switch
                {
                    "Build Tree" => huffman.GenerateBuildTreeSteps(frequencies),
                    "Encode" => huffman.GenerateEncodeSteps(InputValue),
                    "Decode" => huffman.GenerateDecodeSteps(InputValue),
                    _ => new List<AnimationStep>()
                };

            default:
                return new List<AnimationStep>();
        }
    }

    [RelayCommand]
    private void PauseAnimation()
    {
        if (_animationEngine.IsPlaying && !_animationEngine.IsPaused)
        {
            _animationEngine.Pause();
            StatusMessage = "Paused";
        }
        else if (_animationEngine.IsPaused)
        {
            _animationEngine.Resume();
            StatusMessage = "Resumed";
        }
    }

    [RelayCommand]
    private void StopAnimation()
    {
        _animationEngine.Stop();
        IsAnimating = false;
        StatusMessage = "Stopped";
    }

    [RelayCommand]
    private async Task StepForward()
    {
        if (_animationEngine.CurrentStep < _animationEngine.TotalSteps)
        {
            await _animationEngine.StepForwardAsync();
            CurrentStep = _animationEngine.CurrentStep;
        }
    }

    [RelayCommand]
    private void ResetAnimation()
    {
        _animationEngine.Reset();
        CurrentStep = 0;
        InitializeDataStructure();
        _currentDataStructure?.LayoutElements(800, 400);
        StatusMessage = "Reset complete";
    }

    public void OnCanvasSizeChanged(double width, double height)
    {
        _currentDataStructure?.LayoutElements(width, height);
    }
}
