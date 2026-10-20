# EF Core API Conventions

Apply these conventions to new and changed contracts, not as a reason to rename shipped APIs. Start with the demonstrated caller scenario and the nearest EF Core API family. Prefer a smaller surface and local consistency when general guidelines conflict.

## Ownership

- Keep provider-independent behavior in core, relational behavior in relational, and store-specific configuration in the provider. `EFCore.Abstractions` contains dependency-light contracts, not every interface. Avoid making core depend on a provider or relational assembly.
- Keep common query and configuration extensions discoverable in their established `Microsoft.EntityFrameworkCore` extension classes. Service-registration extensions belong in `Microsoft.Extensions.DependencyInjection`; other extensions should be placed in the same namespace as the type being extended.
- Preserve safe defaults and require explicit opt-in for broader side effects, sensitive-data exposure, or weaker guarantees. State where inputs originate and which providers and execution modes support the contract.

## Naming and Shape

- A method with a non-void result and no observable side effects should usually begin with `Get`. Use `Find` for a lookup whose nullable result means not found, as in metadata `FindProperty` or entity `Find`/`FindAsync`. Nullability alone does not turn a factory or conversion into a lookup.
- Use `Create` for producing an object, `Set` for replacement, and `Add` for insertion. Boolean predicates use the established `Is`/`Has`/`Can` family; conversions and fluent query/configuration methods keep their domain verbs rather than acquiring a mechanical `Get` prefix.
- `Try` denotes an explicit success/failure channel, normally `bool` with an `out` result for a synchronous probe.
- Use affirmative, descriptive Boolean names. Boolean parameters must not start with `is` or `are`.
- Match sibling parameter ordering, null semantics, defaults, and generic constraints. A nullable input must mean something concrete, such as removing a facet or selecting a default, rather than tolerating misconfiguration.
- Accept the least demanding input shape that serves the operation and preserves existing overload binding. Return the abstraction that expresses ownership and caller capabilities; do not mechanically replace established arrays, concrete collections, or metadata contracts.
- Async operations end in `Async`, propagate a final optional `CancellationToken` where the family supports it, and preserve sync/async semantics. Follow the nearby `Task`/`ValueTask` contract; justify a different completion or allocation profile.
- Review modern C# features by the public members and behavioral obligations they introduce. Do not import another repository's blanket sealing, record, or `required`/`init` policy into EF Core. Preserve existing virtual extension points and constructor/service dependency patterns.

## Pubternal APIs and Extensibility

- CLR accessibility is not the support boundary. EF types are public by default; public types in `.Internal` namespaces and members marked `[EntityFrameworkInternal]` are "pubternal": assembly-visible infrastructure with no supported compatibility guarantee.
- Use the repository's standard internal-API XML warning on pubternal types and members. Mark infrastructure-only members on otherwise supported public types with `[EntityFrameworkInternal]`, as with builder constructors and underlying internal builders. Do not expose implementation mechanics as ordinary application APIs.
- `.Infrastructure` alone does not imply unsupported API. Check attributes, XML contracts, implementations, and consumers before classifying a service or extension point. `[Experimental]` has its own explicit diagnostic and stability contract; do not conflate it with pubternal APIs.
- Preserve supported source, binary, and virtual-dispatch contracts. A coordinated change to EF's own implementation does not preserve third-party overrides or already-compiled consumers. Pubternal changes do not require the same compatibility promises, but still require coherent provider and internal call sites.
- Many EF services and builders deliberately have virtual members for provider extensibility. Follow the nearest family's interface, dependency-parameter object, service lifetime, and override pattern rather than sealing it or inventing a new customization hook.
- Keep metadata layers coherent: `IReadOnly*` reads, `IMutable*` represents explicit mutation, and `IConvention*` exposes convention-aware configuration. Where a facet belongs in multiple layers, maintain their nullability, configuration-source, and runtime-model behavior together.

## Fluent Configuration

- `*Builder` classes returned by `ModelBuilder`, `DbContextOptionsBuilder` or nested configuration methods follow the Fluent API pattern, where the chained calls resemble English sentence structure when read. Builders describe configuration, not unrelated execution, and application callers should not need to construct internal builders.
- A facet-setting method returns the same logical builder so configuration can continue. A method that selects or creates another model element returns that element's builder, such as `Property`, `HasKey`, or `HasIndex`. Do not demand reference identity when the family can return a wrapper over updated metadata.
- Keep generic and non-generic surfaces aligned. Typed variants retain `TEntity`/`TProperty`, use expression selectors where appropriate, and return typed builders so chaining does not lose type information. Follow existing forwarding or `new virtual` patterns rather than duplicating configuration logic.
- Relationships are staged: `HasOne`/`HasMany` selects the navigation, `WithOne`/`WithMany` determines the other end, and the resulting relationship builder owns keys, requiredness, and delete behavior. Expose only operations meaningful at that stage.
- Use existing `Has*`/`Is*` names for model facets, `With*` for relationship ends, and `Use*`/`Enable*` for options and provider selection.
- Nested configuration callbacks receive the correct nested builder; returning to the enclosing method preserves its existing chaining contract. Provider extensions belong in the provider and preserve typed receivers/returns where corresponding generic builders exist.
- Delegate model changes to the owning metadata/internal builder with explicit configuration precedence. Application Fluent API uses explicit configuration; convention builders must honor convention/data-annotation precedence and may decline a change. Do not bypass those rules with direct annotation writes.
- Expose `Metadata` and `IInfrastructure<T>` only according to the established family. Keep annotations, convention dispatch, model validation, and compiled/runtime model propagation consistent with the new facet.

## Compatibility and Completeness

- Preserve shipped signatures and dispatch when adding overloads. Compare existing source calls before and after, especially `params`, optional parameters, `null`, and arguments convertible to multiple parameter types; additive APIs can still change recompilation behavior.
- When replacement is necessary, retain the old supported member and use the repository's obsoletion policy with a message naming the replacement. Do not obsolete a still-valid API merely to prefer a newer shape.
- Document required inputs, absence/reset semantics, return ownership, cancellation, provider limits, and side effects. Include trimming/NativeAOT annotations and a safe activation path when construction or reflection is part of the contract.
- Require focused behavioral coverage for the new contract and any binding/dispatch consequence. API-baseline maintenance is test-infrastructure work, not a separate review finding.

## References

- [.NET Framework Design Guidelines digest](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/framework-design-guidelines-digest.md). Use as background; EF-specific contracts and established patterns take precedence.
