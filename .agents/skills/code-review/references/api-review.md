# EF Core API Review

Use this reference only for supported public or protected API changes outside `.Internal` namespaces and without `[EntityFrameworkInternal]`.

## Review Procedure

1. Enumerate every added, removed, or changed supported member from changed source, using modified API baselines to verify the complete API shape. Include containing types, inherited contracts, implemented interfaces, overload families, and provider counterparts.
2. Establish the current API shape from source and sibling APIs. Do not infer it from names or model memory.
3. Keep abstractions minimal and avoid exposing implementation details.
4. Gate every new API on demonstrated need. Identify a concrete user scenario and verify that an existing API or extension point does not already serve it. If that evidence is absent, recommend preserving the current surface rather than adding an overload. New public surface is permanent maintenance and compatibility cost.
5. Review each member and its type as a coherent family:
   - naming and namespace placement;
   - nullability, parameter ordering, defaults, and generic constraints;
   - parameter and return types, including collection mutability;
   - sync/async pairing, `Async` suffixes, and cancellation-token placement and flow;
   - overload resolution and optional-parameter ambiguity;
   - sealing, virtual members, constructors, interfaces, and intended extensibility;
   - defaults and behavior consistency across core, relational, and providers.
6. Check source, binary, and behavioral compatibility. Prefer an additive overload over changing a shipped signature. When replacement is necessary, the old API should be preserved with `ObsoleteAttribute` with a message that points to the replacement unless established release policy permits removal.
7. Require complete XML documentation for the contract, including null behavior, defaults, cancellation, and provider limitations where relevant.
8. Verify implementations and overrides agree with the abstraction contract.
9. Before finishing, account explicitly for demonstrated need, contract documentation, and focused behavioral coverage. Do not recommend an additive overload until the need gate in Step 4 passes. When the proposed change omits documentation or tests for a justified added or replacement API, report that concrete omission; do not let compatibility findings displace this completeness check.

The final review must be explicit: reject or defer the new surface because need is not demonstrated or the shape needs to be reworked, or ensure that any justified additive API requires complete XML documentation and focused behavioral tests. Do not leave those obligations implicit.

## API Baselines

API baseline generation, baseline-file updates, and `EFCore.ApiBaseline.Tests` execution are owned by the repository's test infrastructure. Do not request them, flag their absence, or make them an API-review acceptance condition.

## Findings

Feed API findings into the main code-review format. A useful API finding identifies the affected member, the concrete compatibility or usability consequence, the established sibling pattern or contract, and a viable API shape. Do not emit a separate API report.

Do not report general Framework Design Guideline preferences unless they produce a concrete problem for this API or conflict with an established EF Core API conventions.
