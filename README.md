# Visual Data Structure Animator

A beautiful, cross-platform desktop application for visualizing and animating common data structures and algorithms. Built with C# and Avalonia UI, it works on **macOS**, **Windows**, and **Linux**.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![Avalonia](https://img.shields.io/badge/Avalonia-11.1-purple?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

## Features

### Data Structures
- **Arrays** - Visualize array operations with sorting algorithms
- **Linked Lists** - Single and doubly linked list operations
- **Hash Tables** - Chaining collision resolution visualization
- **Heaps** - Min and Max heap operations with tree view
- **Huffman Trees** - Compression algorithm visualization

### Algorithms
| Category | Algorithms |
|----------|-----------|
| **Sorting** | Bubble Sort, Selection Sort, Insertion Sort, Quick Sort |
| **Searching** | Linear Search, Binary Search |
| **Tree Operations** | Insert, Delete, Build Heap, Extract Root |
| **Compression** | Huffman Encoding/Decoding |

### Animation Controls
- ▶️ Play/Pause animations
- ⏭️ Step-by-step execution
- 🔄 Speed control (0.25x - 4x)
- ↺ Reset to initial state

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- macOS 10.15+, Windows 10+, or Linux

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/DataStructureAnimator.git
   cd DataStructureAnimator
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run --project src/DataStructureAnimator
   ```

### Building for Release

```bash
# Build for your current platform
dotnet publish -c Release

# Build for specific platforms
dotnet publish -c Release -r osx-arm64    # macOS Apple Silicon
dotnet publish -c Release -r osx-x64      # macOS Intel
dotnet publish -c Release -r win-x64      # Windows
dotnet publish -c Release -r linux-x64    # Linux
```

## 📖 Usage Guide

### Basic Workflow

1. **Select a Data Structure** from the dropdown menu
2. **Enter initial values** (comma-separated for arrays, lists, heaps)
3. **Click "Initialize"** to create the data structure
4. **Select an Operation** to animate
5. **Enter a value** (if required for the operation)
6. **Click "Run"** to start the animation

### Example: Visualizing Bubble Sort

1. Select "Array" as the data structure
2. Enter values: `5, 3, 8, 1, 9, 2, 7`
3. Click "Initialize"
4. Select "Bubble Sort"
5. Click "Run" and watch the sorting process!

### Example: Building a Huffman Tree

1. Select "Huffman Tree"
2. Enter a string like `hello world` in the values field
3. Click "Initialize"
4. Select "Build Tree" and click "Run"
5. Watch the tree construction step by step!

## Project Structure

```
DataStructureAnimator/
├── src/
│   └── DataStructureAnimator/
│       ├── Animations/           # Animation engine
│       ├── Controls/             # Custom UI controls
│       ├── DataStructures/       # Data structure implementations
│       │   ├── Array/
│       │   ├── LinkedList/
│       │   ├── HashTable/
│       │   ├── Heap/
│       │   └── HuffmanTree/
│       ├── Models/               # Visual element models
│       ├── ViewModels/           # MVVM view models
│       ├── Views/                # UI views
│       └── Styles/               # Application styles
├── DataStructureAnimator.sln
└── README.md
```

## 🎯 Architecture

The application follows the **MVVM (Model-View-ViewModel)** pattern:

- **Models** (`VisualElement`, `NodeElement`, etc.) - Represent visual elements
- **ViewModels** (`MainWindowViewModel`) - Handle UI logic and state
- **Views** (`MainWindow`) - XAML-based UI
- **Animation Engine** - Manages step-by-step animations
- **Data Structures** - Implement `IAnimatableDataStructure` for consistent visualization

## 🛠️ Technology Stack

- **UI Framework**: [Avalonia UI](https://avaloniaui.net/) - Cross-platform .NET UI framework
- **MVVM Toolkit**: [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - Source generators for MVVM
- **Reactive Extensions**: [Avalonia.ReactiveUI](https://www.reactiveui.net/) - Reactive programming support
- **Language**: C# 12 with .NET 8

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Ideas for Contributions

- [ ] Add more sorting algorithms (Merge Sort, Heap Sort)
- [ ] Add Binary Search Tree visualization
- [ ] Add Graph algorithms (BFS, DFS, Dijkstra)
- [ ] Add AVL Tree / Red-Black Tree
- [ ] Add Stack and Queue visualizations
- [ ] Export animation as GIF
- [ ] Add code panel showing algorithm pseudocode

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Avalonia UI](https://avaloniaui.net/) for the amazing cross-platform framework
- Inspired by various algorithm visualization tools like VisuAlgo and Algorithm Visualizer

---

Made with ❤️ for learning data structures and algorithms
