// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Statically analyzes user code and locates EF LINQ queries within it, by identifying well-known terminating operators
///     (e.g. <c>ToList</c>, <c>Single</c>).
/// </summary>
/// <remarks>
///     After a <see cref="Compilation" /> is loaded via <see cref="Initialize" />, <see cref="LocateQueries" /> is called repeatedly
///     for all syntax trees in the compilation.
/// </remarks>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class QueryLocator : CSharpSyntaxWalker
{
    private Compilation? _compilation;
    private Symbols _symbols;

    private SemanticModel _semanticModel = null!;
    private CancellationToken _cancellationToken;
    private List<InvocationExpressionSyntax> _locatedQueries = null!;
    private List<PrecompiledQueryCodeGenerator.QueryPrecompilationError> _precompilationErrors = null!;

    /// <summary>
    ///     Loads a new <see cref="Compilation" />, representing a user project in which to locate queries.
    /// </summary>
    /// <param name="compilation">A <see cref="Compilation" /> representing a user project.</param>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual void Initialize(Compilation compilation)
    {
        _compilation = compilation;
        _symbols = Symbols.Load(compilation);
    }

    /// <summary>
    ///     Locates EF LINQ queries within the given <see cref="SyntaxTree" />, which represents user code.
    /// </summary>
    /// <param name="syntaxTree">A <see cref="SyntaxTree" /> in which to locate EF LINQ queries.</param>
    /// <param name="precompilationErrors">
    ///     A list of errors populated with dynamic LINQ queries detected in <paramref name="syntaxTree" />.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A list of EF LINQ queries confirmed to be compatible with precompilation.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual IReadOnlyList<InvocationExpressionSyntax> LocateQueries(
        SyntaxTree syntaxTree,
        List<PrecompiledQueryCodeGenerator.QueryPrecompilationError> precompilationErrors,
        CancellationToken cancellationToken = default)
    {
        if (_compilation is null)
        {
            throw new UnreachableException("A compilation must be loaded.");
        }

        if (!_compilation.SyntaxTrees.Contains(syntaxTree))
        {
            throw new UnreachableException("Syntax tree isn't part of the loaded compilation.");
        }

        _cancellationToken = cancellationToken;
        _semanticModel = _compilation.GetSemanticModel(syntaxTree);
        _locatedQueries = new List<InvocationExpressionSyntax>();
        _precompilationErrors = precompilationErrors;
        Visit(syntaxTree.GetRoot(cancellationToken));

        return _locatedQueries;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
    {
        // TODO: Support non-extension invocation syntax: var blogs = ToList(ctx.Blogs);
        if (invocation.Expression is MemberAccessExpressionSyntax
            {
                Name: IdentifierNameSyntax { Identifier.Text: var identifier },
                Expression: var innerExpression
            })
        {
            // First, pattern-match on the method name as a string; this avoids accessing the semantic model for each and
            // every invocation (more efficient).
            switch (identifier)
            {
                // These sync terminating operators exist exist over IEnumerable only, so verify the actual argument is an IQueryable (otherwise
                // this is just LINQ to Objects)
                case nameof(Enumerable.AsEnumerable) or nameof(Enumerable.ToArray) or nameof(Enumerable.ToDictionary)
                    or nameof(Enumerable.ToHashSet) or nameof(Enumerable.ToLookup) or nameof(Enumerable.ToList)
                    when IsOnEnumerable() && IsQueryable(innerExpression):

                case nameof(IEnumerable.GetEnumerator)
                    when IsOnIEnumerable() && IsQueryable(innerExpression):

                // The async terminating operators are defined by EF, and accept an IQueryable - no need to look at the argument.
                case nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable)
                    or nameof(EntityFrameworkQueryableExtensions.ToArrayAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ToDictionaryAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ToHashSetAsync)
                    // or nameof(EntityFrameworkQueryableExtensions.ToLookupAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ToListAsync)
                    when IsOnEfQueryableExtensions():

                case nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable)
                    when IsOnEfQueryableExtensions() || IsOnTypeSymbol(_symbols.DbSet):

                case nameof(Queryable.All)
                    or nameof(Queryable.Any)
                    or nameof(Queryable.Average)
                    or nameof(Queryable.Contains)
                    or nameof(Queryable.Count)
                    or nameof(Queryable.DefaultIfEmpty)
                    or nameof(Queryable.ElementAt)
                    or nameof(Queryable.ElementAtOrDefault)
                    or nameof(Queryable.First)
                    or nameof(Queryable.FirstOrDefault)
                    or nameof(Queryable.Last)
                    or nameof(Queryable.LastOrDefault)
                    or nameof(Queryable.LongCount)
                    or nameof(Queryable.Max)
                    or nameof(Queryable.MaxBy)
                    or nameof(Queryable.Min)
                    or nameof(Queryable.MinBy)
                    or nameof(Queryable.Single)
                    or nameof(Queryable.SingleOrDefault)
                    or nameof(Queryable.Sum)
                    when IsOnQueryable():

                case nameof(EntityFrameworkQueryableExtensions.AllAsync)
                    or nameof(EntityFrameworkQueryableExtensions.AnyAsync)
                    or nameof(EntityFrameworkQueryableExtensions.AverageAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ContainsAsync)
                    or nameof(EntityFrameworkQueryableExtensions.CountAsync)
                    // or nameof(EntityFrameworkQueryableExtensions.DefaultIfEmptyAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ElementAtAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ElementAtOrDefaultAsync)
                    or nameof(EntityFrameworkQueryableExtensions.FirstAsync)
                    or nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync)
                    or nameof(EntityFrameworkQueryableExtensions.LastAsync)
                    or nameof(EntityFrameworkQueryableExtensions.LastOrDefaultAsync)
                    or nameof(EntityFrameworkQueryableExtensions.LongCountAsync)
                    or nameof(EntityFrameworkQueryableExtensions.MaxAsync)
                    // or nameof(EntityFrameworkQueryableExtensions.MaxByAsync)
                    or nameof(EntityFrameworkQueryableExtensions.MinAsync)
                    // or nameof(EntityFrameworkQueryableExtensions.MinByAsync)
                    or nameof(EntityFrameworkQueryableExtensions.SingleAsync)
                    or nameof(EntityFrameworkQueryableExtensions.SingleOrDefaultAsync)
                    or nameof(EntityFrameworkQueryableExtensions.SumAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ForEachAsync)
                    when IsOnEfQueryableExtensions():

                case nameof(EntityFrameworkQueryableExtensions.ExecuteDelete)
                    or nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate)
                    or nameof(EntityFrameworkQueryableExtensions.ExecuteDeleteAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ExecuteUpdateAsync)
                    when IsOnEfQueryableExtensions():
                    if (ProcessQueryCandidate(invocation))
                    {
                        return;
                    }

                    break;
            }
        }

        base.VisitInvocationExpression(invocation);

        bool IsOnEnumerable()
            => IsOnTypeSymbol(_symbols.Enumerable);

        bool IsOnIEnumerable()
            => IsOnTypeSymbol(_symbols.IEnumerableOfT);

        bool IsOnQueryable()
            => IsOnTypeSymbol(_symbols.Queryable);

        bool IsOnEfQueryableExtensions()
            => IsOnTypeSymbol(_symbols.EfQueryableExtensions);

        bool IsOnTypeSymbol(ITypeSymbol typeSymbol)
            => _semanticModel.GetSymbolInfo(invocation, _cancellationToken).Symbol is IMethodSymbol methodSymbol
                && methodSymbol.ContainingType.OriginalDefinition.Equals(typeSymbol, SymbolEqualityComparer.Default);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void VisitForEachStatement(ForEachStatementSyntax forEach)
    {
        // Note: a LINQ queryable can't be placed directly inside await foreach, since IQueryable does not extend
        // IAsyncEnumerable. So users need to add our AsAsyncEnumerable, which is detected above as a normal invocation.

        // C# interceptors can (currently) intercept only method calls, not property accesses; this means that we can't
        // TODO: Support DbSet() method call directly inside foreach/await foreach
        // TODO: Warn for DbSet property access directly inside foreach (can't be intercepted so not supported)
        if (forEach.Expression is InvocationExpressionSyntax invocation
            && IsQueryable(invocation)
            && ProcessQueryCandidate(invocation))
        {
            return;
        }

        base.VisitForEachStatement(forEach);
    }

    private bool ProcessQueryCandidate(InvocationExpressionSyntax query)
    {
        // TODO: Carefully think about exactly what kind of verification we want to do here: static/non-static, actually get the
        // TODO: method symbols and confirm it's an IQueryable flowing all the way through, etc.
        // TODO: Move this code out, for reuse in the inner loop source generator

        // Work backwards through the LINQ operator chain until we reach something that isn't a method invocation
        ExpressionSyntax operatorSyntax = query;
        while (operatorSyntax is InvocationExpressionSyntax
               {
                   Expression: MemberAccessExpressionSyntax { Expression: var innerExpression }
               })
        {
            if (innerExpression is QueryExpressionSyntax or ParenthesizedExpressionSyntax { Expression: QueryExpressionSyntax })
            {
                _precompilationErrors.Add(
                    new PrecompiledQueryCodeGenerator.QueryPrecompilationError(
                        query, new InvalidOperationException(DesignStrings.QueryComprehensionSyntaxNotSupportedInPrecompiledQueries)));
                return false;
            }

            operatorSyntax = innerExpression;
        }

        // We've reached a non-invocation.

        // First, check if this is a property access for a DbSet
        if (operatorSyntax is MemberAccessExpressionSyntax { Expression: var innerExpression2 }
            && IsDbContext(innerExpression2))
        {
            _locatedQueries.Add(query);

            // TODO: Check symbol for DbSet?
            return true;
        }

        // If we had context.Set<Blog>(), the Set() method was skipped like any other method, and we're on the context.
        if (IsDbContext(operatorSyntax))
        {
            _locatedQueries.Add(query);
            return true;
        }

        _precompilationErrors.Add(
            new PrecompiledQueryCodeGenerator.QueryPrecompilationError(
                query, new InvalidOperationException(DesignStrings.DynamicQueryNotSupported)));
        return false;

        bool IsDbContext(ExpressionSyntax expression)
        {
            return _semanticModel.GetSymbolInfo(expression, _cancellationToken).Symbol switch
            {
                ILocalSymbol localSymbol => IsDbContextType(localSymbol.Type),
                IPropertySymbol propertySymbol => IsDbContextType(propertySymbol.Type),
                IFieldSymbol fieldSymbol => IsDbContextType(fieldSymbol.Type),
                IMethodSymbol methodSymbol => IsDbContextType(methodSymbol.ReturnType),
                _ => false
            };

            bool IsDbContextType(ITypeSymbol typeSymbol)
            {
                while (true)
                {
                    // TODO: Check for the user's specific DbContext type #33866
                    if (typeSymbol.Equals(_symbols.DbContext, SymbolEqualityComparer.Default))
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

    private bool IsQueryable(ExpressionSyntax expression)
        => _semanticModel.GetSymbolInfo(expression, _cancellationToken).Symbol switch
        {
            IMethodSymbol methodSymbol
                => methodSymbol.ReturnType.OriginalDefinition.Equals(_symbols.IQueryableOfT, SymbolEqualityComparer.Default)
                || methodSymbol.ReturnType.OriginalDefinition.AllInterfaces
                    .Contains(_symbols.IQueryable, SymbolEqualityComparer.Default),

            IPropertySymbol propertySymbol => IsDbSet(propertySymbol.Type),

            _ => false
        };

    // TODO: Handle DbSet subclasses which aren't InternalDbSet?
    private bool IsDbSet(ITypeSymbol typeSymbol)
        => SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, _symbols.DbSet);

    private readonly struct Symbols
    {
        private readonly Compilation _compilation;

        // ReSharper disable InconsistentNaming
        public readonly INamedTypeSymbol IQueryableOfT;
        public readonly INamedTypeSymbol IQueryable;
        public readonly INamedTypeSymbol DbContext;
        public readonly INamedTypeSymbol DbSet;

        public readonly INamedTypeSymbol Enumerable;
        public readonly INamedTypeSymbol IEnumerableOfT;
        public readonly INamedTypeSymbol Queryable;
        public readonly INamedTypeSymbol EfQueryableExtensions;
        // ReSharper restore InconsistentNaming

        private Symbols(Compilation compilation)
        {
            _compilation = compilation;

            IQueryableOfT = GetTypeSymbolOrThrow("System.Linq.IQueryable`1");
            IQueryable = GetTypeSymbolOrThrow("System.Linq.IQueryable");
            DbContext = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.DbContext");
            DbSet = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.DbSet`1");

            Enumerable = GetTypeSymbolOrThrow("System.Linq.Enumerable");
            IEnumerableOfT = GetTypeSymbolOrThrow("System.Collections.Generic.IEnumerable`1");
            Queryable = GetTypeSymbolOrThrow("System.Linq.Queryable");
            EfQueryableExtensions = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");
        }

        public static Symbols Load(Compilation compilation)
            => new(compilation);

        private INamedTypeSymbol GetTypeSymbolOrThrow(string fullyQualifiedMetadataName)
            => _compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
                ?? throw new InvalidOperationException("Could not find type symbol for: " + fullyQualifiedMetadataName);
    }
}
