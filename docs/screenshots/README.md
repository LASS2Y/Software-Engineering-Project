# Screenshots

This folder holds the application screenshots referenced from `PROJECT_GUIDE.md`.

The guide already includes Markdown image links pointing at the file names below, so once you drop the PNGs here the document renders the images automatically — no edits to the guide are needed.

## How to capture

1. Start MariaDB and apply `schema.sql` + `seed.sql`.
2. Run the app from the `KitBox/` folder: `dotnet run`.
3. Navigate to each screen listed below and capture the window. The recommended viewport is **1600 × 900** (the size the application is laid out for).
4. Save each capture as PNG (no scaling) using the exact file name shown in the **File** column. Keep file names lowercase and use hyphens (no spaces).

## Expected files

| # | File | Screen / how to reach it |
|---|---|---|
| 1 | `01-welcome.png` | App launch — the Welcome screen with the big "KitBox" title, the "Click to continue" button and the small key icon (bottom-right). |
| 2 | `02-employee-login.png` | Welcome → click the key icon → Customer Selection (employee login). Show the form blank (no error banner). |
| 3 | `03-employee-inscription.png` | From the legacy Secretary menu → Register employee. Show the empty form. |
| 4a | `04-secretary-menu-expanded.png` | After login → the new Secretary menu with the **sidebar expanded** (default state), Order history tab selected. |
| 4b | `04b-secretary-menu-collapsed.png` | Same view but with the sidebar **collapsed** (after clicking the hamburger icon). |
| 5 | `05-cabinet-configuration.png` | Welcome → Click to continue → Cabinet builder. Add 2 lockers with different dimensions so the live 3D preview is visible. |
| 6 | `06-order-summary.png` | From the cabinet builder → Preview Order. Pick a configuration whose stock is **partially available** so the deposit area and the green/red badges both appear. |
| 7 | `07-order-history.png` | Sidebar → Order history. After placing at least one order so the table isn't empty. |
| 8 | `08-stock-management.png` | Sidebar → Stock management. Make sure a few parts are below their minimum so the orange highlight and the "Order from best supplier" button are visible. |
| 9 | `09-supplier-catalog.png` | Sidebar → Supplier catalog. Optionally expand the "add new entry" form for the screenshot. |
| 10 | `10-supplier-order-tracking.png` | Sidebar → Order tracking. Easiest way to populate this list: place a partial-availability customer order (step 6) or click "Order from best supplier" on a low-stock part (step 8). |
| 11 | `11-dashboard.png` | Sidebar → Dashboard. The KPI cards + chart placeholders. |

## Conventions

- Always use the same window size across screenshots — readers compare them side-by-side.
- Hide developer tools / debug overlays before capturing.
- Crop to the application window (no OS chrome).
- Keep the file size reasonable (< 500 KB per screenshot is usually enough); use a PNG optimiser like `pngquant` if needed.
