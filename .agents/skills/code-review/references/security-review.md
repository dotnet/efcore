# EF Core Security Review

Use this reference for a scoped review of security-sensitive changes. Read `docs/threat_model.md` first and treat its trust boundaries, guarantees, assumptions, and mitigations as authoritative for EF Core.

## Triggers

Apply this review when a change affects:

- data received from a database, network, configuration, command line, or other trust boundary;
- SQL or database-command construction and parameterization;
- JSON or other serialization and deserialization;
- scaffolding or generated code derived from database metadata;
- logging, diagnostics, exceptions, connection strings, secrets, or sensitive values;
- caches and attacker-influenced key cardinality;
- recursion, result size, parsing, or other complexity bounds;
- migrations locks, runtime DDL privileges, tooling assembly loading, or design-time project loading;
- providers, plug-ins, interceptors, or extension points that can replace trusted behavior.

Paths and words are signals, not findings. Activate this review from the changed data flow and risk.

## Threat Trace

For each plausible concern, establish:

1. **Source**: what an attacker or less-trusted party controls.
2. **Boundary**: where the value enters EF Core or crosses into another component.
3. **Transformation**: parsing, validation, normalization, escaping, caching, or censoring applied.
4. **Sink and asset**: where the value is executed, emitted, logged, loaded, or retained and what is at risk.
5. **Abuse path**: a concrete input and sequence that reaches the impact.
6. **Existing mitigation**: unchanged callers, helpers, limits, driver behavior, configuration gates, and tests.

Use STRIDE categories as a completeness aid, not as a substitute for this trace.

## EF Core Guarantees and Checks

- LINQ and interpolated SQL paths parameterize values. Raw SQL APIs deliberately accept responsibility from the application; do not misclassify documented raw behavior as an EF vulnerability.
- Sensitive parameter values are censored from logging and exceptions by default. Any new exposure must remain behind the established explicit opt-in.
- Deserialization and cache-key work influenced by untrusted data must remain bounded as documented. Check complexity, recursion depth, allocation growth, cache cardinality, and eviction rather than only malformed-input handling.
- Scaffolding must generate safe C# even when database identifiers, comments, and metadata are malicious. Follow data through identifier normalization, string/XML escaping, templates, and generated-code compilation.
- The model is trusted. Do not invent an untrusted-model threat unless the change alters that assumption.
- Migrations and tooling load trusted application code and may use elevated privileges. Check that changes do not move untrusted input into assembly loading or encourage runtime DDL credentials for normal application work.
- Providers and plug-ins can replace behavior. Identify whether a mitigation belongs to EF Core, the lower-level driver, or the extension contract before assigning a finding.

## Verification and Tests

Search the full relevant implementation for defenses before reporting. A security finding requires a plausible abuse path and repository evidence that the mitigation is absent or weakened. State assumptions and downgrade or discard findings that cannot be verified.

For each identified plausible abuse path request an adversarial regression test that exercises the malicious input or resource-boundary condition, not merely a happy-path unit test. For generated-code risks, the test must pass the hostile metadata through the complete generator and compile the emitted source (or perform the repository's equivalent end-to-end validity check), proving the payload remains inert as well as syntactically valid. A helper-output assertion alone is insufficient. Never reproduce secrets or sensitive environment values in review output.

Recommend updating `docs/threat_model.md` only when the change adds or alters a trust boundary, assumption, threat, guarantee, or mitigation. A scoped code review is not a broad security audit.
