using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MoneyTracker.Presentation.Collections;

/// <summary>
/// ObservableCollection optimizada para grandes datasets con virtualización y paginación
/// </summary>
public class VirtualizedObservableCollection<T> : INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
{
    private readonly List<T> _allItems = new();
    private readonly List<T> _visibleItems = new();
    private readonly int _pageSize;
    private readonly object _lock = new();

    private int _currentPage = 0;
    private string _filterText = string.Empty;
    private Func<T, string, bool>? _filterPredicate;
    private Func<T, T, int>? _sortComparer;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public VirtualizedObservableCollection(int pageSize = 50)
    {
        _pageSize = Math.Max(1, pageSize);
    }

    // Public properties
    public IReadOnlyList<T> VisibleItems
    {
        get
        {
            lock (_lock)
            {
                return _visibleItems.ToList(); // Return copy for thread safety
            }
        }
    }

    public int TotalCount
    {
        get
        {
            lock (_lock)
            {
                return GetFilteredItems().Count();
            }
        }
    }

    public int VisibleCount
    {
        get
        {
            lock (_lock)
            {
                return _visibleItems.Count;
            }
        }
    }

    public bool HasMorePages
    {
        get
        {
            lock (_lock)
            {
                var filteredCount = GetFilteredItems().Count();
                return (_currentPage + 1) * _pageSize < filteredCount;
            }
        }
    }

    public bool HasPreviousPages => _currentPage > 0;

    public int CurrentPage
    {
        get
        {
            lock (_lock)
            {
                return _currentPage;
            }
        }
    }

    public int PageSize => _pageSize;

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (_filterText != value)
            {
                _filterText = value;
                OnPropertyChanged();
                RefreshVisibleItems();
            }
        }
    }

    // Main operations
    public void ReplaceAll(IEnumerable<T> newItems)
    {
        lock (_lock)
        {
            _allItems.Clear();
            if (newItems != null)
            {
                _allItems.AddRange(newItems);
            }
            _currentPage = 0;
            RefreshVisibleItemsInternal();
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(HasMorePages));
        OnPropertyChanged(nameof(HasPreviousPages));
        OnPropertyChanged(nameof(CurrentPage));
    }

    public void AddItem(T item)
    {
        if (item == null) return;

        lock (_lock)
        {
            _allItems.Add(item);

            // Check if item should be visible in current page
            if (ShouldItemBeVisible(item))
            {
                RefreshVisibleItemsInternal();
                OnCollectionChanged(NotifyCollectionChangedAction.Reset);
            }
        }

        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(HasMorePages));
    }

    public bool RemoveItem(T item)
    {
        if (item == null) return false;

        bool removed;
        lock (_lock)
        {
            removed = _allItems.Remove(item);
            if (removed)
            {
                RefreshVisibleItemsInternal();
            }
        }

        if (removed)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Reset);
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(HasMorePages));
        }

        return removed;
    }

    public void UpdateItem(T oldItem, T newItem)
    {
        if (oldItem == null || newItem == null) return;

        lock (_lock)
        {
            var index = _allItems.IndexOf(oldItem);
            if (index >= 0)
            {
                _allItems[index] = newItem;
                RefreshVisibleItemsInternal();
            }
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _allItems.Clear();
            _visibleItems.Clear();
            _currentPage = 0;
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(VisibleCount));
        OnPropertyChanged(nameof(HasMorePages));
        OnPropertyChanged(nameof(HasPreviousPages));
        OnPropertyChanged(nameof(CurrentPage));
    }

    // Navigation
    public bool LoadNextPage()
    {
        lock (_lock)
        {
            if (!HasMorePages) return false;

            _currentPage++;
            RefreshVisibleItemsInternal();
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(HasMorePages));
        OnPropertyChanged(nameof(HasPreviousPages));
        return true;
    }

    public bool LoadPreviousPage()
    {
        lock (_lock)
        {
            if (_currentPage <= 0) return false;

            _currentPage--;
            RefreshVisibleItemsInternal();
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(HasMorePages));
        OnPropertyChanged(nameof(HasPreviousPages));
        return true;
    }

    public void GoToPage(int page)
    {
        lock (_lock)
        {
            var maxPage = Math.Max(0, (int)Math.Ceiling((double)GetFilteredItems().Count() / _pageSize) - 1);
            _currentPage = Math.Max(0, Math.Min(page, maxPage));
            RefreshVisibleItemsInternal();
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(HasMorePages));
        OnPropertyChanged(nameof(HasPreviousPages));
    }

    // Filtering and sorting
    public void SetFilterPredicate(Func<T, string, bool>? predicate)
    {
        _filterPredicate = predicate;
        RefreshVisibleItems();
    }

    public void SetSortComparer(Func<T, T, int>? comparer)
    {
        _sortComparer = comparer;
        RefreshVisibleItems();
    }

    public void RefreshVisibleItems()
    {
        lock (_lock)
        {
            _currentPage = 0; // Reset to first page when filtering
            RefreshVisibleItemsInternal();
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(VisibleCount));
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(HasMorePages));
        OnPropertyChanged(nameof(HasPreviousPages));
    }

    // Private methods
    private void RefreshVisibleItemsInternal()
    {
        _visibleItems.Clear();

        var filteredItems = GetFilteredItems();
        var sortedItems = _sortComparer != null
            ? filteredItems.OrderBy(x => x, Comparer<T>.Create((x, y) => _sortComparer(x, y)))
            : filteredItems;

        var pagedItems = sortedItems
            .Skip(_currentPage * _pageSize)
            .Take(_pageSize);

        _visibleItems.AddRange(pagedItems);

        OnPropertyChanged(nameof(VisibleCount));
    }

    private IEnumerable<T> GetFilteredItems()
    {
        var items = _allItems.AsEnumerable();

        if (_filterPredicate != null && !string.IsNullOrWhiteSpace(_filterText))
        {
            items = items.Where(item => _filterPredicate(item, _filterText));
        }

        return items;
    }

    private bool ShouldItemBeVisible(T item)
    {
        // Simple check - could be optimized based on current page and filters
        return _filterPredicate == null ||
               string.IsNullOrWhiteSpace(_filterText) ||
               _filterPredicate(item, _filterText);
    }

    // Event helpers
    protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // IDisposable
    public void Dispose()
    {
        lock (_lock)
        {
            _allItems.Clear();
            _visibleItems.Clear();
        }

        CollectionChanged = null;
        PropertyChanged = null;
    }
}
