# Software Engineering Project
Guide du Projet KitBox (État Actuel)

Ce document reflète l'état actuel de l'implémentation du projet et le met en relation avec le contexte métier défini dans context.md.
1. Portée du Projet

KitBox est une application de bureau utilisée en magasin pour configurer des commandes d'armoires et gérer les opérations de stock et de fournisseurs.

Objectifs métier principaux issus de context.md :

    Réduire les erreurs de préparation de commande en numérisant la composition des armoires.

    Gérer la disponibilité partielle avec acompte et retrait différé.

    Optimiser la gestion des stocks :

        une pièce peut être fournie par plusieurs fournisseurs,

        le stock minimum est piloté par l'historique des ventes,

        la sélection du fournisseur suit le meilleur prix, puis le meilleur délai de livraison.

2. Technologie et Architecture

    Langage : C# (.NET 9.0)

    Interface utilisateur (UI) : Avalonia 11.3.6 (MVVM)

    Base de données : MariaDB/MySQL via MySqlConnector

    Pattern : Architecture en couches

        Vue (.axaml)

        ViewModel

        Services (logique métier)

        Repositories (accès aux données)

La composition des dépendances est centralisée dans KitBox/AppServices.cs.
3. Aperçu de l'État Fonctionnel
3.1 Flux de commande client

Implémenté :

    Saisie de l'identité du client (client nommé ou invité).

    Configuration de l'armoire avec 1 à 7 casiers.

    Contraintes des casiers :

        dimensions et couleurs autorisées selon le catalogue,

        largeurs restreintes lorsque les portes sont activées,

        même largeur pour tous les casiers d'une même armoire.

    Crochet de prévisualisation 3D (WebView + Assets/Web/index.html) recevant l'état sérialisé du casier.

    Aperçu de la commande :

        décomposition en pièces requises,

        vérification du stock par pièce,

        calcul du prix total.

    Passation de commande :

        persistance complète (client, commande, armoire, casiers, lignes de commande),

        déduction de stock pour les quantités disponibles,

        support de la disponibilité partielle avec acompte.

3.2 Optimisation des fournisseurs et des stocks

Implémenté :

    CRUD du catalogue fournisseur (supplier_part) par le secrétaire.

    Stratégie du meilleur fournisseur implémentée dans le service + tri SQL :

        prix croissant (ASC),

        jours de livraison croissant (ASC) en cas d'égalité.

    Calcul dynamique du stock minimum basé sur l'historique des ventes :

        requête des quantités vendues récentes depuis order_line + customer_order.order_date,

        minimum recommandé = max(valeur par défaut, arrondi.sup(ventes quotidiennes moyennes * jours de couverture)).

    Tableau de bord de stock bas avec statut de risque au niveau de la ligne.

    Action de réapprovisionnement de la gestion des stocks :

        bouton dans le tableau de stock pour passer une commande fournisseur depuis une ligne en rupture,

        quantité par défaut égale à l'écart minimum,

        fournisseur choisi selon le meilleur prix puis le meilleur délai,

        persisté dans supplier_order.

3.3 Processus commercial (acompte, livraison, facturation)

Implémenté :

    Si toutes les pièces ne sont pas en stock, la commande est marquée PartiallyAvailable (Partiellement Disponible) et un acompte est enregistré.

    Les quantités manquantes sont automatiquement approvisionnées lors de la commande auprès des meilleurs fournisseurs.

    La date de disponibilité (available_date) est recalculée en utilisant le délai de livraison du fournisseur le plus lent parmi les commandes fournisseurs générées.

    Flux de statut de l'historique des commandes avec actions du secrétaire :

        PartiallyAvailable -> Available -> Delivered,

        annulation (sauf pour les commandes livrées/annulées).

    Génération de facture à l'étape livrée :

        montant de la facture = montant total de la commande - acompte.

    Exportation TXT des documents de paiement :

        reçu d'acompte généré à la confirmation de commande partielle,

        facture de paiement final générée lors de la création de la facture,

        exporté dans le dossier "Téléchargements" de l'utilisateur.

4. État de la Base de Données (Mis à jour)

Le schéma actuel inclut :

    customer (client)

    bill (facture)

    customer_order (commande client)

    cabinet (armoire)

    locker (casier)

    part (pièce)

    order_line (ligne de commande)

    supplier (fournisseur)

    supplier_part (pièce fournisseur)

    supplier_order (commande fournisseur)

Notes :

    part utilise l'héritage par table unique (discriminateur part_type).

    supplier_order.customer_order_id est nullable :

        lié aux commandes pour l'approvisionnement en cas de pénurie client,

        nul pour les commandes de réapprovisionnement pur de gestion de stock.

