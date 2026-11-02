namespace Webo.App;

/// <summary>
/// Snapshot-based undo/redo. A snapshot is the serialized page list, so every kind of edit (pages, sections,
/// fields) is undone the same way. Rapid edits with the same key (typing in one text box) are merged into one step.
/// </summary>
public sealed class EditHistory
{
    private const int MaxSteps = 200;
    private static readonly TimeSpan MergeWindow = TimeSpan.FromSeconds(1.2);

    private readonly List<string> _undo = new();
    private readonly List<string> _redo = new();
    private string _current;
    private string? _lastKey;
    private DateTime _lastAt = DateTime.MinValue;

    public EditHistory(string initialSnapshot) => _current = initialSnapshot;

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    /// <summary>Records the state after an edit. Returns false when nothing actually changed.</summary>
    public bool Commit(string snapshot, string? mergeKey = null)
    {
        if (snapshot == _current) return false;

        var now = DateTime.UtcNow;
        var merge = mergeKey != null && mergeKey == _lastKey && now - _lastAt < MergeWindow && _undo.Count > 0;
        if (!merge)
        {
            _undo.Add(_current);
            if (_undo.Count > MaxSteps) _undo.RemoveAt(0);
        }

        _current = snapshot;
        _redo.Clear();
        _lastKey = mergeKey;
        _lastAt = now;
        return true;
    }

    public string? Undo()
    {
        if (_undo.Count == 0) return null;
        _redo.Add(_current);
        _current = _undo[^1];
        _undo.RemoveAt(_undo.Count - 1);
        _lastKey = null;
        return _current;
    }

    public string? Redo()
    {
        if (_redo.Count == 0) return null;
        _undo.Add(_current);
        _current = _redo[^1];
        _redo.RemoveAt(_redo.Count - 1);
        _lastKey = null;
        return _current;
    }
}
