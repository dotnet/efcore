## MergeOptionFeature Tasks

### 01. Obuhvaćeni fileovi (testovi)
Obuhvaćeni su svi fileovi - testovi iz ovog foldera:
RefreshFromDb_ComplexTypes_SqlServer_Test.cs
RefreshFromDb_ComputedColumns_SqlServer_Test.cs
RefreshFromDb_GlobalFilters_SqlServer_Test.cs
RefreshFromDb_ManyToMany_SqlServer_Test.cs
RefreshFromDb_Northwind_SqlServer_Test.cs
RefreshFromDb_PrimitiveCollections_SqlServer_Test.cs
RefreshFromDb_ShadowProperties_SqlServer_Test.cs
RefreshFromDb_TableSharing_SqlServer_Test.cs
RefreshFromDb_ValueConverters_SqlServer_Test.cs

Apsolutna je putanja do foldera:
c:\Devel\iplus-github\ef_90_Main\test\EFCore.SqlServer.FunctionalTests\MergeOptionFeature\

Ali ti vidiš sve ove fileove iz foldera gdje se nalazi ovaj file sa zadatkom.

### 01. Objašnjenje generalno cilja zadatka
Želja je dobiti jedan zajednički kontekst koji je kreiran na osnovu Northwind baze podataka.
Cilj je da se promjene koje se ovdje dešavaju ne mogu doći u intefrerenciju s bilo kojim drugim testom.


### 02. Zadatak 01. Kreiranje filea NorthwindMergeOptionFeatureContext.cs
To će biti file i novi NorthwindMergeOptionFeatureContext (NorthwindMergeOptionFeatureContext.cs) kontekst koji će kreirati
istoimenu bazu i koji će sadržavati zajednički DbContext i OnModelCreating metodu.
Koristi će ga gore svi navdedeni testovi.

Ideja bi bila da se nasljedi Northwind baza i dodaju članovi koji trebaju svim testovima.
Znači ne više konteksta i klasa u tom fileu već samo jedan proširen s svime što treba svim testovima s liste.
Ono što sam bio primjetio kod pokušaja izvršavanja ovog zadatka da se sadržaj testova
briše i da svaki test se nasljeđuje iz tog novo kreiranog NorthwindMergeOptionFeatureContext.
To ne želim, NorthwindMergeOptionFeatureContext je samo DI input za test a svi testovi
koji su označeni s [Fact] trebaju ostati ali promjenjeni na način da koriste novi NorthwindMergeOptionFeatureContext.

### 03. Zadatak 02. Modifikacija postojećih testova
 Svi navedeni testovi trebaju biti modificirani da koriste novi NorthwindMergeOptionFeatureContext

### 04 Zadatak 03 Dodatna modifikacija testova
Kako sada neće biti interferencije između testova može se izbaciti sve što je povezano s time
da se napravi "clean up" po završetku testa pa treba napraviti sljedeće:
- 03.01 ukloniti sve try catch blokove - nisu potrebni jer se neće ništa vraćati u prvobitno stanje
- 03.02 isto tako ukloniti naredbe koje su sada obično u finally blocku koje vrećaju rekorde s kojima se testira u prvobitno stanje
- 03.03 obično su svi testovi takvi se obično uzme orginalna vrijednost i onda se radi promjena,
neka ta promjena bude uvijek takva da je jedinistvena za to pokretanje, recimo timestamp u stringu
tako da se u drugom pokretanju ne pojavi vrijednosti iz prvog pokretanja testa.