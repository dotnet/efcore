// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
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
    private ITypeSymbol _efQueryableExtensionsSymbol, _enumerableSymbol, _queryableSymbol;
    private ITypeSymbol _cancellationTokenSymbol;
#pragma warning restore CS8618

    private SemanticModel? _currentSemanticModel;
    private bool _foundQueries;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<SyntaxTree> SyntaxTreesWithQueryCandidates => _syntaxTreesWithQueryCandidates;

    private readonly List<SyntaxTree> _syntaxTreesWithQueryCandidates = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void LoadCompilation(Compilation compilation)
    {
        _compilation = compilation;

        _genericIQueryableSymbol = GetTypeSymbolOrThrow("System.Linq.IQueryable`1");
        _nonGenericIQueryableSymbol = GetTypeSymbolOrThrow("System.Linq.IQueryable");
        _dbSetSymbol = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.DbSet`1");

        _efQueryableExtensionsSymbol =
            GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");
        _enumerableSymbol = GetTypeSymbolOrThrow("System.Linq.Enumerable");
        _queryableSymbol = GetTypeSymbolOrThrow("System.Linq.Queryable");
        _cancellationTokenSymbol = GetTypeSymbolOrThrow("System.Threading.CancellationToken");

        _syntaxTreesWithQueryCandidates.Clear();

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
    public SyntaxTree LocateQueries(SyntaxTree syntaxTree)
    {
        if (_compilation is null)
        {
            throw new InvalidOperationException("A compilation must be loaded.");
        }

        Check.DebugAssert(_compilation.SyntaxTrees.Contains(syntaxTree), "Given syntax tree isn't part of the compilation.");

        _foundQueries = false;

        var oldRoot = syntaxTree.GetRoot();
        var newRoot = Visit(oldRoot);

        // Note that we rewrite the syntax tree for async methods, since SingleAsync inserts a sync Single node into
        // the tree, not SingleAsync.
        if (!ReferenceEquals(newRoot, oldRoot))
        {
            Debug.Assert(_foundQueries);
            syntaxTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);
        }

        if (_foundQueries)
        {
            _syntaxTreesWithQueryCandidates.Add(syntaxTree);
        }

        return syntaxTree;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax invocation)
    {
        // TODO: Skip visiting identified candidates, there's no point.
        var visitedInvocation = (InvocationExpressionSyntax)base.VisitInvocationExpression(invocation)!;

        // TODO: Support non-extension invocation syntax: var blogs = ToList(ctx.Blogs);
        if (visitedInvocation.Expression is not MemberAccessExpressionSyntax
            {
                Name: IdentifierNameSyntax { Identifier.Text : var identifier },
                Expression: var innerExpression
            } memberAccess)
        {
            return visitedInvocation;
        }

        // First, pattern-match on the method name as a string; this avoids accessing the semantic model for each and
        // every invocation (more efficient).
        //
        // Some terminating operators need to go into the query tree (Single), others not (ToList).
        // Note that checking whether the method's parameter is an IQueryable or not isn't sufficient (e.g.
        // ToListAsync accepts an IQueryable parameter but should not be part of the query tree).
        switch (identifier)
        {
            // Sync ToList, ToArray and AsEnumerable exist over IEnumerable only, so verify the actual argument is an
            // IQueryable (otherwise this is just LINQ to Objects)
            // Also, the terminating operator in these cases should not be part of the expression tree.
            case nameof(Enumerable.ToList):
            case nameof(Enumerable.ToArray):
            case nameof(Enumerable.AsEnumerable):
                return IsOnEnumerable() && IsQueryable(innerExpression)
                    ? CheckAndAddQuery(innerExpression, async: false)
                    : visitedInvocation;

            case nameof(EntityFrameworkQueryableExtensions.ToListAsync):
            case nameof(EntityFrameworkQueryableExtensions.ToArrayAsync):
            case nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable):
                return IsOnEfQueryableExtensions()
                    ? CheckAndAddQuery(innerExpression, async: true)
                    : visitedInvocation;

            case nameof(Queryable.All):
            case nameof(Queryable.Any):
            case nameof(Queryable.Average):
            case nameof(Queryable.Contains):
            case nameof(Queryable.Count):
            case nameof(Queryable.DefaultIfEmpty):
            case nameof(Queryable.ElementAt):
            case nameof(Queryable.ElementAtOrDefault):
            case nameof(Queryable.First):
            case nameof(Queryable.FirstOrDefault):
            case nameof(Queryable.Last):
            case nameof(Queryable.LastOrDefault):
            case nameof(Queryable.LongCount):
            case nameof(Queryable.Max):
            case nameof(Queryable.MaxBy):
            case nameof(Queryable.Min):
            case nameof(Queryable.MinBy):
            case nameof(Queryable.Single):
            case nameof(Queryable.SingleOrDefault):
            case nameof(Queryable.Sum):
                return IsOnQueryable()
                    ? CheckAndAddQuery(visitedInvocation, async: false)
                    : visitedInvocation;

            case nameof(EntityFrameworkQueryableExtensions.AllAsync):
            case nameof(EntityFrameworkQueryableExtensions.AnyAsync):
            case nameof(EntityFrameworkQueryableExtensions.AverageAsync):
            case nameof(EntityFrameworkQueryableExtensions.ContainsAsync):
            case nameof(EntityFrameworkQueryableExtensions.CountAsync):
            // case nameof(EntityFrameworkQueryableExtensions.DefaultIfEmptyAsync):
            case nameof(EntityFrameworkQueryableExtensions.ElementAtAsync):
            case nameof(EntityFrameworkQueryableExtensions.ElementAtOrDefaultAsync):
            case nameof(EntityFrameworkQueryableExtensions.FirstAsync):
            case nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync):
            case nameof(EntityFrameworkQueryableExtensions.LastAsync):
            case nameof(EntityFrameworkQueryableExtensions.LastOrDefaultAsync):
            case nameof(EntityFrameworkQueryableExtensions.LongCountAsync):
            case nameof(EntityFrameworkQueryableExtensions.MaxAsync):
            // case nameof(EntityFrameworkQueryableExtensions.MaxByAsync):
            case nameof(EntityFrameworkQueryableExtensions.MinAsync):
            // case nameof(EntityFrameworkQueryableExtensions.MinByAsync):
            case nameof(EntityFrameworkQueryableExtensions.SingleAsync):
            case nameof(EntityFrameworkQueryableExtensions.SingleOrDefaultAsync):
            case nameof(EntityFrameworkQueryableExtensions.SumAsync):
                return IsOnEfQueryableExtensions() && TryRewriteInvocationToSync(out var rewrittenSyncInvocation)
                    ? CheckAndAddQuery(rewrittenSyncInvocation, async: true)
                    : visitedInvocation;

            default:
                return visitedInvocation;
        }

        bool IsOnEfQueryableExtensions()
            => IsOnTypeSymbol(_efQueryableExtensionsSymbol);

        bool IsOnEnumerable()
            => IsOnTypeSymbol(_enumerableSymbol);

        bool IsOnQueryable()
            => IsOnTypeSymbol(_queryableSymbol);

        bool IsOnTypeSymbol(ITypeSymbol typeSymbol)
        {
            if (GetSymbol(visitedInvocation) is not IMethodSymbol methodSymbol)
            {
                Console.WriteLine("Couldn't get method symbol for invocation: " + visitedInvocation);
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, typeSymbol);
        }

        bool TryRewriteInvocationToSync([NotNullWhen(true)] out InvocationExpressionSyntax? syncInvocation)
        {
            // Chop off the Async suffix
            Debug.Assert(identifier.EndsWith("Async", StringComparison.Ordinal));
            var syncMethodName = identifier.Substring(0, identifier.Length - "Async".Length);

            // If the last argument is a cancellation token, chop it off
            var arguments = visitedInvocation.ArgumentList.Arguments;
            if (GetSymbol(visitedInvocation) is not IMethodSymbol methodSymbol)
            {
                syncInvocation = null;
                return false;
            }

            if (SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[^1].Type, _cancellationTokenSymbol)
                && visitedInvocation.ArgumentList.Arguments.Count == methodSymbol.Parameters.Length)
            {
                arguments = arguments.RemoveAt(arguments.Count - 1);
            }

            syncInvocation = visitedInvocation.Update(
                    memberAccess.Update(
                        memberAccess.Expression,
                        memberAccess.OperatorToken,
                        SyntaxFactory.IdentifierName(syncMethodName)),
                    visitedInvocation.ArgumentList.WithArguments(arguments));

            return true;
        }
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

        // TODO: Support DbSet directly inside await foreach
        if (IsQueryable(forEach.Expression))
        {
            return forEach.WithExpression(CheckAndAddQuery(forEach.Expression, async: false));
        }

        return visited;
    }

    private ExpressionSyntax CheckAndAddQuery(ExpressionSyntax query, bool async)
    {
        // TODO: Drill down and see that there's a DbSet at the bottom (other LINQ providers may exist)

        Console.WriteLine("Located EF query candidate: " + query);

        _foundQueries = true;

        // We annotate the expression as an EF query candidate, preserving the sync/async information.
        // We'll search for nodes with these annotations later.
        return query.WithAdditionalAnnotations(
            new SyntaxAnnotation(IQueryLocator.EfQueryCandidateAnnotationKind, async ? "Async" : "Sync"));
    }

    private bool IsQueryable(ExpressionSyntax expression)
    {
        var symbol = GetSymbol(expression);
        switch (symbol)
        {
            case IMethodSymbol methodSymbol:
                return SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType.OriginalDefinition, _genericIQueryableSymbol) ||
                       methodSymbol.ReturnType.OriginalDefinition.AllInterfaces.Contains(_nonGenericIQueryableSymbol, SymbolEqualityComparer.Default);

            case IPropertySymbol propertySymbol:
                return IsDbSet(propertySymbol.Type);

            // TODO: Other cases of DbSet, e.g. field, local variable...

            case null:
                Console.WriteLine("Could not resolve symbol for query: " + expression);
                return false;

            default:
                // Console.WriteLine($"Unexpected symbol type '{symbol.GetType().Name}' for symbol '{symbol}' for query: " + expression);
                return false;
        }
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
