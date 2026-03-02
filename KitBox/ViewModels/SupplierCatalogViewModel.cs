using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.Models;

namespace KitBox.ViewModels;

/// <summary>Wraps a single SupplierPart row for inline editing by the secretary.</summary>
public partial class SupplierCatalogItemViewModel : ViewModelBase
{
    private readonly SupplierCatalogViewModel _parent;

    public SupplierCatalogItemViewModel(SupplierPart sp, SupplierCatalogViewModel parent)
    {
        _parent       = parent;
        SupplierPart  = sp;
        _editPrice    = sp.Price;
        _editDelivery = sp.DeliveryDays;
    }

    public SupplierPart SupplierPart { get; }

    // Read-only display fields
    public string SupplierName  => SupplierPart.Supplier?.Name ?? $"Supplier #{SupplierPart.SupplierId}";
    public string PartName      => SupplierPart.PartName;
    public string PartReference => SupplierPart.PartReference;
    public string PartType      => SupplierPart.PartType;
    public decimal CurrentPrice => SupplierPart.Price;
    public int CurrentDelivery  => SupplierPart.DeliveryDays;

    // Editable fields
    [ObservableProperty] private decimal _editPrice;
    [ObservableProperty] private int     _editDelivery;
    [ObservableProperty] private bool    _isSaved;

    public bool HasChanges => EditPrice != SupplierPart.Price || EditDelivery != SupplierPart.DeliveryDays;

    [RelayCommand]
    public void SavePrice()
    {
        try
        {
            SupplierPart.Price        = EditPrice;
            SupplierPart.DeliveryDays = EditDelivery;
            _parent.Main.Services.SupplierPartRepository.Update(SupplierPart);

            OnPropertyChanged(nameof(CurrentPrice));
            OnPropertyChanged(nameof(CurrentDelivery));
            OnPropertyChanged(nameof(HasChanges));
            IsSaved = true;
            _parent.SetStatus($"Updated {PartName} – {SupplierName}: €{EditPrice}, {EditDelivery} days");
        }
        catch (Exception ex)
        {
            _parent.SetStatus($"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    public void DeleteEntry()
    {
        try
        {
            _parent.Main.Services.SupplierPartRepository.Delete(SupplierPart.Id);
            _parent.RemoveItem(this);
            _parent.SetStatus($"Deleted {PartName} from {SupplierName}");
        }
        catch (Exception ex)
        {
            _parent.SetStatus($"Error: {ex.Message}");
        }
    }
}

/// <summary>
/// Secretary view-model: displays the full supplier catalog (supplier × part)
/// and allows inline price / delivery-time editing.
/// Corresponds to context.md: "Une secrétaire met régulièrement à jour les prix
/// à partir des catalogues fournisseurs."
/// </summary>
public partial class SupplierCatalogViewModel : ViewModelBase
{
    public MainViewModel Main { get; }

    public SupplierCatalogViewModel(MainViewModel main)
    {
        Main = main;
        LoadSuppliers();
        Refresh();
    }

    // ── Observable state ─────────────────────────────────────────────────────

    public ObservableCollection<SupplierCatalogItemViewModel> CatalogItems { get; } = new();
    public ObservableCollection<Supplier> Suppliers { get; } = new();

    [ObservableProperty] private int     _totalEntries;
    [ObservableProperty] private string  _statusMessage = string.Empty;
    [ObservableProperty] private string  _loadError     = string.Empty;

    // Filter
    [ObservableProperty] private Supplier? _selectedSupplier;
    [ObservableProperty] private string    _searchText = string.Empty;

    partial void OnSelectedSupplierChanged(Supplier? value) => ApplyFilter();
    partial void OnSearchTextChanged(string value)          => ApplyFilter();

    // ── Add-entry form fields ────────────────────────────────────────────────

    [ObservableProperty] private Supplier? _newSupplier;
    [ObservableProperty] private int       _newPartId;
    [ObservableProperty] private decimal   _newPrice;
    [ObservableProperty] private int       _newDeliveryDays = 5;
    [ObservableProperty] private bool      _isAddFormVisible;

    // ── Private full list for filtering ──────────────────────────────────────

    private System.Collections.Generic.List<SupplierCatalogItemViewModel>? _allItems;

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    public void Refresh()
    {
        LoadError = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var entries = Main.Services.SupplierPartRepository.GetAll();
            _allItems = entries.Select(sp => new SupplierCatalogItemViewModel(sp, this)).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            LoadError = $"Could not load supplier catalog: {ex.Message}\nMake sure the database is running.";
        }
    }

    [RelayCommand]
    public void ToggleAddForm() => IsAddFormVisible = !IsAddFormVisible;

    [RelayCommand]
    public void AddEntry()
    {
        try
        {
            if (NewSupplier == null) { SetStatus("Please select a supplier."); return; }
            if (NewPartId <= 0)      { SetStatus("Please enter a valid Part ID."); return; }
            if (NewPrice <= 0)       { SetStatus("Price must be positive."); return; }

            var sp = new SupplierPart
            {
                SupplierId   = NewSupplier.Id,
                PartId       = NewPartId,
                Price        = NewPrice,
                DeliveryDays = NewDeliveryDays
            };

            Main.Services.SupplierPartRepository.Add(sp);
            SetStatus($"Added entry #{sp.Id} successfully.");
            IsAddFormVisible = false;
            Refresh();
        }
        catch (Exception ex)
        {
            SetStatus($"Error adding entry: {ex.Message}");
        }
    }

    [RelayCommand]
    public void ClearFilter()
    {
        SelectedSupplier = null;
        SearchText = string.Empty;
    }

    [RelayCommand]
    public void Back() => Main.NavigateTo(new StartPageViewModel(Main));

    // ── Helpers ──────────────────────────────────────────────────────────────

    public void SetStatus(string msg) => StatusMessage = msg;

    public void RemoveItem(SupplierCatalogItemViewModel item)
    {
        _allItems?.Remove(item);
        CatalogItems.Remove(item);
        TotalEntries = CatalogItems.Count;
    }

    private void LoadSuppliers()
    {
        try
        {
            Suppliers.Clear();
            foreach (var s in Main.Services.SupplierRepository.GetAll())
                Suppliers.Add(s);
        }
        catch { /* handled in Refresh */ }
    }

    private void ApplyFilter()
    {
        if (_allItems == null) return;

        CatalogItems.Clear();

        var filtered = _allItems.AsEnumerable();

        if (SelectedSupplier != null)
            filtered = filtered.Where(i => i.SupplierPart.SupplierId == SelectedSupplier.Id);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(i =>
                i.PartName.ToLowerInvariant().Contains(s) ||
                i.PartReference.ToLowerInvariant().Contains(s) ||
                i.PartType.ToLowerInvariant().Contains(s));
        }

        foreach (var item in filtered)
            CatalogItems.Add(item);

        TotalEntries = CatalogItems.Count;
    }
}
