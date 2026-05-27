# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

This is a feature request rather than a bug report: the request is for built-in EF Core support for composing a reusable `Expression<Func<...>>` inside a larger query lambda via a natural C# syntax such as `fullName.Eval(customer)`, where the call would be expanded into an expression tree form that EF can translate.

Suggested classification:

- Type: Feature
- Area: `area-query`

No minimal repro was created because this is a feature request. The scenario is understandable from the provided sample and the supplied `IQueryExpressionInterceptor` workaround.

Possible duplicates/related issues:

- https://github.com/dotnet/efcore/issues/10497 appears to be the closest existing issue. It asks to make it easy to use methods returning `Expression<TDelegate>` inside LINQ queries, including patterns like `Expressions.IsPopular().Compile().Invoke(p)`. This issue is essentially the same product need, with a proposed `Eval` API shape.
- https://github.com/dotnet/efcore/issues/15670 is the broader backlog/design issue for LINQKit-style dynamic query expansion and expression reuse in EF Core.
- https://github.com/dotnet/efcore/issues/34866 is related, covering nested/reusable projection expressions. It was closed as working as expected for the specific `Compile()` behavior, but the use case overlaps with this feature request.
- https://github.com/dotnet/efcore/issues/17791 is related historical context around translating `Expression.Invoke`; that specific regression was completed, but it is not the same API/usability request.

Recommendation: mark this as `area-query` and consider closing as a duplicate of https://github.com/dotnet/efcore/issues/10497, or otherwise link it there as a concrete API-shape proposal for the same expression-composition scenario.
