---
name: model-building
description: 'Implementation details for EF Core model building. Use when changing ConventionSet, ModelBuilder, IConvention implementations, ModelRuntimeInitializer, RuntimeModel, or related classes.'
user-invocable: false
---

# Model Building, Conventions & Initialization

Covers model construction (conventions, fluent API, metadata hierarchy) and model initialization (runtime annotation propagation, compiled model filtering).

## Convention System

`ConventionSet` (`src/EFCore/Metadata/Conventions/ConventionSet.cs`) holds `List<I*Convention>` for every metadata event. Key conventions in `src/EFCore/Metadata/Conventions/`:

- `DbSetFindingConvention` — discovers entities from `DbSet<T>`
- `PropertyDiscoveryConvention` — discovers properties from CLR types
- `KeyDiscoveryConvention` — finds PKs (`Id`, `TypeId`)
- `RelationshipDiscoveryConvention` — infers FKs from navigations
- `RuntimeModelConvention` — creates optimized `RuntimeModel` from mutable model

Override `ConfigureConventions(ModelConfigurationBuilder)` to add/remove conventions.

Convention fixes must preserve dispatcher behavior and ambiguity information. For inverse-navigation attributes, do not solve recursion by permanently ignoring an ambiguous navigation: removing one candidate can make another ambiguity appear unique and configure the wrong relationship. `InversePropertyAttributeConvention` records inverse candidates and uses `AddInverseNavigation`, `TryRemoveIfAmbiguous`, and `FindAmbiguousInverse` while still allowing convention processing to continue.

Test the state transitions, not only the initially ambiguous model: first verify that multiple interacting candidates configure no relationship; then genuinely remove ambiguity both by ignoring/removing one competing navigation and by removing a competing entity type, and verify that the remaining inverse relationship is configured. These cases distinguish candidate bookkeeping from a permanent-ignore workaround.

## Metadata Interface Hierarchy

`IReadOnly*` → `IMutable*` → `IConvention*` → `IRuntime*`

Applies to: Model, EntityType, Property, Key, ForeignKey, Navigation, Index, etc. Builders follow: `*Builder` → `IConvention*Builder`.

## Adding A Configurable Metadata Facet

Follow the nearest existing facet end to end rather than stopping at the public interface:

1. Add the read-only, mutable, and convention metadata contracts, concrete storage, default, configuration source, convention setter, and builder precedence checks. Preserve the facet when metadata is replaced or a relationship is rebuilt.
2. If changes are observable to conventions, add the convention interface, every `ConventionSet` add/replace/remove path, and immediate plus delayed dispatcher scopes and nodes.
3. Carry the value into the optimized runtime metadata through `RuntimeModelConvention`, runtime constructors/builders, and `CSharpRuntimeModelCodeGenerator` so compiled models preserve it.
4. If snapshots persist the facet, update the fluent builder API and `CSharpSnapshotGenerator`; keep the old absence equivalent to the historical default.
5. Update public API baselines and cover configuration-source precedence, convention dispatch, runtime/compiled model propagation, and snapshot round trips.

## Model Lifecycle

1. **Mutable Model** — built by `ModelBuilder` during `OnModelCreating`, made read-only by `FinalizeModel()`
2. **Design-Time Model** — finalized read-only `Model` that also contains design-time-only annotations used in migrations
3. **Runtime Model** — an optimized read-only model created by `RuntimeModelConvention.ProcessModelFinalized()`, does not contain design-time-only annotations

`ModelRuntimeInitializer.Initialize()` (called by `DbContextServices.CreateModel()`):

```
Initialize(model, designTime, validationLogger)
├─ FinalizeModel() if mutable
├─ Set ModelDependencies, InitializeModel
└─ RuntimeModelConvention creates RuntimeModel, copies/filters annotations
```

## Complex Type Property Recursion

When processing properties in conventions or validation, remember that complex types can contain their own declared properties. Use `GetFlattenedProperties()` to iterate all properties (including on nested non-collection complex types) or manually recurse through `GetDeclaredComplexProperties()` → `complexProperty.ComplexType`.

## Adding a New Annotation

1. Add constant to `CoreAnnotationNames` and its `AllNames`
2. Filter in `RuntimeModelConvention.ProcessModelAnnotations` if it's a design-time-only annotation (only used in migration operations)
   1. If it was filtered out, add logic to the getters that throws an exception if accessed on the runtime model
3. Filter in `CSharpRuntimeAnnotationCodeGenerator.Generate` if it can be computed lazily at runtime (e.g. based on other annotations)
4. Propagate in `RelationalAnnotationProvider` if used in up-migrations or the relational model and `IMigrationsAnnotationProvider` if used in down-migrations

## Relational Model

`RelationalModel` (`src/EFCore.Relational/Metadata/Internal/RelationalModel.cs`) is a database-centric view of the EF model, mapping entity types to physical database objects: `Tables`, `Views`, `Functions`, `Queries`, and `DefaultTables`. `DefaultTables` are pseudo-table objects only used for `FromSql` queries.

Created lazily by `RelationalModelRuntimeInitializer`, accessed via `model.GetRelationalModel()`. Used by migrations (`MigrationsModelDiffer`), update and query pipelines.

`RelationalAnnotationProvider` populates annotations on relational model elements. Provider subclasses (e.g., `SqlServerAnnotationProvider`) add provider-specific annotations. `IMigrationsAnnotationProvider` controls annotations used in down-migration operations.

## Model Validation

`ModelValidator` (`src/EFCore/Infrastructure/ModelValidator.cs`) and `RelationalModelValidator` (`src/EFCore.Relational/Infrastructure/RelationalModelValidator.cs`) run after model finalization, during `ModelRuntimeInitializer.Initialize()` between the pre- and post-validation `InitializeModel` calls.

## Migration Snapshot Compatibility

Model-building changes can trigger spurious migrations for users who upgrade. Two causes:

1. **New metadata written to the snapshot** — old snapshots won't have it; `MigrationsModelDiffer` sees a diff. Fix: ensure absence of the annotation in an old snapshot is treated as the old default.
2. **Annotation renamed or reinterpreted** — old snapshots produce a different model. Fix: keep backward-compatible reading logic.

Inspect `CSharpSnapshotGenerator` (what gets written) and `MigrationsModelDiffer` (how absence is handled). Add a snapshot round-trip test in `test/EFCore.Design.Tests/Migrations/ModelSnapshotSqlServerTest.cs`.

## Testing

| Area | Location |
|------|----------|
| Convention unit tests | `test/EFCore.Tests/Metadata/Conventions/` |
| Metadata unit tests | `test/EFCore.Tests/Metadata/Internal/` |
| Model builder API tests | `test/EFCore.Specification.Tests/ModelBuilding/ModelBuilderTest*.cs` |
| Relationship discovery tests | `test/EFCore.Specification.Tests/ModelBuilding101*.cs` |
| Model validation tests | `test/EFCore.Tests/Infrastructure/ModelValidatorTest*.cs` |
| Compiled model tests | `test/EFCore.Specification.Tests/Scaffolding/CompiledModelTestBase.cs` |

## Validation

- Model builds without `InvalidOperationException` during finalization
- All new API is covered by tests
- Compiled model baselines update cleanly with `EF_TEST_REWRITE_BASELINES=1`
- `ToString()` on metadata objects shows concise contents without throwing exceptions
- No spurious migration is generated against a project with an existing snapshot