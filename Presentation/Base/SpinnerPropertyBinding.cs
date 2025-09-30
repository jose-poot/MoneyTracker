using Android.Content;
using Android.Widget;
using CommunityToolkit.Mvvm.ComponentModel;
using MoneyTracker.Presentation.Base.Enums;
using MoneyTracker.Presentation.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace MoneyTracker.Presentation.Base;

internal interface ISpinnerBinding<TItem>
{
    void SetItemsSource(Expression<Func<object, IEnumerable<TItem>>> itemsExpression);

    void SetAdapterFactory(Func<Context, IList<TItem>, SpinnerAdapter> adapterFactory);
}

internal class SpinnerPropertyBinding<TViewModel, TItem> : IBinding, IBindingModeAware, ISpinnerBinding<TItem>
    where TViewModel : ObservableObject
{
    private readonly Spinner _spinner;
    private readonly TViewModel _viewModel;
    private readonly Expression<Func<object, TItem>> _selectedItemExpression;
    private readonly Func<object, TItem> _selectedItemGetter;
    private readonly Action<object, TItem>? _selectedItemSetter;
    private readonly string _selectedItemPropertyName;

    private Expression<Func<object, IEnumerable<TItem>>>? _itemsSourceExpression;
    private Func<object, IEnumerable<TItem>>? _itemsSourceGetter;
    private string? _itemsSourcePropertyName;
    private Func<Context, IList<TItem>, SpinnerAdapter>? _adapterFactory;
    private SpinnerAdapter? _adapter;
    private IList<TItem> _currentItems = new List<TItem>();
    private INotifyCollectionChanged? _collectionChangedSource;
    private bool _isUpdatingSelection;

    public BindingMode Mode { get; set; } = BindingMode.OneWay;

    public SpinnerPropertyBinding(Spinner spinner, TViewModel viewModel,
        Expression<Func<object, TItem>> selectedItemExpression)
    {
        _spinner = spinner;
        _viewModel = viewModel;
        _selectedItemExpression = selectedItemExpression;
        _selectedItemGetter = selectedItemExpression.Compile();
        _selectedItemSetter = CreateSetter(selectedItemExpression);
        _selectedItemPropertyName = GetPropertyName(selectedItemExpression);
    }

    public void SetItemsSource(Expression<Func<object, IEnumerable<TItem>>> itemsExpression)
    {
        _itemsSourceExpression = itemsExpression;
        _itemsSourceGetter = itemsExpression.Compile();
        _itemsSourcePropertyName = GetPropertyName(itemsExpression);
    }

    public void SetAdapterFactory(Func<Context, IList<TItem>, SpinnerAdapter> adapterFactory)
    {
        _adapterFactory = adapterFactory;
    }

    public void Apply()
    {
        if (_itemsSourceGetter == null)
        {
            throw new InvalidOperationException("Spinner binding requires an ItemsSource to be configured.");
        }

        RefreshItemsSource();

        if (Mode != BindingMode.Source)
        {
            UpdateSpinnerSelection();
        }

        _viewModel.PropertyChanged += OnPropertyChanged;
        _spinner.ItemSelected += OnItemSelected;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == null || e.PropertyName == _selectedItemPropertyName)
        {
            if (Mode != BindingMode.Source)
            {
                UpdateSpinnerSelection();
            }
        }

        if (_itemsSourcePropertyName != null && (e.PropertyName == null || e.PropertyName == _itemsSourcePropertyName))
        {
            RefreshItemsSource();
            if (Mode != BindingMode.Source)
            {
                UpdateSpinnerSelection();
            }
        }
    }

    private void RefreshItemsSource()
    {
        DetachCollectionChanged();

        var itemsEnumerable = EvaluateItemsSource();
        if (itemsEnumerable is INotifyCollectionChanged collection)
        {
            _collectionChangedSource = collection;
            _collectionChangedSource.CollectionChanged += OnCollectionChanged;
        }

        _currentItems = itemsEnumerable?.ToList() ?? new List<TItem>();
        RebuildAdapter();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateItemsFromCollection();
    }

    private void UpdateItemsFromCollection()
    {
        var itemsEnumerable = EvaluateItemsSource();
        _currentItems = itemsEnumerable?.ToList() ?? new List<TItem>();
        UpdateAdapterItems();
    }

    private IEnumerable<TItem> EvaluateItemsSource()
    {
        if (_itemsSourceGetter == null)
        {
            return Enumerable.Empty<TItem>();
        }

        var items = _itemsSourceGetter(_viewModel);
        return items ?? Enumerable.Empty<TItem>();
    }

    private void RebuildAdapter()
    {
        _adapter?.Dispose();

        var itemsSnapshot = _currentItems.ToList();
        _adapter = _adapterFactory != null
            ? _adapterFactory(_spinner.Context, itemsSnapshot)
            : CreateDefaultAdapter(itemsSnapshot);

        _spinner.Adapter = _adapter;
    }

    private SpinnerAdapter CreateDefaultAdapter(IList<TItem> items)
    {
        var displayItems = items.Select(item => item?.ToString() ?? string.Empty).ToList();
        var arrayAdapter = new ArrayAdapter<string>(_spinner.Context, Android.Resource.Layout.SimpleSpinnerItem, displayItems);
        arrayAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
        return arrayAdapter;
    }

    private void UpdateAdapterItems()
    {
        if (_adapterFactory == null && _adapter is ArrayAdapter<string> arrayAdapter)
        {
            arrayAdapter.Clear();
            foreach (var item in _currentItems)
            {
                arrayAdapter.Add(item?.ToString() ?? string.Empty);
            }
            arrayAdapter.NotifyDataSetChanged();
        }
        else
        {
            RebuildAdapter();
        }
    }

    private void UpdateSpinnerSelection()
    {
        var selected = _selectedItemGetter(_viewModel);
        var index = IndexOfItem(selected);

        if (index < 0)
        {
            return;
        }

        if (_spinner.SelectedItemPosition == index)
        {
            return;
        }

        try
        {
            _isUpdatingSelection = true;
            _spinner.SetSelection(index);
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    private int IndexOfItem(TItem item)
    {
        if (_currentItems.Count == 0)
        {
            return -1;
        }

        var comparer = EqualityComparer<TItem>.Default;
        for (var i = 0; i < _currentItems.Count; i++)
        {
            if (comparer.Equals(_currentItems[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    private void OnItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        if (Mode == BindingMode.OneWay)
        {
            return;
        }

        if (_isUpdatingSelection)
        {
            return;
        }

        if (_selectedItemSetter == null)
        {
            return;
        }

        var position = e.Position;
        if (position < 0 || position >= _currentItems.Count)
        {
            return;
        }

        var selectedItem = _currentItems[position];
        _selectedItemSetter(_viewModel, selectedItem);
    }

    private void DetachCollectionChanged()
    {
        if (_collectionChangedSource != null)
        {
            _collectionChangedSource.CollectionChanged -= OnCollectionChanged;
            _collectionChangedSource = null;
        }
    }

    public void Dispose()
    {
        _viewModel.PropertyChanged -= OnPropertyChanged;
        _spinner.ItemSelected -= OnItemSelected;
        DetachCollectionChanged();

        if (_spinner.Adapter == _adapter)
        {
            _spinner.Adapter = null;
        }

        _adapter?.Dispose();
        _adapter = null;
    }

    private string GetPropertyName<T>(Expression<Func<object, T>> expression)
    {
        if (expression.Body is MemberExpression member)
        {
            return member.Member.Name;
        }

        throw new ArgumentException("Expression must be a property access");
    }

    private Action<object, TItem>? CreateSetter(Expression<Func<object, TItem>> expression)
    {
        if (expression.Body is not MemberExpression member)
        {
            return null;
        }

        var param = Expression.Parameter(typeof(object));
        var valueParam = Expression.Parameter(typeof(TItem));
        var convertedParam = Expression.Convert(param, member.Expression!.Type);
        var assign = Expression.Assign(Expression.Property(convertedParam, member.Member.Name), valueParam);

        return Expression.Lambda<Action<object, TItem>>(assign, param, valueParam).Compile();
    }
}
