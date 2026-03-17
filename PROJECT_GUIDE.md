# KitBox — Guide Complet du Projet

> Application de digitalisation du processus de commande d'armoires en kit pour la société KitBox.  
> **Stack** : C# / .NET 9.0 / Avalonia UI 11.3.6 / MariaDB / CommunityToolkit.Mvvm 8.2.1

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Prérequis & Installation](#2-prérequis--installation)
3. [Architecture technique](#3-architecture-technique)
4. [Base de données](#4-base-de-données)
5. [Couche Models](#5-couche-models)
6. [Couche DataAccess](#6-couche-dataaccess)
7. [Couche Services](#7-couche-services)
8. [Couche ViewModels](#8-couche-viewmodels)
9. [Couche Views](#9-couche-views)
10. [Flux de navigation](#10-flux-de-navigation)
11. [Processus métier détaillés](#11-processus-métier-détaillés)
12. [Ce qui a été implémenté](#12-ce-qui-a-été-implémenté)
13. [Ce qu'il reste à faire](#13-ce-quil-reste-à-faire)

---

## 1. Vue d'ensemble

KitBox est une application desktop cross-platform permettant de :

- **Clients** : configurer une armoire (1 à 7 casiers), prévisualiser la disponibilité des pièces, passer commande.
- **Secrétaire** : gérer le catalogue fournisseurs (prix, délais), consulter/modifier le stock, suivre l'historique des commandes (statuts, facturation).
- **Propriétaire/Magasinier** : gérer les quantités en stock avec alertes de stock bas.

L'application s'ouvre sur un écran de sélection de rôle (**Client** ou **Secrétaire**) puis redirige vers le flux approprié.

---

## 2. Prérequis & Installation

### 2.1 Outils requis

| Outil | Version minimum |
|---|---|
| .NET SDK | 9.0 |
| MariaDB (ou MySQL) | 10.6+ |
| IDE | Visual Studio 2022, Rider, ou VS Code avec C# Dev Kit |

### 2.2 Configuration de la base de données

1. Installer MariaDB et créer un serveur accessible.
2. Exécuter le fichier `schema.sql` (à la racine du repo) pour créer la base `kitbox` et ses 8 tables.
3. Exécuter le fichier `seed.sql` pour insérer les données initiales (fournisseurs, pièces, catalogue fournisseur).

```bash
mysql -u root -p < schema.sql
mysql -u root -p < seed.sql
```

### 2.3 Configuration du fichier .env

L'application lit ses identifiants de connexion depuis un fichier `.env` placé dans le dossier `KitBox/`.

1. Copier `.env.example` → `.env` :
   ```bash
   cp KitBox/.env.example KitBox/.env
   ```
2. Éditer `KitBox/.env` avec vos paramètres :
   ```dotenv
   DB_SERVER=localhost
   DB_NAME=kitbox
   DB_USER=root
   DB_PASSWORD=votre_mot_de_passe
   DB_PORT=3306
   ```

> **Note** : Le fichier `.env` est ignoré par Git (`.gitignore`). Chaque développeur doit créer le sien.

### 2.4 Lancer l'application

```bash
cd KitBox
dotnet run
```

L'application démarre en plein écran (1600×900) sur la page de sélection de rôle.

---

## 3. Architecture technique

### 3.1 Patron MVVM (Model-View-ViewModel)

```
┌──────────────────────────────────────────────────┐
│                    Views (.axaml)                 │
│          Affichage + Bindings XAML                │
└────────────────────┬─────────────────────────────┘
                     │ DataBinding
┌────────────────────▼─────────────────────────────┐
│               ViewModels (.cs)                    │
│       Logique UI + Commands + Navigation          │
└────────────────────┬─────────────────────────────┘
                     │ Appels
┌────────────────────▼─────────────────────────────┐
│                Services (.cs)                     │
│        Logique métier (validation, calcul,        │
│        gestion stock, commandes)                  │
└────────────────────┬─────────────────────────────┘
                     │ Appels
┌────────────────────▼─────────────────────────────┐
│              DataAccess (.cs)                     │
│     Repositories (CRUD via MySqlConnector)        │
└────────────────────┬─────────────────────────────┘
                     │ SQL
┌────────────────────▼─────────────────────────────┐
│                MariaDB                            │
│          Base de données `kitbox`                 │
└──────────────────────────────────────────────────┘
```

### 3.2 Principes SOLID appliqués

| Principe | Application |
|---|---|
| **SRP** | Chaque classe a une responsabilité unique (ex. `OrderService` = logique commande, `PartRepository` = accès données pièces) |
| **OCP** | Hiérarchie `Part` abstraite → facilement extensible avec de nouveaux types (étagères, tiroirs) sans modifier le code existant |
| **LSP** | Toutes les sous-classes de `Part` (Panel, Crossbar, Batten, etc.) sont interchangeables |
| **ISP** | Interfaces séparées par domaine (`IOrderRepository`, `IPartRepository`, etc.) |
| **DIP** | Services dépendent d'interfaces, pas d'implémentations concrètes |

### 3.3 Fichiers d'infrastructure

| Fichier | Rôle |
|---|---|
| `Program.cs` | Point d'entrée, initialise Avalonia |
| `App.axaml.cs` | Crée la `MainView` avec `MainViewModel` comme DataContext |
| `ViewLocator.cs` | Résolution automatique des Views depuis les ViewModels (remplace "ViewModel" par "View" dans le nom de type) |
| `EnvConfig.cs` | Lecture du fichier `.env` (clé=valeur par ligne) |
| `AppServices.cs` | Composition root : instancie tous les repositories et services |

### 3.4 Dépendances NuGet

| Package | Version | Usage |
|---|---|---|
| `Avalonia` | 11.3.6 | Framework UI cross-platform |
| `Avalonia.Desktop` | 11.3.6 | Support desktop natif |
| `Avalonia.Themes.Fluent` | 11.3.6 | Thème Fluent Design |
| `Avalonia.Fonts.Inter` | 11.3.6 | Police Inter |
| `CommunityToolkit.Mvvm` | 8.2.1 | `[ObservableProperty]`, `[RelayCommand]`, `ObservableObject` |
| `MySqlConnector` | 2.4.0 | Connexion MariaDB/MySQL |

---

## 4. Base de données

### 4.1 Schéma (8 tables)

```
customer ──1:N──> customer_order ──1:N──> order_line ──N:1──> part
                       │                                        │
                       │ 0..1                                   │
                       ▼                                        │
                     bill                                       │
                       │                                  N:N (supplier_part)
                       │                                        │
customer_order ──1:1──> cabinet ──1:N──> locker            supplier
```

#### Table `customer`
| Colonne | Type | Contrainte |
|---|---|---|
| `id` | INT AUTO_INCREMENT | PK |
| `first_name` | VARCHAR(100) | NOT NULL |
| `last_name` | VARCHAR(100) | NOT NULL |
| `email` | VARCHAR(255) | UNIQUE, NOT NULL |
| `phone` | VARCHAR(20) | |

#### Table `bill`
| Colonne | Type | Contrainte |
|---|---|---|
| `id` | INT AUTO_INCREMENT | PK |
| `emission_date` | DATE | NOT NULL |
| `amount` | DECIMAL(10,2) | NOT NULL |

#### Table `customer_order`
| Colonne | Type | Contrainte |
|---|---|---|
| `id` | INT AUTO_INCREMENT | PK |
| `customer_id` | INT | FK → customer |
| `bill_id` | INT | FK → bill (nullable) |
| `order_date` | DATE | NOT NULL |
| `deposit` | DECIMAL(10,2) | nullable |
| `available_date` | DATE | nullable |
| `status` | ENUM('Pending','PartiallyAvailable','Available','Delivered','Cancelled') | NOT NULL |

#### Table `cabinet`
| Colonne | Type | Contrainte |
|---|---|---|
| `id` | INT AUTO_INCREMENT | PK |
| `order_id` | INT | FK → customer_order (CASCADE) |
| `angle_iron_color` | VARCHAR(50) | |

#### Table `locker`
| Colonne | Type | Contrainte |
|---|---|---|
| `id` | INT AUTO_INCREMENT | PK |
| `cabinet_id` | INT | FK → cabinet (CASCADE) |
| `height` | DOUBLE | NOT NULL |
| `width` | DOUBLE | NOT NULL |
| `depth` | DOUBLE | NOT NULL |
| `color` | VARCHAR(50) | |
| `has_doors` | BOOLEAN | DEFAULT FALSE |
| `door_color` | VARCHAR(50) | nullable |

#### Table `part` (Single Table Inheritance)
| Colonne | Type | Contrainte |
|---|---|---|
| `id` | INT AUTO_INCREMENT | PK |
| `reference` | VARCHAR(50) | UNIQUE, NOT NULL |
| `name` | VARCHAR(255) | NOT NULL |
| `part_type` | ENUM('Panel','Crossbar','Batten','AngleIron','Door','Handle') | NOT NULL |
| `height` | DOUBLE | |
| `width` | DOUBLE | |
| `depth` | DOUBLE | |
| `color` | VARCHAR(50) | |
| `unit_price` | DECIMAL(10,2) | NOT NULL |
| `stock_quantity` | INT | DEFAULT 0 |
| `minimum_stock` | INT | DEFAULT 5 |
| `panel_type` | ENUM('Horizontal','Side','Back') | nullable (Panel uniquement) |
| `crossbar_type` | ENUM('Front','Back','Side') | nullable (Crossbar uniquement) |
| `groove_count` | INT | nullable (Crossbar uniquement) |
| `standard_length` | DOUBLE | nullable (AngleIron uniquement) |
| `is_glass` | BOOLEAN | nullable (Door uniquement) |

#### Table `order_line`
| Colonne | Type | Contrainte |
|---|---|---|
| `id` | INT AUTO_INCREMENT | PK |
| `order_id` | INT | FK → customer_order (CASCADE) |
| `part_id` | INT | FK → part |
| `quantity` | INT | NOT NULL |
| `unit_price` | DECIMAL(10,2) | NOT NULL |

#### Table `supplier`
| Colonne | Type | Contrainte |
|---|---|---|
| `id` | INT AUTO_INCREMENT | PK |
| `name` | VARCHAR(255) | NOT NULL |
| `contact_email` | VARCHAR(255) | |
| `phone` | VARCHAR(20) | |

#### Table `supplier_part`
| Colonne | Type | Contrainte |
|---|---|---|
| `id` | INT AUTO_INCREMENT | PK |
| `supplier_id` | INT | FK → supplier (CASCADE) |
| `part_id` | INT | FK → part (CASCADE) |
| `price` | DECIMAL(10,2) | NOT NULL |
| `delivery_days` | INT | NOT NULL |
| | | UNIQUE(`supplier_id`, `part_id`) |

### 4.2 Index de performance

Des index sont créés sur : `customer_order(customer_id)`, `customer_order(status)`, `cabinet(order_id)`, `locker(cabinet_id)`, `order_line(order_id)`, `order_line(part_id)`, `part(part_type)`, `part(stock_quantity, minimum_stock)`, `supplier_part(part_id)`, `supplier_part(price, delivery_days)`.

### 4.3 Données initiales (seed.sql)

- **3 fournisseurs** : PanelPro NV (panneaux+montants), SteelFix Europe (acier+quincaillerie), KitParts Wholesale (généraliste)
- **~100 pièces** : 10 montants, 24 traverses, 39 panneaux, 10 cornières, 13 portes, 4 poignées
- **Catalogue fournisseur** : PanelPro à 85% du prix de vente, SteelFix à 82-88%, KitParts à 90% (7j de délai)

---

## 5. Couche Models

### 5.1 Enums

| Enum | Valeurs | Usage |
|---|---|---|
| `OrderStatus` | Pending, PartiallyAvailable, Available, Delivered, Cancelled | Statut de commande |
| `PanelType` | Horizontal, Side, Back | Type de panneau |
| `CrossbarType` | Front, Back, Side | Type de traverse |

### 5.2 Hiérarchie Part (héritage)

```
         Part (abstract)
        ┌──┼──┬──┬──┬──┐
     Panel Crossbar Batten AngleIron Door Handle
```

| Classe | Propriétés supplémentaires | Rôle dans un casier |
|---|---|---|
| `Part` (base) | Id, Reference, Name, Height, Width, Depth, Color, UnitPrice, StockQuantity, MinimumStock | — |
| `Panel` | Type (PanelType) | 2 horizontaux + 2 latéraux + 1 arrière par casier |
| `Crossbar` | Type (CrossbarType), GrooveCount | 2 avant + 2 arrière + 4 latérales par casier |
| `Batten` | — | 4 montants verticaux par casier |
| `AngleIron` | StandardLength | 4 par armoire (longueur = somme hauteurs casiers) |
| `Door` | IsGlass | 2 portes optionnelles par casier |
| `Handle` | — | 2 poignées si portes (pas pour verre) |

### 5.3 Entités métier

| Classe | Propriétés principales | Relations |
|---|---|---|
| `Customer` | Id, FirstName, LastName, Email, Phone | → Orders (liste) |
| `Order` | Id, CustomerId, BillId?, OrderDate, Deposit?, AvailableDate?, Status | → Customer, Bill, Lines, Cabinets |
| `OrderLine` | Id, OrderId, PartId, Quantity, UnitPrice | TotalPrice = Quantity × UnitPrice |
| `Cabinet` | Id, OrderId, AngleIronColor | → Lockers ; AngleIronLength (calculé) ; Max 7 casiers |
| `Locker` | Id, CabinetId, Height, Width, Depth, Color, HasDoors, DoorColor? | TotalHeight = Height + 2×2cm |
| `Bill` | Id, EmissionDate, Amount | Facture après livraison |
| `Supplier` | Id, Name, ContactEmail, Phone | → SupplierParts |
| `SupplierPart` | Id, SupplierId, PartId, Price, DeliveryDays | → Supplier ; PartName, PartReference, PartType (via JOIN) |

---

## 6. Couche DataAccess

### 6.1 Connexion

`DatabaseConnection` construit une `MySqlConnection` à partir des variables d'environnement (via `EnvConfig`). La méthode `GetConnection()` ouvre et retourne une connexion.

### 6.2 Repositories (9 paires Interface/Implémentation)

Chaque repository suit le pattern : interface dans `DataAccess/Interfaces/`, implémentation dans `DataAccess/Repositories/`.

| Repository | Opérations clés |
|---|---|
| **CustomerRepository** | GetById, GetAll, Add, Update, Delete |
| **OrderRepository** | GetById, GetAll, **GetAllWithDetails()** (JOIN customer + SUM order_line), GetByCustomerId, Add, Update, Delete |
| **OrderLineRepository** | GetByOrderId, Add, Delete |
| **BillRepository** | GetById, Add |
| **CabinetRepository** | GetById, GetByOrderId, Add, Update, Delete |
| **LockerRepository** | GetById, GetByCabinetId, Add, Update, Delete |
| **PartRepository** | GetById, GetByReference, GetAll, GetByType, **GetLowStock()**, Add, Update, **UpdateStock()**, Delete |
| **SupplierRepository** | GetById, GetAll, Add, Update, Delete |
| **SupplierPartRepository** | **GetAll()** (JOIN supplier+part), GetByPartId, GetBySupplierId, **GetBestSupplierForPart()**, Add, Update, Delete |

**Points notables :**
- `PartRepository` utilise le **Single Table Inheritance** : la méthode `MapPart()` lit le champ `part_type` et instancie la bonne sous-classe C# (Panel, Crossbar, etc.)
- `OrderRepository.GetAllWithDetails()` fait un LEFT JOIN avec `customer` et une sous-requête SUM sur `order_line` pour afficher le résumé des commandes
- `SupplierPartRepository.GetAll()` joint `supplier` et `part` pour peupler les propriétés d'affichage

---

## 7. Couche Services

### 7.1 CatalogService

Catalogue codé en dur des dimensions et couleurs disponibles :

| Méthode | Retour |
|---|---|
| `GetAvailableHeights()` | {25, 30, 35, 40, 50} cm |
| `GetAvailableWidths()` | {40, 60, 80, 100} cm |
| `GetAvailableWidthsWithDoors()` | {40, 60, 80} cm (max 80 car 2 portes × 40cm) |
| `GetAvailableDepths()` | {30, 40, 50} cm |
| `GetAvailableColors()` | White, Black, Grey, Beige, Oak, Walnut |
| `CrossbarHeight` | 2.0 cm |

### 7.2 AngleIronCalculatorService

| Méthode | Logique |
|---|---|
| `CalculateLockerTotalHeight(locker)` | hauteur casier + 2 × hauteur traverse (2cm) |
| `CalculateAngleIronLength(lockers)` | somme des hauteurs totales de tous les casiers |

### 7.3 LockerValidationService

| Méthode | Vérifications |
|---|---|
| `AreDimensionsValid(h, w, d, hasDoors)` | Dimensions dans le catalogue ? (largeur restreinte si portes) |
| `IsLockerCountValid(count)` | 1 ≤ count ≤ 7 |
| `ValidateCabinet(lockers)` | Nombre valide + **tous même largeur** + dimensions valides + couleur porte si portes |

### 7.4 SupplierSelectionService

| Méthode | Logique |
|---|---|
| `GetBestSupplier(partId)` | Meilleur prix → puis meilleur délai (via SupplierPartRepository) |

### 7.5 StockService

| Méthode | Logique |
|---|---|
| `IsAvailable(partId, qty)` | Pièce existe ET stock ≥ quantité demandée |
| `GetLowStockParts()` | Pièces où stock_quantity < minimum_stock |
| `DeductStock(partId, qty)` | Déduit la quantité ; exception si insuffisant |
| `AddStock(partId, qty)` | Ajoute au stock existant |

### 7.6 OrderService (service principal — 261 lignes)

#### `PreviewOrder(lockers, angleIronColor)` → OrderPreview

1. Appelle `BuildRequirements()` pour décomposer les casiers en pièces nécessaires
2. Pour chaque pièce requise, cherche la pièce correspondante en base (`FindPart()`)
3. Vérifie la disponibilité en stock
4. Retourne un `OrderPreview` contenant :
   - Liste de `PartAvailability` (nom, référence, qté requise, en stock, prix)
   - Longueur totale des cornières
   - Prix total
   - `AllPartsAvailable` (booléen)

#### `PlaceOrder(customer, lockers, angleIronColor, depositAmount)` → Order

1. Appelle `PreviewOrder()` pour calculer les pièces
2. Persiste le client s'il est nouveau (via `CustomerRepository.Add`)
3. Crée la commande :
   - **Statut Available** si toutes les pièces en stock
   - **Statut PartiallyAvailable** sinon (+ dépôt + date disponible = aujourd'hui + 14 jours)
4. Crée le `Cabinet` + ses `Locker` en base
5. Crée les `OrderLine` pour chaque pièce
6. **Déduit le stock** pour les pièces disponibles
7. Retourne la commande créée

#### `BuildRequirements()` — Décomposition d'un casier en pièces

Pour **chaque casier** :
| Pièce | Quantité | Dimensions utilisées |
|---|---|---|
| Batten (montant) | 4 | hauteur = hauteur casier |
| Front Crossbar | 2 | largeur = largeur casier |
| Back Crossbar | 2 | largeur = largeur casier |
| Side Crossbar | 4 | largeur = profondeur casier |
| Horizontal Panel | 2 | largeur × profondeur |
| Side Panel | 2 | hauteur × profondeur |
| Back Panel | 1 | hauteur × largeur |
| Door (si portes) | 2 | hauteur × (largeur/2) |
| Handle (si portes) | 2 | couleur = couleur porte |

Pour **l'armoire entière** :
| Pièce | Quantité | Dimension |
|---|---|---|
| Angle Iron (cornière) | 4 | longueur = somme hauteurs totales des casiers |

#### `FindPart()` — Recherche de pièce en base

Cherche par : `part_type`, sous-type (`panel_type`/`crossbar_type`), dimensions non-nulles (tolérance ±0.01), couleur, `is_glass`. Retourne `null` si aucune correspondance.

---

## 8. Couche ViewModels

### 8.1 MainViewModel (hub de navigation)

C'est le ViewModel racine. Il possède :
- `CurrentPage` : le ViewModel actuellement affiché
- `Services` : instance de `AppServices` (injection de services)

**Méthodes de navigation :**
| Méthode | Page cible |
|---|---|
| `GoToCustomerSelection()` | Sélection client |
| `GoToOwnerDashboard()` | Dashboard stock |
| `GoToSecretaryMenu()` | Menu secrétaire |
| `GoToSupplierCatalog()` | Catalogue fournisseur |
| `GoToOrderHistory()` | Historique commandes |
| `GoToHome()` | Page d'accueil |
| `GoToCabinetConfiguration(customer)` | Configuration armoire |
| `GoToOrderSummary(customer, lockers, color)` | Résumé commande |

### 8.2 StartPageViewModel

Écran d'accueil avec 2 boutons :
- **Client** → `GoToCustomerSelection()`
- **Secrétaire** → `GoToSecretaryMenu()`

### 8.3 CustomerSelectionViewModel

Formulaire client :
- Champs : Prénom, Nom, Email, Téléphone
- **Continue** : valide les champs (prénom + nom requis, email avec @) → crée `Customer` → CabinetConfiguration
- **Continue as Guest** : crée un client avec email `guest-XXXXXXXX@guest.kitbox` → CabinetConfiguration
- **Back** → StartPage

### 8.4 CabinetConfigurationViewModel

Constructeur d'armoire :
- Liste de `LockerConfigViewModel` (1 à 7 casiers)
- **Add Locker** : ajoute un casier (max 7)
- **Remove Locker** : supprime un casier (min 1)
- Sélection couleur cornière
- **Preview Order** : convertit les VMs en modèles `Locker` → `LockerValidationService.ValidateCabinet()` → si valide, va au résumé

### 8.5 LockerConfigViewModel

Configuration d'un casier individuel :
- ComboBox : Hauteur, Largeur, Profondeur, Couleur (chargés depuis `CatalogService`)
- CheckBox : A des portes ?
- ComboBox : Couleur porte (visible si portes)
- Quand `HasDoors` change, la liste des largeurs disponibles se restreint (max 80cm)

### 8.6 OrderSummaryViewModel

Écran de prévisualisation et confirmation :
- Affiche les pièces nécessaires avec statut de disponibilité (✓/✗)
- Prix total
- Si pas tout en stock : zone de saisie d'acompte (suggestion : 30% du total)
- **Confirm Order** → appelle `OrderService.PlaceOrder()` → affiche le numéro de commande
- **Back** → CabinetConfiguration
- **New Order** (après confirmation) → StartPage

### 8.7 OwnerDashboardViewModel (Stock)

Tableau de gestion de stock :
- Charge toutes les pièces via `PartRepository.GetAll()`
- Affiche par ligne : nom, type, référence, stock actuel, stock minimum, badge statut
- **Alerte stock bas** : compteur de pièces sous le seuil, lignes en surbrillance orange
- **Modification inline** : champ numérique + bouton Save par ligne → `StockService.AddStock()`
- **Refresh** : recharge les données

### 8.8 SecretaryMenuViewModel

Menu intermédiaire avec 3 options :
| Bouton | Action |
|---|---|
| **Supplier Catalog** (violet) | → SupplierCatalog |
| **Stock Management** (gris foncé) | → OwnerDashboard |
| **Order History** (teal) | → OrderHistory |

### 8.9 SupplierCatalogViewModel

Gestion du catalogue fournisseur :
- **Tableau** : fournisseur, nom pièce, référence, type, prix actuel, délai actuel, nouveau prix, nouveau délai, Save, Delete
- **Filtrage** : par fournisseur (ComboBox) et/ou par texte (recherche dans nom, référence, type)
- **Ajout** : formulaire dépliable (fournisseur, ID pièce, prix, délai)
- **Modification inline** : modifier prix et délai → Save → `SupplierPartRepository.Update()`
- **Suppression** : bouton 🗑 → `SupplierPartRepository.Delete()`

### 8.10 OrderHistoryViewModel

Historique et suivi des commandes :
- **Tableau** : n° commande, client, email, date, nb pièces, total, acompte, date disponible, statut, actions
- **Filtrage** : par statut (All, Pending, PartiallyAvailable, Available, Delivered, Cancelled)
- **Actions par commande** (affichées conditionnellement) :

| Action | Condition | Effet |
|---|---|---|
| ✓ Available | Statut = PartiallyAvailable | Passe à Available, met date = aujourd'hui |
| 📦 Delivered | Statut = Available | Passe à Delivered |
| 🧾 Bill | Statut = Delivered ET pas de facture | Crée une facture (montant = total - acompte) |
| ✕ Cancel | Statut ≠ Delivered ET ≠ Cancelled | Passe à Cancelled |

---

## 9. Couche Views

Toutes les vues sont des fichiers `.axaml` (XAML Avalonia) avec un code-behind minimal (uniquement `InitializeComponent()`).

### 9.1 MainView.axaml
Fenêtre principale (1600×900, maximisée). Contient un seul `ContentControl` lié à `CurrentPage` — le `ViewLocator` résout automatiquement la bonne vue.

### 9.2 StartPageView.axaml
- Fond gris, carte blanche centrée (800×600)
- Titre **"KitBox"** en indigo, 60pt
- Bouton **Customer** (indigo `#4F46E5`)
- Bouton **Secretary** (violet `#7C3AED`)

### 9.3 CustomerSelectionView.axaml
- Header indigo avec bouton retour
- Formulaire centré : 4 champs texte (Prénom, Nom, Email, Téléphone)
- Bandeau d'erreur rouge si validation échoue
- Boutons Continue / Continue as Guest

### 9.4 CabinetConfigurationView.axaml
- Header indigo avec compteur "N/7 lockers"
- Liste scrollable de casiers (7 colonnes : Hauteur, Largeur, Profondeur, Couleur, Portes?, Couleur porte)
- Bouton Add Locker (+ Remove sur chaque ligne)
- ComboBox couleur cornière
- Bandeau validation (amber) si erreurs
- Bouton "Preview Order →"

### 9.5 OrderSummaryView.axaml
- Header indigo avec nom client
- Carte info : nombre casiers, longueur cornière, couleur cornière
- Tableau des pièces avec badges colorés (vert = en stock, rouge = insuffisant)
- Prix total en grand
- Zone acompte (amber) si stock insuffisant
- Bouton "Confirm Order ✓"

### 9.6 OwnerDashboardView.axaml
- Header gris foncé (`#374151`) avec compteur alertes
- Tableau : Nom, Type, Référence, En stock, Min., Statut (badge), Nouvelle qté + Save
- Lignes en orange si stock bas

### 9.7 SecretaryMenuView.axaml
- Carte centrée avec titre "Secretary" en violet
- 3 boutons : Supplier Catalog (violet), Stock Management (gris), Order History (teal)
- Chaque bouton a un sous-titre descriptif

### 9.8 SupplierCatalogView.axaml
- Header violet (`#7C3AED`)
- Barre de filtre : ComboBox fournisseur + recherche texte + Clear
- Formulaire d'ajout dépliable
- Tableau 10 colonnes avec édition inline (NumericUpDown pour prix et délai)

### 9.9 OrderHistoryView.axaml
- Header teal (`#0F766E`)
- Filtre par statut (ComboBox)
- Tableau 10 colonnes avec badges colorés par statut
- Boutons d'action conditionnels par ligne

### 9.10 Styles (AppDefaultStyle.axaml)
Styles partagés : grille menu, boutons thématiques, cards avec ombres, icônes Phosphor.

---

## 10. Flux de navigation

```
StartPageView
├── [Customer] → CustomerSelectionView
│   ├── [Continue / Guest] → CabinetConfigurationView
│   │   ├── [Preview Order →] → OrderSummaryView
│   │   │   ├── [Confirm Order ✓] → (reste, affiche statut)
│   │   │   ├── [← New Order] → StartPageView
│   │   │   └── [← Back] → CabinetConfigurationView
│   │   └── [← Back] → CustomerSelectionView
│   └── [← Back] → StartPageView
│
├── [Secretary] → SecretaryMenuView
│   ├── [Supplier Catalog] → SupplierCatalogView
│   │   └── [← Back] → StartPageView
│   ├── [Stock Management] → OwnerDashboardView
│   │   └── [← Back] → StartPageView
│   ├── [Order History] → OrderHistoryView
│   │   └── [← Back] → SecretaryMenuView
│   └── [← Back] → StartPageView
```

---

## 11. Processus métier détaillés

### 11.1 Processus de commande client (workflow complet)

```
1. Client arrive → StartPage → clique "Customer"
2. Saisit ses infos (ou continue en tant qu'invité)
3. Configure son armoire :
   a. Ajouter des casiers (1 à 7)
   b. Pour chaque casier : hauteur, largeur, profondeur, couleur
   c. Optionnel : cocher "Portes" + choisir couleur porte
   d. Choisir la couleur des cornières
   e. Validation : tous les casiers doivent avoir la même largeur
4. Prévisualisation de la commande :
   a. L'application décompose l'armoire en pièces individuelles
   b. Chaque pièce est recherchée en base de données
   c. La disponibilité en stock est vérifiée
   d. Le prix total est calculé
5. Confirmation :
   a. Si tout est en stock → statut "Available"
   b. Si stock partiel → statut "PartiallyAvailable"
      → Le client doit verser un acompte (suggestion : 30%)
      → Date de disponibilité estimée : aujourd'hui + 14 jours
   c. Le stock est déduit pour les pièces disponibles
   d. La commande est enregistrée avec toutes ses lignes
```

### 11.2 Processus de gestion du catalogue fournisseur

```
1. Secrétaire → Menu → "Supplier Catalog"
2. Visualise tous les prix et délais par fournisseur/pièce
3. Peut filtrer par fournisseur ou rechercher par texte
4. Modifier un prix/délai : modifier les champs → cliquer "Save"
   → Met à jour la table supplier_part en base
5. Ajouter une entrée : ouvrir le formulaire, renseigner fournisseur,
   ID pièce, prix, délai → "Add"
6. Supprimer une entrée : cliquer le bouton 🗑
```

### 11.3 Processus de gestion des stocks

```
1. Secrétaire → Menu → "Stock Management" (ou Propriétaire direct)
2. Visualise toutes les pièces avec leur stock actuel vs. minimum
3. Les pièces en stock bas sont surlignées en orange avec badge "⚠ Low"
4. Pour réapprovisionner : modifier la quantité → "Save"
   → Calcule le delta et appelle StockService.AddStock()
5. Le compteur d'alertes se met à jour en temps réel
```

### 11.4 Processus de suivi des commandes

```
1. Secrétaire → Menu → "Order History"
2. Visualise toutes les commandes avec détails et statut
3. Peut filtrer par statut (All, Pending, PartiallyAvailable, etc.)
4. Workflow des statuts :

   PartiallyAvailable ──[✓ Available]──→ Available
                                              │
                                    [📦 Delivered]
                                              │
                                              ▼
                                          Delivered
                                              │
                                        [🧾 Bill]
                                              │
                                              ▼
                                    Facture générée
                                    (montant = total - acompte)

   Tout statut sauf Delivered/Cancelled ──[✕ Cancel]──→ Cancelled
```

### 11.5 Processus de sélection fournisseur

```
Critère 1 : Meilleur prix (prix le plus bas)
Critère 2 : En cas d'égalité, meilleur délai de livraison
→ La requête SQL trie par price ASC, delivery_days ASC LIMIT 1
```

### 11.6 Décomposition d'une armoire en pièces (exemple)

Pour une armoire avec 2 casiers (H30×L60×P40 blanc + H40×L60×P40 noir avec portes) :

**Casier 1** (30×60×40, blanc, sans portes) :
- 4 Battens blanc H=30
- 2 Front Crossbars W=60
- 2 Back Crossbars W=60
- 4 Side Crossbars W=40
- 2 Horizontal Panels 60×40
- 2 Side Panels 30×40
- 1 Back Panel 30×60

**Casier 2** (40×60×40, noir, avec portes) :
- 4 Battens noir H=40
- 2 Front Crossbars W=60
- 2 Back Crossbars W=60
- 4 Side Crossbars W=40
- 2 Horizontal Panels 60×40
- 2 Side Panels 40×40
- 1 Back Panel 40×60
- 2 Doors H=40 W=30 (largeur/2)
- 2 Handles

**Armoire entière** :
- Longueur cornière = (30+4) + (40+4) = 78 cm
- 4 Angle Irons de 78 cm

**Total** : ~34 pièces individuelles à vérifier en stock.

---

## 12. Ce qui a été implémenté

### ✅ Architecture complète
- [x] Pattern MVVM avec Avalonia UI
- [x] Couche Repository + Service Layer
- [x] Injection de services via AppServices (composition root)
- [x] ViewLocator par convention
- [x] Configuration par fichier .env
- [x] Base de données avec 8 tables, index de performance
- [x] Données initiales (seed) avec 100+ pièces et 3 fournisseurs

### ✅ Flux client (commande d'armoire)
- [x] Sélection de rôle (Client / Secrétaire)
- [x] Saisie infos client ou mode invité
- [x] Configuration d'armoire (1-7 casiers)
- [x] Configuration individuelle de chaque casier (dimensions, couleur, portes)
- [x] Validation métier (dimensions, largeur uniforme, couleur porte)
- [x] Décomposition automatique en pièces
- [x] Recherche de pièces en base par correspondance
- [x] Vérification de disponibilité en stock
- [x] Calcul du prix total
- [x] Gestion d'acompte si stock partiel (30% suggéré)
- [x] Passage de commande avec persistance complète
- [x] Déduction automatique du stock

### ✅ Gestion des stocks
- [x] Tableau complet de toutes les pièces
- [x] Alertes de stock bas (visuelles)
- [x] Modification de quantité inline avec sauvegarde

### ✅ Catalogue fournisseur
- [x] Affichage du catalogue complet (JOIN supplier + part)
- [x] Filtrage par fournisseur et recherche textuelle
- [x] Modification inline des prix et délais
- [x] Ajout de nouvelles entrées
- [x] Suppression d'entrées

### ✅ Historique des commandes
- [x] Liste de toutes les commandes avec détails
- [x] Filtrage par statut
- [x] Workflow de statuts (PartiallyAvailable → Available → Delivered)
- [x] Génération de facture
- [x] Annulation de commande

### ✅ Logique métier
- [x] Sélection automatique du meilleur fournisseur (prix puis délai)
- [x] Calcul de longueur des cornières
- [x] Validation des dimensions depuis le catalogue
- [x] Single Table Inheritance pour les pièces
- Si stock suffisant: prélèvement du stock local.
- Si stock insuffisant: prélèvement du disponible, puis commande automatique du manque au meilleur fournisseur.
La date de disponibilité de la commande client est recalculée selon la date de livraison attendue la plus tardive des commandes fournisseurs créées.

---

## 13. Ce qu'il reste à faire

### 🔲 Fonctionnalités manquantes

| Priorité | Fonctionnalité | Détail |
|---|---|---|
| **Haute** | Résumé de commande avec prise en charge du paiement | Actuellement le paiement n'est pas géré (pas de module de paiement) ; seul l'acompte est enregistré |
| **Haute** | Réapprovisionnement automatique | Le stock bas est détecté mais il n'y a pas de commande fournisseur automatique ni de suggestion de réapprovisionnement |
| **Haute** | Gestion des retours et remboursements | Aucun processus de retour n'est implémenté |
| **Moyenne** | Impression/export de facture | La facture est créée en base mais pas exportée (PDF, impression) |
| **Moyenne** | Historique des ventes pour calcul du stock minimum | Le stock minimum est statique (seed) ; il devrait être calculé à partir de l'historique -- > FAIT mais à tester !| 
| **Moyenne** | Gestion des clients existants | Le formulaire client crée toujours un nouveau client ; pas de recherche/sélection d'un client existant |
| **Moyenne** | Notification quand commande disponible | Pas de système de notification client (email, etc.) |
| **Basse** | Étagères, tiroirs et autres éléments | L'architecture Part est extensible mais ces modèles ne sont pas encore créés |
| **Basse** | Bouton Owner sur StartPage | Le ViewModel a la commande mais le bouton n'est pas affiché dans la vue AXAML |
| **Basse** | Page d'accueil fonctionnelle (HomePageView) | La page existe avec du contenu statique mais n'est pas intégrée au flux navigation |
| **Basse** | Tests unitaires | Aucun test n'est implémenté |
| **Basse** | Gestion d'erreurs réseau | Les erreurs DB sont affichées mais pas de retry ni de mode hors-ligne |

### 🔲 Améliorations techniques

| Amélioration | Détail |
|---|---|
| Injection de dépendances (DI container) | Remplacer `AppServices` par un vrai conteneur DI (Microsoft.Extensions.DependencyInjection) |
| Tests unitaires et d'intégration | Avec xUnit ou NUnit, mocking des repositories |
| Async/Await sur les requêtes DB | Les opérations DB sont actuellement synchrones |
| Logging | Ajouter un système de logs (Serilog, NLog) |
| Gestion des transactions | Les opérations multi-tables (PlaceOrder) ne sont pas dans une transaction SQL |
| Cache du catalogue | `CatalogService` est déjà statique, mais les données DB pourraient être cachées |
| Validation côté client plus poussée | Regex email, format téléphone, etc. |

### 🔲 Navigation à corriger

| Problème | Détail |
|---|---|
| Back du SupplierCatalog → StartPage | Devrait retourner au SecretaryMenu |
| Back du OwnerDashboard → StartPage | Devrait retourner au SecretaryMenu si venant du menu secrétaire |

---

## Annexe : Structure des fichiers

```
Software-Engineering-Project/
├── context.md                          # Énoncé du projet
├── schema.sql                          # Script de création de la base
├── seed.sql                            # Données initiales
├── PROJECT_GUIDE.md                    # Ce fichier
├── README.md
│
└── KitBox/
    ├── .env.example                    # Template de configuration
    ├── KitBox.csproj                   # Projet .NET 9.0
    ├── KitBox.sln                      # Solution
    ├── Program.cs                      # Point d'entrée
    ├── App.axaml / App.axaml.cs        # Application Avalonia
    ├── ViewLocator.cs                  # Résolution View ↔ ViewModel
    ├── EnvConfig.cs                    # Lecture .env
    │
    ├── Models/
    │   ├── Enums/
    │   │   ├── OrderStatus.cs
    │   │   ├── PanelType.cs
    │   │   └── CrossbarType.cs
    │   ├── Parts/
    │   │   ├── Part.cs (abstract)
    │   │   ├── Panel.cs
    │   │   ├── Crossbar.cs
    │   │   ├── Batten.cs
    │   │   ├── AngleIron.cs
    │   │   ├── Door.cs
    │   │   └── Handle.cs
    │   ├── Customer.cs
    │   ├── Order.cs
    │   ├── OrderLine.cs
    │   ├── Cabinet.cs
    │   ├── Locker.cs
    │   ├── Bill.cs
    │   ├── Supplier.cs
    │   └── SupplierPart.cs
    │
    ├── DataAccess/
    │   ├── DatabaseConnection.cs
    │   ├── AppServices.cs
    │   ├── Interfaces/
    │   │   ├── IBillRepository.cs
    │   │   ├── ICustomerRepository.cs
    │   │   ├── IOrderRepository.cs
    │   │   ├── IOrderLineRepository.cs
    │   │   ├── ICabinetRepository.cs
    │   │   ├── ILockerRepository.cs
    │   │   ├── IPartRepository.cs
    │   │   ├── ISupplierRepository.cs
    │   │   └── ISupplierPartRepository.cs
    │   └── Repositories/
    │       ├── BillRepository.cs
    │       ├── CustomerRepository.cs
    │       ├── OrderRepository.cs
    │       ├── OrderLineRepository.cs
    │       ├── CabinetRepository.cs
    │       ├── LockerRepository.cs
    │       ├── PartRepository.cs
    │       ├── SupplierRepository.cs
    │       └── SupplierPartRepository.cs
    │
    ├── Services/
    │   ├── Interfaces/
    │   │   ├── ICatalogService.cs
    │   │   ├── IAngleIronCalculatorService.cs
    │   │   ├── ILockerValidationService.cs
    │   │   ├── ISupplierSelectionService.cs
    │   │   ├── IStockService.cs
    │   │   └── IOrderService.cs
    │   ├── CatalogService.cs
    │   ├── AngleIronCalculatorService.cs
    │   ├── LockerValidationService.cs
    │   ├── SupplierSelectionService.cs
    │   ├── StockService.cs
    │   └── OrderService.cs
    │
    ├── ViewModels/
    │   ├── ViewModelBase.cs
    │   ├── MainViewModel.cs
    │   ├── StartPageViewModel.cs
    │   ├── HomePageViewModel.cs
    │   ├── CustomerSelectionViewModel.cs
    │   ├── CabinetConfigurationViewModel.cs
    │   ├── LockerConfigViewModel.cs
    │   ├── OrderSummaryViewModel.cs
    │   ├── PartAvailabilityViewModel.cs
    │   ├── OwnerDashboardViewModel.cs
    │   ├── SecretaryMenuViewModel.cs
    │   ├── SupplierCatalogViewModel.cs
    │   └── OrderHistoryViewModel.cs
    │
    ├── Views/
    │   ├── MainView.axaml(.cs)
    │   ├── StartPageView.axaml(.cs)
    │   ├── HomePageView.axaml(.cs)
    │   ├── CustomerSelectionView.axaml(.cs)
    │   ├── CabinetConfigurationView.axaml(.cs)
    │   ├── OrderSummaryView.axaml(.cs)
    │   ├── OwnerDashboardView.axaml(.cs)
    │   ├── SecretaryMenuView.axaml(.cs)
    │   ├── SupplierCatalogView.axaml(.cs)
    │   └── OrderHistoryView.axaml(.cs)
    │
    └── Styles/
        └── AppDefaultStyle.axaml
```
