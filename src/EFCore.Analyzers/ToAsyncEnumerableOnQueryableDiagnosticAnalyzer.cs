// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Reports calls to <c>System.Linq.AsyncEnumerable.ToAsyncEnumerable&lt;TSource&gt;(IEnumerable&lt;TSource&gt;)</c>
///     whose source is an <c>IQueryable&lt;T&gt;</c>. The conversion forces the queryable to be enumerated synchronously,
///     defeating the purpose of using <c>await foreach</c>. EF Core's <c>AsAsyncEnumerable&lt;T&gt;()</c> is the
///     intended async-friendly alternative.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ToAsyncEnumerableOnQueryableDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor
        = new(
            EFDiagnostics.ToAsyncEnumerableOnQueryable,
            title: AnalyzerStrings.ToAsyncEnumerableOnQueryableTitle,
            messageFormat: AnalyzerStrings.ToAsyncEnumerableOnQueryableMessageFormat,
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var targetMethod = invocation.TargetMethod;

        if (targetMethod.Name != "ToAsyncEnumerable")
        {
            return;
        }

        // Only flag the System.Linq.AsyncEnumerable.ToAsyncEnumerable<TSource>(IEnumerable<TSource>) overload.
        // Other libraries may define methods with the same name (e.g. on Channel<T>, IObservable<T>) — we don't
        // want to false-positive on those.
        var containingType = targetMethod.ContainingType;
        if (containingType is null
            || containingType.Name != "AsyncEnumerable"
            || containingType.ContainingNamespace?.ToDisplayString() != "System.Linq")
        {
            return;
        }

        // The source is exposed as Arguments[0].Value for an extension method invocation. Peel any
        // chained implicit conversions to recover the user-written expression's type — a single C#
        // expression can produce stacked IConversionOperation nodes in the Roslyn tree. An explicit
        // cast to IEnumerable<T> (e.g. ((IEnumerable<T>)q).ToAsyncEnumerable()) shows up as
        // IsImplicit: false, so the loop stops there and the underlying IQueryable type is hidden —
        // a deliberate opt-out. Mirrors the WalkDownConversion(predicate) helper used across
        // dotnet/roslyn-analyzers.
        if (invocation.Arguments.Length == 0)
        {
            return;
        }

        var sourceOperation = invocation.Arguments[0].Value;
        while (sourceOperation is IConversionOperation { IsImplicit: true } conversion)
        {
            sourceOperation = conversion.Operand;
        }

        if (sourceOperation.Type is not { } sourceType || !ImplementsGenericIQueryable(sourceType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, GetInvocationLocation(invocation)));
    }

    private static bool ImplementsGenericIQueryable(ITypeSymbol type)
    {
        if (IsGenericIQueryable(type))
        {
            return true;
        }

        foreach (var iface in type.AllInterfaces)
        {
            if (IsGenericIQueryable(iface))
            {
                return true;
            }
        }

        return false;

        static bool IsGenericIQueryable(ITypeSymbol candidate)
            => candidate is INamedTypeSymbol { Name: "IQueryable", TypeArguments.Length: 1 } named
                && named.ContainingNamespace?.ToDisplayString() == "System.Linq";
    }

    private static Location GetInvocationLocation(IInvocationOperation invocation)
    {
        if (invocation.Syntax is not InvocationExpressionSyntax invocationExpression)
        {
            return invocation.Syntax.GetLocation();
        }

        var targetNode = invocationExpression.Expression;

        while (targetNode is MemberAccessExpressionSyntax memberAccess)
        {
            targetNode = memberAccess.Name;
        }

        // Generic name case (e.g. `AsyncEnumerable.ToAsyncEnumerable<T>(q)`): point at just the identifier.
        if (targetNode is GenericNameSyntax genericName)
        {
            return genericName.Identifier.GetLocation();
        }

        return targetNode.GetLocation();
    }
}
