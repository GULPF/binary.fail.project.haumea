# TODO:
- - -
## Speldesign
- Bestämma grundläggade spelmekaniker.
- Kartdesign.

## Map editor
Parsning från spelfiler finns i Game/Parsing/GameFile.cs, parsning till spelfiler skall fixas snart.

#### Data
- __Tag__: Ett unikt ID bestående av tre bokstäver.
- __Polygon__: En lista av koordinater i planet.
- __Provinser__: `Polygon`, `ProvinceTag`  
_Anm:_ I filformatet finns i nuläget stöd för att definiera en färg för varje provins, detta kommer inte behövas i framtiden så kan ignoreras i redigeraren.
- __Riken__: `List[ProvinceTag]`, `RealmTag`, `Color` (används vid rendering)  
_Anm:__ I nuläget finns inte Color, men kommer åtgärdas snart.

#### Use cases
Denna listan kommer snabbt att bli längre...
1. __Läsa in karta från fil__
2. __Visa nuvarande karta__  
3. __Lägga till ny provins__  
4. __Ta bort befintlig provins__
5. __Redigera information tillhörande befintlig provins__  
_Anm:_ provinser som gränsar till varandra måste ha samma vektorkoordinater för de gemensamma vektorerna. Detta sköter inte filformatet så måste implementeras på något bra sätt i redigeraren.
7. __Ändra ägare av provins__  
_Anm:_ notera att ägaren inte lagras i provinsen, provinsen lagras hos ägaren
8. __Spara redigerad karta till fil__  
_Anm:_ i nuläget finns ingen kod för att skriva till spelfiler, men det kommer att åtgärdas.

#### Övrigt
Väldigt mycket kan komma att ändras, så det är viktigt att göra en flexibel implementation. T ex kommer mycker mer data att bindas till riken, t ex namn, regimtyp, allianser etc.

Så mycket kod som möjligt bör vara delad mellan spelet/redigareren. Det kan hända att en del måste ändras i klasstrukturen i spelet för att uppnå detta.

