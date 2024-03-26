// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// TODO: Temporary during development, remove these
#pragma warning disable CS0219 // Variable is assigned but its value is never used
#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable CS8321 // Local function is declared but never used

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     A source generator that identifies queryable LINQ queries, and checks if they're compatible with query precompilation (i.e. they're
///     a static chain of method invocation over an EF DbContext). For compatible queries, the terminating operator gets an interceptor
///     which injects an additional "safe marker" node into the query expression tree (when this marker is absent, runtime compilation will
///     fail). For incompatible queries, a warning diagnostic is reported.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class LinqQuerySourceGenerator : IIncrementalGenerator
{
    public const string Id = "EF1003";
    public const string DisableRuntimeCompilationMsbuildProperty = "build_property.EFNukeDynamic";

    private static readonly DiagnosticDescriptor DynamicQueryDiagnosticDescriptor
        // HACK: Work around dotnet/roslyn-analyzers#5890 by not using target-typed new
        = new DiagnosticDescriptor(
            Id,
            title: AnalyzerStrings.DynamicQueryTitle,
            messageFormat: AnalyzerStrings.DynamicQueryMessageFormat,
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    // TODO: Reference this from the publish-time source generator
    private const string InterceptorsNamespace = "Microsoft.EntityFrameworkCore.GeneratedInterceptors";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO: Also allow per-source-file metadata which enables/disables the analysis?
        // https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md#consume-msbuild-properties-and-metadata

        // TODO: ideally, the terminatingOperators pipeline below wouldn't even run if the source generator isn't enabled (this is an opt-in
        // TODO: source generator, at least for now). Does that happen if e.g. this pipeline is on the left side of the Combine()
        // TODO: specifically?
        var isGeneratorEnabled =
            context.AnalyzerConfigOptionsProvider.Select(
                (provider, _) =>
                    provider.GlobalOptions.TryGetValue(DisableRuntimeCompilationMsbuildProperty, out var configurationSwitch)
                    && configurationSwitch == "true");

                // context.AnalyzerConfigOptionsProvider.Select(
                //         (provider, _) =>
                //             provider.GlobalOptions.TryGetValue("build_property.EFNukeDynamic", out var nukeDynamicSwitch)
                //             && nukeDynamicSwitch.Equals("true", StringComparison.OrdinalIgnoreCase))
                // .SelectMany((e, _) => new[] { e })
                // .Where(e => e)
                // .Combine(terminatingOperators)
                // .Select((t, _) => t.Right);

        var terminatingOperators = context.SyntaxProvider
            .CreateSyntaxProvider(IsPossibleTerminatingOperator, ProcessLinqQuery)
            .Combine(isGeneratorEnabled)
            // Filter out empty operators (i.e. operator candidates that turned out to be LINQ-to-Objects), and also filter out everything
            // if the source generator is disabled.
            .Where(t => t is { Left.IsEmpty: false, Right: true })
            .Select((t, _) => t.Left)
            .WithTrackingName("EF" + nameof(LinqQuerySourceGenerator));

        // TODO: Currently all interceptors from the entire project go into the same file, which may be a lot to redo every time something
        // TODO: changes; maybe generate an interceptor file per source file, to limit the regeneration scope to a single file?
        // TODO: Is that possible (how)?
        // TODO: Possibly look at GroupWith() in ASP.NET: https://github.com/dotnet/aspnetcore/blob/main/src/Shared/RoslynUtils/IncrementalValuesProviderExtensions.cs#L11

        context.RegisterSourceOutput(
            terminatingOperators.Where(o => o.Diagnostic is not null),
            (context, terminatingOperator) => context.ReportDiagnostic(terminatingOperator.Diagnostic!));

        context.RegisterSourceOutput(
            terminatingOperators.Where(o => o.Diagnostic is null).Collect(),
            GenerateCode);
    }

    private static bool IsPossibleTerminatingOperator(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        => syntaxNode switch
        {
            InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax { Name: IdentifierNameSyntax { Identifier.Text: var identifier } }
                }
                // TODO: Is the perf here something to worry about? If I had FrozenSet maybe I'd use it (not available because of TFM)
                // TODO: There seem to be good optimizations around switch over strings, so this may be fine
                // TODO: (https://github.com/dotnet/roslyn/pull/66081).
                => identifier switch
                {
                    // On Enumerable
                    "AsEnumerable" or "ToArray" or "ToDictionary" or "ToHashSet" or "ToLookup" or "ToList"
                        // On EntityFrameworkQueryableExtensions
                        or "AsAsyncEnumerable" or "ToArrayAsync" or "ToDictionaryAsync" or "ToHashSetAsync" or "ToListAsync"
                        // or "ToLookupAsync"

                        // when syntaxNode.SyntaxTree.FilePath.Contains("Program.cs") // TODO: Hack for now, since the source gen runs on project references too (i.e. EF source code)??
                        => true,

                    _ => false
                },

            // TODO: Handle foreach
            ForEachStatementSyntax => false,

            _ => false
        };

    private TerminatorOperatorInfo ProcessLinqQuery(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        InvocationExpressionSyntax terminatingOperator;
        MemberAccessExpressionSyntax memberAccess;
        string? interceptorDeclaration;

        // Our input is a candidate query terminating node (e.g. ToList()); above we've just verified its name, we now verify that it's
        // actually the correct query operator (e.g. Enumerable.ToList() and not some other ToList()) by checking its symbol
        switch (context.Node)
        {
            case InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name: IdentifierNameSyntax { Identifier.Text: var identifier } } memberAccess2
            } invocation:
            {
                // TODO: Is there an advantage in doing this via Operations (as the ASP.NET generator does)?
                interceptorDeclaration = identifier switch
                {
                    // These sync terminating operators exist exist over IEnumerable only, so verify the actual argument is an IQueryable
                    // (otherwise this is just LINQ to Objects)
                    // On Enumerable:
                    "AsEnumerable" when IsEnumerableOperatorOverQueryable() => """
public static global::System.Collections.Generic.IEnumerable<TSource> AsEnumerable_Safe<TSource>(
    this global::System.Collections.Generic.IEnumerable<TSource> source)
{
    var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose(
        (global::System.Linq.IQueryable<TSource>)source);
    return global::System.Linq.Enumerable.AsEnumerable(safeWrapped);
}
""",
                    "ToArray" when IsEnumerableOperatorOverQueryable() => """
public static TSource[] ToArray_Safe<TSource>(this global::System.Collections.Generic.IEnumerable<TSource> source)
{
    var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose(
        (global::System.Linq.IQueryable<TSource>)source);
    return global::System.Linq.Enumerable.ToArray(safeWrapped);
}
""",
                    "ToDictionary" when IsEnumerableOperatorOverQueryable() => """
public static global::System.Collections.Generic.Dictionary<TKey, TElement> ToDictionary_Safe<TSource, TKey, TElement>(
  this global::System.Collections.Generic.IEnumerable<TSource> source,
  global::System.Func<TSource, TKey> keySelector,
  global::System.Func<TSource, TElement> elementSelector)
  where TKey : notnull
{
    var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose(
        (global::System.Linq.IQueryable<TSource>)source);
    return global::System.Linq.Enumerable.ToDictionary(safeWrapped, keySelector, elementSelector);
}
""",
                    "ToHashSet" when IsEnumerableOperatorOverQueryable() => """
public static global::System.Collections.Generic.HashSet<TSource> ToHashSet_Safe<TSource>(
    this global::System.Collections.Generic.IEnumerable<TSource> source)
{
    var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose(
        (global::System.Linq.IQueryable<TSource>)source);
    return global::System.Linq.Enumerable.ToHashSet(safeWrapped);
}
""",
                    "ToLookup" when IsEnumerableOperatorOverQueryable() => throw new NotImplementedException(),

                    "ToList" when IsEnumerableOperatorOverQueryable() => """
public static global::System.Collections.Generic.List<TSource> ToList_Safe<TSource>(
    this global::System.Collections.Generic.IEnumerable<TSource> source)
{
    var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose((
        global::System.Linq.IQueryable<TSource>)source);
    return global::System.Linq.Enumerable.ToList(safeWrapped);
}
""",

                    // On EntityFrameworkQueryableExtensions
                    "AsAsyncEnumerable" when IsOnEfQueryableExtensions() => """
public static global::System.Collections.Generic.IAsyncEnumerable<TSource> AsAsyncEnumerableAsync_Safe<TSource>(
    this global::System.Linq.IQueryable<TSource> source)
{
    var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose(source);
    return global::Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsAsyncEnumerable(safeWrapped);
}
""",
                    "ToArrayAsync" when IsOnEfQueryableExtensions() => """
public static global::System.Threading.Tasks.Task<TSource[]> ToArrayAsync_Safe<TSource>(
    this global::System.Linq.IQueryable<TSource> source,
    global::System.Threading.CancellationToken cancellationToken = default)
{
var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose(source);
return global::Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToArrayAsync(safeWrapped);
}
""",
                    "ToDictionaryAsync" when IsOnEfQueryableExtensions() => """
public static global::System.Threading.Tasks.Task<global::System.Collections.Generic.Dictionary<TKey, TElement>> ToDictionaryAsync_Safe<TSource, TKey, TElement>(
  this global::System.Linq.IQueryable<TSource> source,
  global::System.Func<TSource, TKey> keySelector,
  global::System.Func<TSource, TElement> elementSelector,
  global::System.Threading.CancellationToken cancellationToken = default)
  where TKey : notnull
{
    var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose(source);
    return global::Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToDictionaryAsync(
        safeWrapped, keySelector, elementSelector);
}
""",
                    "ToHashSetAsync" when IsOnEfQueryableExtensions() => """
public static global::System.Threading.Tasks.Task<global::System.Collections.Generic.HashSet<TSource>> ToHashSetAsync_Safe<TSource>(
    this global::System.Linq.IQueryable<TSource> source,
    global::System.Threading.CancellationToken cancellationToken = default)
{
var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose(source);
return global::Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToHashSetAsync(safeWrapped);
}
""",
                    "ToListAsync" when IsOnEfQueryableExtensions() => """
public static global::System.Threading.Tasks.Task<global::System.Collections.Generic.List<TSource>> ToListAsync_Safe<TSource>(
    this global::System.Linq.IQueryable<TSource> source,
    global::System.Threading.CancellationToken cancellationToken = default)
{
var safeWrapped = global::Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQuerySafeMarker.Compose(source);
return global::Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(safeWrapped);
}
""",
                    // "ToLookupAsync"

                    _ => default
                };

                // Check that we're actually dealing with a queryable LINQ query, with a well-known operator.
                if (interceptorDeclaration is null)
                {
                    return default;
                }

                terminatingOperator = invocation;
                memberAccess = memberAccess2;

                break;

                // TODO: we currently check symbols by their name; should we switch to loading symbols from the Compilation (via
                // CompilationProvider) - like the System.Text.Json gen does - or MetadataReferenceProvider? Do symbols from dependencies
                // (e.g. System.Linq.Enumerable) change across compilations?
                bool IsEnumerableOperatorOverQueryable()
                    => context.SemanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is IMethodSymbol
                        {
                            // TODO: Can check that the parameter is IEnumerable - but it's kind of useless, since we filter on specific method names anyway. May as well keep it like this.
                            ContainingType:
                            { Name: "Enumerable", ContainingNamespace: { Name: "Linq", ContainingNamespace.Name: "System" } }
                        }
                        && context.SemanticModel.GetSymbolInfo(memberAccess2.Expression, cancellationToken).Symbol switch
                        {
                            // Terminating operator over a method that returns an IQueryable, e.g. context.Blogs.Where(...).ToList()
                            // TODO: As an optimization, exclude methods defined on the Enumerable type before doing the more expensive
                            // TODO: IQueryable interface check?
                            IMethodSymbol { ReturnType: var returnType } when returnType.AllInterfaces.Any(
                                    i => i.OriginalDefinition is
                                    {
                                        Name: "IQueryable",
                                        ContainingNamespace:
                                        {
                                            Name: "Linq",
                                            ContainingNamespace.Name: "System"
                                        },
                                        ContainingAssembly.Name: "System.Linq.Expressions"
                                    })
                                => true,

                            // Terminating operator directly over DbSet property, e.g. context.Blogs.ToList()
                            IPropertySymbol
                            {
                                Type:
                                {
                                    Name: "DbSet",
                                    ContainingNamespace:
                                    {
                                        Name: "EntityFrameworkCore",
                                        ContainingNamespace.Name: "Microsoft"
                                    },
                                    ContainingAssembly.Name: "Microsoft.EntityFrameworkCore"
                                }
                            } => true,

                            // TODO: do we support DbSet fields as opposed to properties??

                            _ => false
                        };

                bool IsOnEfQueryableExtensions()
                    => context.SemanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is IMethodSymbol
                    {
                        ContainingType:
                        {
                            Name: "EntityFrameworkQueryableExtensions",
                            ContainingNamespace:
                            {
                                Name: "EntityFrameworkCore",
                                ContainingNamespace.Name: "Microsoft"
                            },
                            ContainingAssembly.Name: "Microsoft.EntityFrameworkCore"
                        }
                    };
            }

            // TODO: Handle foreach
            case ForEachStatementSyntax:
                return default;

            default:
                return default;
        }

        // At this point we know we're dealing with a queryable LINQ query. Check whether it's static - i.e. a string of unbroken method
        // invocations, rooted on an EF Core DbContext. If not, report a warning.
        if (!IsRootOnDbContext(terminatingOperator))
        {
            // TODO: Consider reporting the diagnostic on the whole fragment rather than only the terminating operator?
            return new TerminatorOperatorInfo(Diagnostic.Create(DynamicQueryDiagnosticDescriptor, terminatingOperator.GetLocation()));
        }

        // We now have a confirmed static EF query that needs to be intercepted.
        var syntaxTree = terminatingOperator.SyntaxTree;
        var startPosition = syntaxTree.GetLineSpan(memberAccess.Name.Span, cancellationToken).StartLinePosition;

        return new TerminatorOperatorInfo(syntaxTree.FilePath, startPosition.Line + 1, startPosition.Character + 1, interceptorDeclaration);

        bool IsRootOnDbContext(ExpressionSyntax expression)
        {
            // TODO: Carefully think about exactly what kind of verification we want to do here: static/non-static, actually get the
            // TODO: method symbols and confirm it's an IQueryable flowing all the way through, etc.

            // Work backwards through the LINQ operator chain until we reach something that isn't a method invocation
            while (expression is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax { Expression: var innerExpression }
                })
            {
                expression = innerExpression;
            }

            // We've reached a non-invocation.

            // First, check if this is a property access for a DbSet
            if (expression is MemberAccessExpressionSyntax { Expression: var innerExpression2 }
                && IsDbContext(innerExpression2))
            {
                // TODO: Check symbol for DbSet?
                return true;
            }

            // If we had context.Set<Blog>(), the Set() method was skipped like any other method, and we're on the context.
            return IsDbContext(expression);

            bool IsDbContext(ExpressionSyntax expression)
            {
                switch (ModelExtensions.GetSymbolInfo(context.SemanticModel, expression, cancellationToken).Symbol)
                {
                    case ILocalSymbol localSymbol:
                        return IsDbContextType(localSymbol.Type);

                    case IPropertySymbol:
                    case IFieldSymbol:
                    case IMethodSymbol:
                        return false; // TODO

                    case null:
                        return false;
                    default:
                        return false; // TODO: ?
                }

                bool IsDbContextType(ITypeSymbol typeSymbol)
                {
                    while (true)
                    {
                        if (typeSymbol is // TODO: Add assembly check
                            {
                                Name: "DbContext",
                                ContainingNamespace:
                                {
                                    Name: "EntityFrameworkCore",
                                    ContainingNamespace.Name: "Microsoft"
                                }
                            })
                        {
                            return true;
                        }

                        if (typeSymbol.BaseType is null)
                        {
                            return false;
                        }

                        typeSymbol = typeSymbol.BaseType;
                    }
                }
            }
        }
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<TerminatorOperatorInfo> terminatingOperators)
    {
        if (terminatingOperators.IsDefaultOrEmpty)
        {
            return;
        }

        // TODO: Add [GeneratedCode] to the publish-time generated code as well
        var code = new StringBuilder()
            .AppendLine(
                $$"""
// <auto-generated />
using System;
using System.Runtime.CompilerServices;

namespace {{InterceptorsNamespace}}
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("{{typeof(LinqQuerySourceGenerator).Assembly.FullName}}", "{{typeof(LinqQuerySourceGenerator).Assembly.GetName().Version}}")]
    file static class EntityFrameworkCoreInterceptors
    {
""");

        // TODO: Perf (GroupBy)?
        foreach (var interceptionGroup in terminatingOperators.GroupBy(o => o.InterceptorDeclaration!))
        {
            foreach (var terminatingOperator in interceptionGroup)
            {
                code.AppendLine(
                    $"""[InterceptsLocation("{terminatingOperator.FilePath}", {terminatingOperator.Line}, {terminatingOperator.Character})]""");
            }

            // TODO: Properly indent this for pretty generated code :)
            code.AppendLine(interceptionGroup.Key);
        }

        code.AppendLine(
            """
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute : Attribute
    {
        public InterceptsLocationAttribute(string filePath, int line, int column) { }
    }
}
""");

        context.AddSource("EFInterceptors.g.cs", code.ToString());
    }

    private readonly struct TerminatorOperatorInfo
        : IEquatable<TerminatorOperatorInfo>
    {
        public TerminatorOperatorInfo(string filePath, int line, int character, string interceptorDeclaration)
        {
            FilePath = filePath;
            Line = line;
            Character = character;
            InterceptorDeclaration = interceptorDeclaration;
        }

        public TerminatorOperatorInfo(Diagnostic? diagnostic)
        {
            Diagnostic = diagnostic;
        }

        public readonly string? InterceptorDeclaration;
        public readonly Diagnostic? Diagnostic;

        // TODO: Do these need to participate in the equality/hashcode check? By definition they change if the originating syntax node
        // changes etc.
        public readonly string? FilePath;
        public readonly int Line;
        public readonly int Character;

        public bool IsEmpty
            => InterceptorDeclaration is null && Diagnostic is null;

        public override bool Equals(object? obj)
            => obj is TerminatorOperatorInfo other && Equals(other);

        public bool Equals(TerminatorOperatorInfo other)
            => InterceptorDeclaration is null
                ? Diagnostic!.Equals(other.Diagnostic)
                // Interceptor declarations are always interned strings, so we skip calculating the hash codes and use reference comparison
                // instead
                : ReferenceEquals(InterceptorDeclaration, other.InterceptorDeclaration);
                    // && Line == other.Line
                    // && Character == other.Character
                    // && FilePath == other.FilePath;

        public override int GetHashCode()
        {
            unchecked
            {
                if (InterceptorDeclaration is null)
                {
                    return Diagnostic!.GetHashCode();
                }

                // Interceptor declarations are always interned strings, so we skip calculating the hash codes and use the default reference
                // hash code logic instead
                var hashCode = RuntimeHelpers.GetHashCode(InterceptorDeclaration);
                // hashCode = (hashCode * 397) ^ FilePath!.GetHashCode();
                // hashCode = (hashCode * 397) ^ Line;
                // hashCode = (hashCode * 397) ^ Character;
                return hashCode;
            }
        }
    }
}
