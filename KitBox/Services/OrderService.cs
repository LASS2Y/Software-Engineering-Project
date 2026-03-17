using System;
using System.Collections.Generic;
using System.Linq;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;
using KitBox.Models.Enums;
using KitBox.Models.Parts;
using KitBox.Services.Interfaces;

namespace KitBox.Services;

/// <summary>
/// Implements the order orchestration logic:
///   - Determines all parts required for a cabinet (from locker specifications)
///   - Checks stock availability
///   - Computes prices
///   - Persists the order with deposit logic when stock is incomplete
///
/// Part composition per locker (from context.md):
///   4 battens (montants verticaux)
///   2 front crossbars  (2 grooves each – for doors)
///   2 back crossbars   (1 groove each – for panel)
///   4 side crossbars   (1 groove each – for panel)
///   2 horizontal panels
///   2 side panels
///   1 back panel
///   [optional] 2 doors + 2 cup handles (not for glass doors)
///
/// Plus 4 angle irons per cabinet (one shared length for all).
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository      _orderRepository;
    private readonly IOrderLineRepository  _orderLineRepository;
    private readonly ICabinetRepository    _cabinetRepository;
    private readonly ILockerRepository     _lockerRepository;
    private readonly IBillRepository       _billRepository;
    private readonly ICustomerRepository   _customerRepository;
    private readonly IPartRepository       _partRepository;
    private readonly IAngleIronCalculatorService _angleIronCalc;
    private readonly ISupplierSelectionService _supplierSelectionService;
    private readonly ISupplierOrderRepository _supplierOrderRepository;

    public OrderService(
        IOrderRepository        orderRepository,
        IOrderLineRepository    orderLineRepository,
        ICabinetRepository      cabinetRepository,
        ILockerRepository       lockerRepository,
        IBillRepository         billRepository,
        ICustomerRepository     customerRepository,
        IPartRepository         partRepository,
        IAngleIronCalculatorService angleIronCalc,
        ISupplierSelectionService supplierSelectionService,
        ISupplierOrderRepository supplierOrderRepository)
    {
        _orderRepository     = orderRepository;
        _orderLineRepository = orderLineRepository;
        _cabinetRepository   = cabinetRepository;
        _lockerRepository    = lockerRepository;
        _billRepository      = billRepository;
        _customerRepository  = customerRepository;
        _partRepository      = partRepository;
        _angleIronCalc       = angleIronCalc;
        _supplierSelectionService = supplierSelectionService;
        _supplierOrderRepository = supplierOrderRepository;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ──────────────────────────────────────────────────────────────────────────

    public OrderPreview PreviewOrder(List<Locker> lockers, string angleIronColor)
    {
        var requirements = BuildRequirements(lockers, angleIronColor);
        double angleIronLength = _angleIronCalc.CalculateAngleIronLength(lockers);

        var availabilities = new List<PartAvailability>();
        decimal total = 0m;
        bool allAvailable = true;

        foreach (var (partType, subType, qty, h, w, d, color, isGlass) in requirements)
        {
            var part = FindPart(partType, subType, h, w, d, color, isGlass);

            bool available;
            decimal price;
            string name;
            string reference;

            if (part == null)
            {
                available = false;
                allAvailable = false;
                price = 0m;
                name = $"{partType} ({subType ?? ""}) {h}×{w}×{d} cm {color}";
                reference = "N/A";
            }
            else
            {
                available = part.StockQuantity >= qty;
                if (!available) allAvailable = false;
                price = part.UnitPrice;
                name = part.Name;
                reference = part.Reference;
            }

            total += price * qty;
            availabilities.Add(new PartAvailability(name, reference, qty,
                                                     part?.StockQuantity ?? 0,
                                                     available, price));
        }

        return new OrderPreview(availabilities, angleIronLength, angleIronColor, total, allAvailable);
    }

    public Order PlaceOrder(Customer customer, List<Locker> lockers,
                            string angleIronColor, decimal depositAmount)
    {
        var preview = PreviewOrder(lockers, angleIronColor);
        DateTime? expectedAvailableDate = null;

        // Persist customer if new
        if (customer.Id == 0)
            _customerRepository.Add(customer);

        var order = new Order
        {
            CustomerId    = customer.Id,
            OrderDate     = DateTime.Today,
            Status        = preview.AllPartsAvailable ? OrderStatus.Available : OrderStatus.PartiallyAvailable,
            Deposit       = preview.AllPartsAvailable ? null : depositAmount,
            AvailableDate = preview.AllPartsAvailable ? null : DateTime.Today.AddDays(14)
        };

        _orderRepository.Add(order);

        // Persist cabinet
        var cabinet = new Cabinet
        {
            OrderId        = order.Id,
            AngleIronColor = angleIronColor
        };
        _cabinetRepository.Add(cabinet);

        // Persist each locker
        foreach (var locker in lockers)
        {
            locker.CabinetId = cabinet.Id;
            _lockerRepository.Add(locker);
        }

        // Persist order lines (deduct stock when available)
        var requirements = BuildRequirements(lockers, angleIronColor);
        foreach (var (partType, subType, qty, h, w, d, color, isGlass) in requirements)
        {
            var part = FindPart(partType, subType, h, w, d, color, isGlass);
            if (part == null) continue;

            var line = new OrderLine
            {
                OrderId   = order.Id,
                PartId    = part.Id,
                Quantity  = qty,
                UnitPrice = part.UnitPrice
            };
            _orderLineRepository.Add(line);

            int stockUsed = Math.Min(part.StockQuantity, qty);
            if (stockUsed > 0)
            {
                int newStock = part.StockQuantity - stockUsed;
                _partRepository.UpdateStock(part.Id, newStock);
                part.StockQuantity = newStock;
            }

            int missingQuantity = qty - stockUsed;
            if (missingQuantity > 0)
            {
                var bestSupplier = _supplierSelectionService.GetBestSupplier(part.Id);
                if (bestSupplier != null)
                {
                    var supplierOrder = new SupplierOrder
                    {
                        CustomerOrderId = order.Id,
                        PartId = part.Id,
                        SupplierId = bestSupplier.SupplierId,
                        Quantity = missingQuantity,
                        UnitCost = bestSupplier.Price,
                        DeliveryDays = bestSupplier.DeliveryDays,
                        OrderedAt = DateTime.Today,
                        ExpectedDeliveryDate = DateTime.Today.AddDays(bestSupplier.DeliveryDays),
                        Status = "Ordered"
                    };

                    _supplierOrderRepository.Add(supplierOrder);

                    if (!expectedAvailableDate.HasValue || supplierOrder.ExpectedDeliveryDate > expectedAvailableDate.Value)
                    {
                        expectedAvailableDate = supplierOrder.ExpectedDeliveryDate;
                    }
                }
            }
        }

        if (!preview.AllPartsAvailable)
        {
            order.AvailableDate = expectedAvailableDate ?? DateTime.Today.AddDays(14);
            _orderRepository.Update(order);
        }

        return order;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the full list of required parts for the given lockers + angle irons.
    /// Returns tuples (partType, subType, quantity, h, w, d, color, isGlass?).
    ///
    /// Dimension mapping matches the real catalog (kitboxparts.csv):
    ///   - Battens: height only, no color (universal)
    ///   - Front/Back crossbars: width only, no color (universal)
    ///   - Side crossbars: depth only, no color (universal)
    ///   - Horizontal panels: width × depth, locker color
    ///   - Side panels: height × depth, locker color
    ///   - Back panels: height × width, locker color
    ///   - Doors: height × width (= locker width), door color
    ///   - Handles: no dimensions, no color (universal)
    ///   - Angle irons: height = total length, angle iron color
    /// </summary>
    private List<(string partType, string? subType, int qty,
                  double h, double w, double d, string color, bool? isGlass)>
        BuildRequirements(List<Locker> lockers, string angleIronColor)
    {
        var reqs = new List<(string, string?, int, double, double, double, string, bool?)>();

        foreach (var l in lockers)
        {
            // 4 battens – matched by height only (no color in real catalog)
            reqs.Add(("Batten", null, 4, l.Height, 0, 0, "", null));

            // 2 front crossbars (2 grooves) – matched by width = locker width (no color)
            reqs.Add(("Crossbar", "Front", 2, 0, l.Width, 0, "", null));

            // 2 back crossbars (1 groove) – matched by width = locker width (no color)
            reqs.Add(("Crossbar", "Back", 2, 0, l.Width, 0, "", null));

            // 4 side crossbars (1 groove) – matched by depth = locker depth (no color)
            reqs.Add(("Crossbar", "Side", 4, 0, 0, l.Depth, "", null));

            // 2 horizontal panels – matched by width × depth, locker color
            reqs.Add(("Panel", "Horizontal", 2, 0, l.Width, l.Depth, l.Color, null));

            // 2 side panels – matched by height × depth, locker color
            reqs.Add(("Panel", "Side", 2, l.Height, 0, l.Depth, l.Color, null));

            // 1 back panel – matched by height × width, locker color
            reqs.Add(("Panel", "Back", 1, l.Height, l.Width, 0, l.Color, null));

            if (l.HasDoors)
            {
                bool isGlass = string.Equals(l.DoorColor, "Glass", StringComparison.OrdinalIgnoreCase);

                // 2 doors – matched by height × width (width = locker width in catalog)
                reqs.Add(("Door", null, 2, l.Height, l.Width, 0, l.DoorColor!, isGlass));

                // 2 cup handles – only for non-glass doors (no color, no dimensions)
                if (!isGlass)
                    reqs.Add(("Handle", null, 2, 0, 0, 0, "", null));
            }
        }

        // 4 angle irons – one shared length = sum of TotalHeights
        double angleLength = _angleIronCalc.CalculateAngleIronLength(lockers);
        reqs.Add(("AngleIron", null, 4, angleLength, 0, 0, angleIronColor, null));

        return reqs;
    }

    /// <summary>
    /// Finds the closest matching part in the DB.
    /// Matches by part_type, subtype, and non-zero dimensions.
    /// </summary>
    private Part? FindPart(string partType, string? subType,
                           double h, double w, double d, string color, bool? isGlass)
    {
        var candidates = _partRepository.GetByType(partType);

        return candidates.FirstOrDefault(p =>
        {
            // Match subtype (panel_type / crossbar_type)
            if (p is Panel panel && subType != null)
                if (panel.Type.ToString() != subType) return false;
            if (p is Crossbar cb && subType != null)
                if (cb.Type.ToString() != subType) return false;

            // Match non-zero dimensions with small tolerance
            const double tol = 0.01;
            if (h > 0 && Math.Abs(p.Height - h) > tol) return false;
            if (w > 0 && Math.Abs(p.Width  - w) > tol) return false;
            if (d > 0 && Math.Abs(p.Depth  - d) > tol) return false;

            // Match color
            if (!string.IsNullOrEmpty(color) &&
                !string.Equals(p.Color, color, StringComparison.OrdinalIgnoreCase))
                return false;

            // Match glass flag for doors
            if (isGlass.HasValue && p is Door door)
                if (door.IsGlass != isGlass.Value) return false;

            return true;
        });
    }
}
