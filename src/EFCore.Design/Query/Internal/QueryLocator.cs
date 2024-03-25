// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Statically analyzes user code and locates EF LINQ queries within it, by identifying well-known terminating operators
///     (e.g. <c>ToList</c>, <c>Single</c>).
/// </summary>
/// <remarks>
///     After a <see cref="Compilation" /> is loaded via <see cref="LoadCompilation" />, <see cref="LocateQueries" /> is called repeatedly
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

#pragma warning disable CS8618 // Uninitialized non-nullable fields. We check _compilation to make sure LoadCompilation was invoked.
    private ITypeSymbol _genericIQueryableSymbol, _nonGenericIQueryableSymbol, _dbSetSymbol;
    private ITypeSymbol _enumerableSymbol, _queryableSymbol, _efQueryableExtensionsSymbol, _efRelationalQueryableExtensionsSymbol;
#pragma warning restore CS8618

    private SemanticModel? _currentSemanticModel;
    private List<InvocationExpressionSyntax> _locatedQueries = null!;

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
    public virtual void LoadCompilation(Compilation compilation)
    {
        _compilation = compilation;

        _genericIQueryableSymbol = GetTypeSymbolOrThrow("System.Linq.IQueryable`1");
        _nonGenericIQueryableSymbol = GetTypeSymbolOrThrow("System.Linq.IQueryable");
        _dbSetSymbol = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.DbSet`1");

        _enumerableSymbol = GetTypeSymbolOrThrow("System.Linq.Enumerable");
        _queryableSymbol = GetTypeSymbolOrThrow("System.Linq.Queryable");
        _efQueryableExtensionsSymbol = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");
        _efRelationalQueryableExtensionsSymbol = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.RelationalQueryableExtensions");

        ITypeSymbol GetTypeSymbolOrThrow(string fullyQualifiedMetadataName)
            => _compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
               ?? throw new InvalidOperationException("Could not find type symbol for: " + fullyQualifiedMetadataName);
    }

    /// <summary>
    ///     Locates EF LINQ queries within the given <see cref="SyntaxTree" />, which represents user code.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         In some cases, the provided <see cref="SyntaxTree" /> must be rewritten (since async invocations such as <c>SingleAsync</c>
    ///         inject a sync <c>Single</c> node). As a result, this method returns a possibly-rewritten <see cref="SyntaxTree" />.
    ///     </para>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    /// </remarks>
    /// <param name="syntaxTree">A <see cref="SyntaxTree" /> in which to locate EF LINQ queries.</param>
    /// <returns>A possibly rewritten <see cref="SyntaxTree" />.</returns>
    public virtual IReadOnlyList<InvocationExpressionSyntax> LocateQueries(SyntaxTree syntaxTree)
    {
        if (_compilation is null)
        {
            throw new InvalidOperationException("A compilation must be loaded.");
        }

        Check.DebugAssert(_compilation.SyntaxTrees.Contains(syntaxTree), "Given syntax tree isn't part of the compilation.");

        _locatedQueries = new();
        Visit(syntaxTree.GetRoot());
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

                // The async terminating operators are defined by EF, and accept an IQueryable - no need to look at the argument.
                case nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable)
                    or nameof(EntityFrameworkQueryableExtensions.ToArrayAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ToDictionaryAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ToHashSetAsync)
                    // or nameof(EntityFrameworkQueryableExtensions.ToLookupAsync)
                    or nameof(EntityFrameworkQueryableExtensions.ToListAsync)
                    when IsOnEfQueryableExtensions():

                case nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable)
                    when IsOnEfQueryableExtensions() || IsOnTypeSymbol(_dbSetSymbol):

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

                case nameof(RelationalQueryableExtensions.ExecuteDelete)
                    or nameof(RelationalQueryableExtensions.ExecuteUpdate)
                    when IsOnEfRelationalQueryableExtensions():

                case nameof(RelationalQueryableExtensions.ExecuteDeleteAsync) or nameof(RelationalQueryableExtensions.ExecuteUpdateAsync)
                    when IsOnEfRelationalQueryableExtensions():
                    if (TryProcessQuery(invocation))
                    {
                        return;
                    }

                    break;
            }
        }

        base.VisitInvocationExpression(invocation);

        bool IsOnEnumerable()
            => IsOnTypeSymbol(_enumerableSymbol);

        bool IsOnQueryable()
            => IsOnTypeSymbol(_queryableSymbol);

        bool IsOnEfQueryableExtensions()
            => IsOnTypeSymbol(_efQueryableExtensionsSymbol);

        bool IsOnEfRelationalQueryableExtensions()
            => IsOnTypeSymbol(_efRelationalQueryableExtensionsSymbol);

        bool IsOnTypeSymbol(ITypeSymbol typeSymbol)
            => GetSymbol(invocation) is IMethodSymbol methodSymbol
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
        if (forEach.Expression is InvocationExpressionSyntax invocation
            && IsQueryable(invocation)
            && TryProcessQuery(invocation))
        {
            return;
        }

        base.VisitForEachStatement(forEach);
    }

    private bool TryProcessQuery(InvocationExpressionSyntax query)
    {
        // TODO: Drill down and see that there's a DbSet at the bottom (other LINQ providers may exist)
        _locatedQueries.Add(query);
        return true;
    }

    private bool IsQueryable(ExpressionSyntax expression)
    {
        var symbol = GetSymbol(expression);
        return symbol switch
        {
            IMethodSymbol methodSymbol
                => SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType.OriginalDefinition, _genericIQueryableSymbol)
                || methodSymbol.ReturnType.OriginalDefinition.AllInterfaces
                    .Contains(_nonGenericIQueryableSymbol, SymbolEqualityComparer.Default),

            IPropertySymbol propertySymbol => IsDbSet(propertySymbol.Type),
            // TODO: Other cases of DbSet, e.g. field, local variable...

            _ => false
        };
    }

    private ISymbol? GetSymbol(SyntaxNode node)
    {
        if (_currentSemanticModel?.SyntaxTree != node.SyntaxTree)
        {
            _currentSemanticModel = _compilation!.GetSemanticModel(node.SyntaxTree);
        }

        var symbol = _currentSemanticModel.GetSymbolInfo(node).Symbol;

        if (symbol is null)
        {
            Console.WriteLine("Could not find symbol for: " + node);
        }

        return symbol;
    }

    // TODO: Handle DbSet subclasses which aren't InternalDbSet?
    private bool IsDbSet(ITypeSymbol typeSymbol)
        => SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, _dbSetSymbol);
}
