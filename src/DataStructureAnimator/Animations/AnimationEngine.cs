using Avalonia.Media;
using DataStructureAnimator.Models;

namespace DataStructureAnimator.Animations;

/// <summary>
/// Defines different types of animations
/// </summary>
public enum AnimationType
{
    Insert,
    Delete,
    Search,
    Swap,
    Compare,
    Highlight,
    Move,
    Fade,
    Scale,
    Connect,
    Disconnect
}

/// <summary>
/// Represents a single animation step
/// </summary>
public class AnimationStep
{
    public AnimationType Type { get; set; }
    public VisualElement? Target { get; set; }
    public VisualElement? SecondaryTarget { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(500);
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public Func<Task>? CustomAction { get; set; }
}

/// <summary>
/// Animation engine that handles all visual animations
/// </summary>
public class AnimationEngine
{
    private readonly List<AnimationStep> _stepQueue = new();
    private bool _isPlaying;
    private bool _isPaused;
    private int _currentStepIndex;
    private CancellationTokenSource? _cts;

    public event EventHandler<AnimationStep>? StepStarted;
    public event EventHandler<AnimationStep>? StepCompleted;
    public event EventHandler? AnimationCompleted;
    public event EventHandler<string>? DescriptionChanged;

    public double SpeedMultiplier { get; set; } = 1.0;
    public bool IsPlaying => _isPlaying;
    public bool IsPaused => _isPaused;
    public int CurrentStep => _currentStepIndex;
    public int TotalSteps => _stepQueue.Count;

    public void EnqueueStep(AnimationStep step) => _stepQueue.Add(step);
    public void EnqueueSteps(IEnumerable<AnimationStep> steps) => _stepQueue.AddRange(steps);

    public void Clear()
    {
        Stop();
        _stepQueue.Clear();
        _currentStepIndex = 0;
    }

    public async Task PlayAsync()
    {
        if (_isPlaying && !_isPaused) return;

        _isPlaying = true;
        _isPaused = false;
        _cts = new CancellationTokenSource();

        try
        {
            while (_currentStepIndex < _stepQueue.Count && !_cts.Token.IsCancellationRequested)
            {
                var step = _stepQueue[_currentStepIndex];
                
                StepStarted?.Invoke(this, step);
                DescriptionChanged?.Invoke(this, step.Description);

                await ExecuteStepAsync(step, _cts.Token);

                StepCompleted?.Invoke(this, step);
                _currentStepIndex++;

                while (_isPaused && !_cts.Token.IsCancellationRequested)
                    await Task.Delay(50, _cts.Token);
            }

            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException) { }
        finally { _isPlaying = false; }
    }

    public void Pause() => _isPaused = true;
    public void Resume() => _isPaused = false;
    public void Stop() { _cts?.Cancel(); _isPlaying = false; _isPaused = false; }
    public void Reset() { Stop(); _currentStepIndex = 0; }

    public async Task StepForwardAsync()
    {
        if (_currentStepIndex < _stepQueue.Count)
        {
            var step = _stepQueue[_currentStepIndex];
            StepStarted?.Invoke(this, step);
            DescriptionChanged?.Invoke(this, step.Description);
            await ExecuteStepAsync(step, CancellationToken.None);
            StepCompleted?.Invoke(this, step);
            _currentStepIndex++;
        }
    }

    private async Task ExecuteStepAsync(AnimationStep step, CancellationToken ct)
    {
        var duration = TimeSpan.FromMilliseconds(step.Duration.TotalMilliseconds / SpeedMultiplier);

        switch (step.Type)
        {
            case AnimationType.Highlight: await AnimateHighlightAsync(step, duration, ct); break;
            case AnimationType.Move: await AnimateMoveAsync(step, duration, ct); break;
            case AnimationType.Swap: await AnimateSwapAsync(step, duration, ct); break;
            case AnimationType.Compare: await AnimateCompareAsync(step, duration, ct); break;
            case AnimationType.Insert: await AnimateInsertAsync(step, duration, ct); break;
            case AnimationType.Delete: await AnimateDeleteAsync(step, duration, ct); break;
            case AnimationType.Scale: await AnimateScaleAsync(step, duration, ct); break;
            case AnimationType.Fade: await AnimateFadeAsync(step, duration, ct); break;
            default: if (step.CustomAction != null) await step.CustomAction(); break;
        }
    }

    private async Task AnimateHighlightAsync(AnimationStep step, TimeSpan duration, CancellationToken ct)
    {
        if (step.Target == null) return;
        var originalFill = step.Target.Fill;
        step.Target.Fill = Brushes.Gold;
        step.Target.IsHighlighted = true;
        await Task.Delay(duration, ct);
        step.Target.Fill = originalFill;
        step.Target.IsHighlighted = false;
    }

