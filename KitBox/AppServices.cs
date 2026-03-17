using KitBox.DataAccess;
using KitBox.DataAccess.Interfaces;
using KitBox.DataAccess.Repositories;
using KitBox.Services;
using KitBox.Services.Interfaces;

namespace KitBox;

/// <summary>
/// Holds all services and repositories for the application.
/// Passed through the ViewModel hierarchy to avoid globals.
/// </summary>
public class AppServices
{
    // ── Repositories ──────────────────────────────────────────────────────────
    public ICustomerRepository      CustomerRepository   { get; }
    public IOrderRepository         OrderRepository      { get; }
    public IOrderLineRepository     OrderLineRepository  { get; }
    public IBillRepository          BillRepository       { get; }
    public ICabinetRepository       CabinetRepository    { get; }
    public ILockerRepository        LockerRepository     { get; }
    public IPartRepository          PartRepository       { get; }
    public ISupplierRepository      SupplierRepository   { get; }
    public ISupplierPartRepository  SupplierPartRepository { get; }

    // ── Services ──────────────────────────────────────────────────────────────
    public ICatalogService              CatalogService           { get; }
    public ILockerValidationService     LockerValidationService  { get; }
    public IAngleIronCalculatorService  AngleIronCalculator      { get; }
    public ISupplierSelectionService    SupplierSelectionService { get; }
    public IStockService                StockService             { get; }
    public IOrderService                OrderService             { get; }

    public AppServices(DatabaseConnection db)
    {
        // Repositories
        CustomerRepository      = new CustomerRepository(db);
        OrderRepository         = new OrderRepository(db);
        OrderLineRepository     = new OrderLineRepository(db);
        BillRepository          = new BillRepository(db);
        CabinetRepository       = new CabinetRepository(db);
        LockerRepository        = new LockerRepository(db);
        PartRepository          = new PartRepository(db);
        SupplierRepository      = new SupplierRepository(db);
        SupplierPartRepository  = new SupplierPartRepository(db);

        // Services
        CatalogService          = new CatalogService();
        AngleIronCalculator     = new AngleIronCalculatorService(CatalogService);
        LockerValidationService = new LockerValidationService(CatalogService);
        SupplierSelectionService = new SupplierSelectionService(SupplierPartRepository);
        StockService            = new StockService(
                        PartRepository,
                        OrderLineRepository,
                        SupplierSelectionService);
        OrderService            = new OrderService(
                                    OrderRepository, OrderLineRepository,
                                    CabinetRepository, LockerRepository,
                                    BillRepository, CustomerRepository,
                                    PartRepository, AngleIronCalculator);
    }
}
