using Avalonia.Controls;
using DataStructureAnimator.ViewModels;

namespace DataStructureAnimator.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Handle canvas size changes
        var canvas = this.FindControl<Control>("Canvas");
        if (canvas != null)
        {
            canvas.PropertyChanged += (s, e) =>
            {
                if (e.Property.Name == "Bounds" && DataContext is MainWindowViewModel vm)
                {
                    vm.OnCanvasSizeChanged(canvas.Bounds.Width, canvas.Bounds.Height);
                }
            };
        }
    }
}
