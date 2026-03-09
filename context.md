Projet Génie Logiciel 2025-2026 (traduction en français)
La société Kitbox vous a contacté dans le cadre de son projet de digitalisation. L’entreprise vend des armoires (cabinets) en kit.
Composition de la commande
Actuellement, pour acheter une armoire, le client doit se rendre en magasin. À partir du catalogue papier, le client remplit manuellement un bon de commande et le remet au vendeur. Ensuite, le vendeur récupère les pièces correspondant à l’armoire.
Chaque armoire est composée de plusieurs casiers (lockers). La hauteur d’un casier peut varier (voir les possibilités dans le catalogue). On peut assembler au maximum 7 casiers. Les casiers sont maintenus ensemble par 4 cornières (angle irons), qui existent en longueurs standard mais peuvent être coupées pour s’adapter à des hauteurs de casiers différentes.
Pour connaître la longueur des cornières, il suffit de calculer la somme des hauteurs des différents casiers qui composent l’armoire. Pour calculer la hauteur d’un casier, prenez la hauteur des montants verticaux et ajoutez 2 × 2 cm (hauteur des traverses).
De nombreuses erreurs surviennent : soit la commande est incorrecte (par exemple le client a assemblé des casiers incompatibles), soit le magasinier a commis une erreur dans la préparation des pièces.
Pour résoudre ce problème, l’entreprise vous demande de créer une application qui permettra au client ou au vendeur d’encoder la commande en magasin.
Remarques commerciales :
Si toutes les pièces ne sont pas disponibles en stock, le client doit verser un acompte et revenir récupérer la commande lorsqu’elle est disponible.
Les clients payent à la réception des pièces puis reçoivent une facture.
Gestion des stocks
L’entreprise souhaite également optimiser sa gestion de stock selon les contraintes suivantes :
Chaque pièce peut être fournie par plusieurs fournisseurs.
Le stock minimal est déterminé à partir de l’historique des ventes des pièces.
Le fournisseur est choisi selon le meilleur prix ou, en cas d’égalité des prix, le meilleur délai de livraison. Une secrétaire met régulièrement à jour les prix à partir des catalogues fournisseurs.
Caractéristiques d’un casier
Chaque casier est caractérisé par :
ses dimensions (hauteur, largeur, profondeur)
sa couleur (identique pour tous les panneaux du même casier)
la présence éventuelle de portes dont la couleur peut être différente de celle du casier.
Les cornières peuvent aussi être choisies dans une gamme de couleurs (voir catalogue).
Les casiers peuvent être construits avec ou sans portes. Les dimensions des casiers avec portes sont limitées (voir largeurs de porte dans le catalogue).
Un casier est composé (voir dessins) de :
4 montants verticaux
2 traverses avant (chacune avec 2 rainures pour les portes)
2 traverses arrière (chacune avec 1 rainure pour panneau)
4 traverses latérales (chacune avec 1 rainure pour panneau)
2 panneaux horizontaux (posés sur les traverses)
2 panneaux latéraux (glissés dans les rainures des montants verticaux et des traverses)
1 panneau arrière (glissé dans les rainures des montants verticaux et d’une traverse)
2 portes (optionnelles) avec 2 poignées de type "cup" (non disponibles pour les portes en verre)
L’entreprise prévoit également d’ajouter d’autres éléments (étagères, tiroirs, etc.). Planifiez l’architecture de votre application pour que ces éléments puissent être ajoutés sans modifier ce qui fonctionne déjà.
Comme les pièces peuvent être utilisées dans différents casiers, on ne stocke pas le casier en tant que tel, mais uniquement les pièces.
Exigences
Langage obligatoire : C#
Base de données : MySQL ou MariaDB
Tout le code doit être en anglais
Toutes les interfaces utilisateur doivent être en anglais
Toute la documentation et les diagrammes doivent être en anglais
L’architecture doit respecter les principes SOLID, faites donc attention à :
la responsabilité unique (Single Responsibility)
le principe d’ouverture/fermeture (Open/Closed Principle)
éviter la duplication de code