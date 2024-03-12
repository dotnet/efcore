// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryLocator : CSharpSyntaxRewriter, IQueryLocator
{
    private Compilation? _compilation;

#pragma warning disable CS8618 // Uninitialized non-nullable fields. We check _compilation to make sure LoadCompilation was invoked.
    private ITypeSymbol _genericIQueryableSymbol, _nonGenericIQueryableSymbol, _dbSetSymbol;
    private ITypeSymbol _enumerableSymbol, _queryableSymbol, _efQueryableExtensionsSymbol, _efRelationalQueryableExtensionsSymbol;
#pragma warning restore CS8618

    private SemanticModel? _currentSemanticModel;
    private int _queryCounter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SyntaxTree LocateQueries(SyntaxTree syntaxTree)
    {
        if (_compilation is null)
        {
            throw new InvalidOperationException("A compilation must be loaded.");
        }

        Check.DebugAssert(_compilation.SyntaxTrees.Contains(syntaxTree), "Given syntax tree isn't part of the compilation.");

        _queryCounter = 0;

        var oldRoot = syntaxTree.GetRoot();
        var newRoot = Visit(oldRoot);

        // Note that we rewrite the syntax tree for async methods, since SingleAsync inserts a sync Single node into
        // the tree, not SingleAsync.
        if (!ReferenceEquals(newRoot, oldRoot))
        {
            Debug.Assert(_queryCounter > 0);
            syntaxTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);
        }

        return syntaxTree;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax invocation)
    {
        // TODO: Support non-extension invocation syntax: var blogs = ToList(ctx.Blogs);
        if (invocation.Expression is not MemberAccessExpressionSyntax
            {
                Name: IdentifierNameSyntax { Identifier.Text: var identifier },
                Expression: var innerExpression
            })
        {
            return base.VisitInvocationExpression(invocation);
        }

        // First, pattern-match on the method name as a string; this avoids accessing the semantic model for each and
        // every invocation (more efficient).
        return identifier switch
        {
            // These sync terminating operators exist exist over IEnumerable only, so verify the actual argument is an IQueryable (otherwise
            // this is just LINQ to Objects)
            nameof(Enumerable.AsEnumerable)
                or nameof(Enumerable.ToArray)
                or nameof(Enumerable.ToDictionary)
                or nameof(Enumerable.ToHashSet)
                or nameof(Enumerable.ToLookup)
                or nameof(Enumerable.ToList)
                when IsOnEnumerable() && IsQueryable(innerExpression)
                => CheckAndAddQuery(invocation),

            // The async terminating operators are defined by EF, and accept an IQueryable - no need to look at the argument.
            nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable)
                or nameof(EntityFrameworkQueryableExtensions.ToArrayAsync)
                or nameof(EntityFrameworkQueryableExtensions.ToDictionaryAsync)
                or nameof(EntityFrameworkQueryableExtensions.ToHashSetAsync)
                // or nameof(EntityFrameworkQueryableExtensions.ToLookupAsync)
                or nameof(EntityFrameworkQueryableExtensions.ToListAsync)
                when IsOnEfQueryableExtensions()
                => CheckAndAddQuery(invocation),

            nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable)
                when IsOnEfQueryableExtensions() || IsOnTypeSymbol(_dbSetSymbol)
                => CheckAndAddQuery(invocation),

            nameof(Queryable.All)
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
                when IsOnQueryable()
                => CheckAndAddQuery(invocation),

            nameof(EntityFrameworkQueryableExtensions.AllAsync)
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
                when IsOnEfQueryableExtensions()
                => CheckAndAddQuery(invocation),

            nameof(RelationalQueryableExtensions.ExecuteDelete) or nameof(RelationalQueryableExtensions.ExecuteUpdate)
                when IsOnEfRelationalQueryableExtensions()
                => CheckAndAddQuery(invocation),

            nameof(RelationalQueryableExtensions.ExecuteDeleteAsync) or nameof(RelationalQueryableExtensions.ExecuteUpdateAsync)
                when IsOnEfRelationalQueryableExtensions()
                => CheckAndAddQuery(invocation),

            _ => base.VisitInvocationExpression(invocation)
        };

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
    public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax forEach)
    {
        // Note: a LINQ queryable can't be placed directly inside await foreach, since IQueryable does not extend
        // IAsyncEnumerable. So users need to add our AsAsyncEnumerable, which is detected above as a normal invocation.
        var visited = base.VisitForEachStatement(forEach);

        // C# interceptors can (currently) intercept only method calls, not property accesses; this means that we can't
        // TODO: Support DbSet() method call directly inside foreach/await foreach
        if (forEach.Expression is InvocationExpressionSyntax invocation && IsQueryable(invocation))
        {
            return forEach.WithExpression(CheckAndAddQuery(forEach.Expression));
        }

        return visited;
    }

    private ExpressionSyntax CheckAndAddQuery(ExpressionSyntax query)
    {
        // TODO: Drill down and see that there's a DbSet at the bottom (other LINQ providers may exist)

        Console.WriteLine("Located EF query candidate: " + query);

        _queryCounter++;

        // We annotate the expression as an EF query candidate, we'll search for nodes with these annotations later.
        return query.WithAdditionalAnnotations(new SyntaxAnnotation(IQueryLocator.EfQueryCandidateAnnotationKind, data: null));
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