5. Correspondance Processus Métier vs Contexte
Exigence A : une pièce peut être fournie par plusieurs fournisseurs

Statut : Implémenté.

    Le modèle de données supporte la relation N:N via supplier_part.

    Le secrétaire gère les tarifs et délais des fournisseurs.

Exigence B : stock minimum déterminé par l'historique des ventes

Statut : Implémenté (logique), nécessite une fiabilisation opérationnelle.

    Les ventes historiques sont agrégées à partir des lignes de commande passées.

    Le stock minimum est périodiquement actualisé lors du rafraîchissement du tableau de bord.

Exigence C : fournisseur sélectionné par meilleur prix, puis meilleur délai

Statut : Implémenté.

    Appliqué à deux endroits :

        approvisionnement automatique lors d'une pénurie sur une commande client,

        action de réapprovisionnement de la gestion des stocks.

6. Processus Détaillés Implémentés
6.1 Processus de commande client

    L'utilisateur crée ou sélectionne l'identité d'un client.

    L'utilisateur configure l'armoire et les casiers.

    Le service de validation vérifie les contraintes dimensionnelles et métier.

    L'aperçu de la commande décompose l'armoire en pièces et vérifie la disponibilité.

    À la confirmation :

        les données de la commande sont stockées,

        le stock disponible est consommé,

        les quantités manquantes déclenchent des commandes fournisseurs,

        la disponibilité prévue est calculée,

        un reçu d'acompte TXT est généré le cas échéant.

6.2 Processus de gestion des stocks

    Le rafraîchissement du tableau de bord recalcule le stock minimum à partir des ventes récentes.

    Les pièces en sous-stock sont mises en évidence.

    Le secrétaire peut :

        ajuster manuellement la quantité en stock,

        cliquer sur Order pour passer une commande de réapprovisionnement auprès du meilleur fournisseur.

    La confirmation de la commande fournisseur est affichée dans la bannière de statut de l'interface.

6.3 Processus de facturation

    Le secrétaire marque la progression du cycle de vie de la commande.

    À l'état Delivered (Livré), le secrétaire génère la facture.

    La facture est stockée et liée à la commande.

    La facture de paiement final TXT est exportée dans "Téléchargements".

7. Ce Qu'il Reste à Faire (Priorités)

Cette section met en évidence les lacunes restantes avec une forte priorité sur le risque métier.
7.1 Lacunes métier critiques

    Pas de flux de réception fournisseur --> DONE

        Les commandes fournisseurs sont créées, mais il n'y a pas de processus de réception formel (pas d'interface de transition de statut, pas d'incrémentation automatique du stock, pas de recalcul pour passer les commandes clients en attente à Available).

    Pas de module de transaction de paiement --> Pas important

        Les montants sont enregistrés, mais aucune opération de paiement réelle n'existe (historique d'audit, numérotation des reçus, etc.).

    Pas de transaction de commande sécurisée (Rollback) --> Pas vitale

        PlaceOrder effectue plusieurs écritures sans frontières de transaction DB. Un échec partiel peut laisser des données incohérentes.

    L'annulation ne restaure pas le stock --> Important

        Annuler une commande ne met à jour que le statut, sans logique de compensation pour les stocks déduits ou réservés.

7.2 Lacunes de processus importantes

    Pas de recherche/réutilisation de la base client : Risque de doublons.

    Pas de système de notification : Le client n'est pas prévenu automatiquement quand sa commande est disponible.

    Visibilité limitée des commandes fournisseurs : Pas d'écran dédié pour lister/filtrer et suivre les livraisons attendues. --> DONE

    Pas de consolidation des achats : Le réapprovisionnement se fait par ligne ; pas de groupement par fournisseur pour optimiser les commandes.

7.3 Qualité et dette technique

    Absence de tests automatisés (unitaires et intégration).

    Gestion de la concurrence absente : Risque de survente si deux commandes sont passées simultanément.

    E/S Asynchrones non adoptées dans la couche repository.

    Observabilité minimale : Pas de logs structurés ni de pistes d'audit.

8. Feuille de Route Recommandée
Phase 1 (Stabilisation indispensable)

    Encapsuler PlaceOrder dans des transactions SQL.

    Implémenter le flux de réception fournisseur et la mise à jour des stocks.

    Ajouter la réévaluation automatique des commandes en attente après réception fournisseur.

    Ajouter la logique de compensation de stock pour les annulations.

Phase 2 (Complétude du processus)

    Écran de gestion des commandes fournisseurs.

    Recherche/fusion de clients pour éviter les doublons.

    Pipeline de notification (e-mail/SMS) pour la disponibilité.

    Export PDF des factures/reçus avec mise en forme légale.