    private async Task AnimateMoveAsync(AnimationStep step, TimeSpan duration, CancellationToken ct)
    {
        if (step.Target == null) return;
        var targetX = step.Parameters.GetValueOrDefault("TargetX", step.Target.X);
        var targetY = step.Parameters.GetValueOrDefault("TargetY", step.Target.Y);
        var startX = step.Target.X;
        var startY = step.Target.Y;
        var steps = 20;
        var stepDuration = duration.TotalMilliseconds / steps;

        for (int i = 0; i <= steps && !ct.IsCancellationRequested; i++)
        {
            var t = EaseInOutCubic((double)i / steps);
            step.Target.X = Lerp(startX, (double)targetX, t);
            step.Target.Y = Lerp(startY, (double)targetY, t);
            await Task.Delay((int)stepDuration, ct);
        }
    }

    private async Task AnimateSwapAsync(AnimationStep step, TimeSpan duration, CancellationToken ct)
    {
        if (step.Target == null || step.SecondaryTarget == null) return;
        var t1 = step.Target; var t2 = step.SecondaryTarget;
        var of1 = t1.Fill; var of2 = t2.Fill;
        t1.Fill = Brushes.Orange; t2.Fill = Brushes.Orange;
        var x1 = t1.X; var y1 = t1.Y; var x2 = t2.X; var y2 = t2.Y;
        var steps = 20; var stepDuration = duration.TotalMilliseconds / steps;

        for (int i = 0; i <= steps && !ct.IsCancellationRequested; i++)
        {
            var t = EaseInOutCubic((double)i / steps);
            var arc = Math.Sin(t * Math.PI) * 30;
            t1.X = Lerp(x1, x2, t); t1.Y = Lerp(y1, y2, t) - arc;
            t2.X = Lerp(x2, x1, t); t2.Y = Lerp(y2, y1, t) + arc;
            await Task.Delay((int)stepDuration, ct);
        }
        t1.Fill = of1; t2.Fill = of2;
    }

    private async Task AnimateCompareAsync(AnimationStep step, TimeSpan duration, CancellationToken ct)
    {
        if (step.Target == null) return;
        var of = step.Target.Fill;
        step.Target.Fill = Brushes.Cyan;
        if (step.SecondaryTarget != null)
        {
            var of2 = step.SecondaryTarget.Fill;
            step.SecondaryTarget.Fill = Brushes.Cyan;
            await Task.Delay(duration, ct);
            step.SecondaryTarget.Fill = of2;
        }
        else await Task.Delay(duration, ct);
        step.Target.Fill = of;
    }

    private async Task AnimateInsertAsync(AnimationStep step, TimeSpan duration, CancellationToken ct)
    {
        if (step.Target == null) return;
        step.Target.Opacity = 0; step.Target.Scale = 0; step.Target.Fill = Brushes.LimeGreen;
        var steps = 15; var stepDuration = duration.TotalMilliseconds / steps;
        for (int i = 0; i <= steps && !ct.IsCancellationRequested; i++)
        {
            var t = EaseOutBack((double)i / steps);
            step.Target.Opacity = t; step.Target.Scale = t;
            await Task.Delay((int)stepDuration, ct);
        }
        await Task.Delay(200, ct);
        step.Target.Fill = Brushes.DodgerBlue;
    }

    private async Task AnimateDeleteAsync(AnimationStep step, TimeSpan duration, CancellationToken ct)
    {
        if (step.Target == null) return;
        step.Target.Fill = Brushes.Red;
        var steps = 15; var stepDuration = duration.TotalMilliseconds / steps;
        for (int i = steps; i >= 0 && !ct.IsCancellationRequested; i--)
        {
            var t = (double)i / steps;
            step.Target.Opacity = t; step.Target.Scale = t;
            await Task.Delay((int)stepDuration, ct);
        }
    }

    private async Task AnimateScaleAsync(AnimationStep step, TimeSpan duration, CancellationToken ct)
    {
        if (step.Target == null) return;
        var targetScale = step.Parameters.GetValueOrDefault("TargetScale", 1.0);
        var startScale = step.Target.Scale;
        var steps = 15; var stepDuration = duration.TotalMilliseconds / steps;
        for (int i = 0; i <= steps && !ct.IsCancellationRequested; i++)
        {
            step.Target.Scale = Lerp(startScale, (double)targetScale, EaseOutCubic((double)i / steps));
            await Task.Delay((int)stepDuration, ct);
        }
    }

    private async Task AnimateFadeAsync(AnimationStep step, TimeSpan duration, CancellationToken ct)
    {
        if (step.Target == null) return;
        var targetOpacity = step.Parameters.GetValueOrDefault("TargetOpacity", 1.0);
        var startOpacity = step.Target.Opacity;
        var steps = 15; var stepDuration = duration.TotalMilliseconds / steps;
        for (int i = 0; i <= steps && !ct.IsCancellationRequested; i++)
        {
            step.Target.Opacity = Lerp(startOpacity, (double)targetOpacity, EaseInOutCubic((double)i / steps));
            await Task.Delay((int)stepDuration, ct);
        }
    }

    private static double Lerp(double start, double end, double t) => start + (end - start) * t;
    private static double EaseInOutCubic(double t) => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
    private static double EaseOutCubic(double t) => 1 - Math.Pow(1 - t, 3);
    private static double EaseOutBack(double t) { const double c1 = 1.70158, c3 = c1 + 1; return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2); }
}
