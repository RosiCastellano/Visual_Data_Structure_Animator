using DataStructureAnimator.Models;

namespace DataStructureAnimator.DataStructures;

public interface IAnimatableDataStructure
{
    IEnumerable<VisualElement> GetVisualElements();
    IEnumerable<ConnectionElement> GetConnections();
    void LayoutElements(double canvasWidth, double canvasHeight);
    void Clear();
    event EventHandler? VisualsChanged;
}

public abstract class AnimatableDataStructureBase : IAnimatableDataStructure
{
    protected readonly List<VisualElement> _elements = new();
    protected readonly List<ConnectionElement> _connections = new();

    public event EventHandler? VisualsChanged;

    public virtual IEnumerable<VisualElement> GetVisualElements() => _elements;
    public virtual IEnumerable<ConnectionElement> GetConnections() => _connections;
    public abstract void LayoutElements(double canvasWidth, double canvasHeight);

    public virtual void Clear()
    {
        _elements.Clear();
        _connections.Clear();
        OnVisualsChanged();
    }

    protected void OnVisualsChanged() => VisualsChanged?.Invoke(this, EventArgs.Empty);
}
