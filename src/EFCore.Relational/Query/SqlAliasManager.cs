// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A stateful manager for SQL aliases, capable of generate uniquified table aliases and rewriting them in post-processing.
///     An instance of <see cref="SqlAliasManager" /> is valid for a single query compilation, and is owned by
///     <see cref="RelationalQueryCompilationContext" />.
/// </summary>
public class SqlAliasManager
{
    /// <summary>
    ///     Maps alias prefixes to the highest number postfix currently in use.
    /// </summary>
    private readonly Dictionary<char, MutableInt> _aliases = new();

    /// <summary>
    ///     Generates an alias based on the given <paramref name="name" />.
    ///     All aliases produced by a given instance of <see cref="SqlAliasManager" /> are unique.
    /// </summary>
    /// <param name="name">A name (e.g. of a table) to use as the starting point for the aliasA base for the alias; a number postfix will be appended to it as necessary.</param>
    /// <returns>A fully unique alias within the context of this translation process.</returns>
    public virtual string GenerateTableAlias(string name)
    {
        var firstChar = char.ToLowerInvariant(name[0]);

        if (_aliases.TryGetValue(firstChar, out var counter))
        {
            return firstChar.ToString() + counter.Value++;
        }

        _aliases[firstChar] = new MutableInt { Value = 0 };
        return firstChar.ToString();
    }

    /// <summary>
    ///     Generates an alias based on the given <paramref name="modelTable" />.
    ///     All aliases produced by a given instance of <see cref="SqlAliasManager" /> are unique.
    /// </summary>
    /// <param name="modelTable">A table from the relational model for which to generate the alias.</param>
    /// <returns>A fully unique alias within the context of this translation process.</returns>
    public virtual string GenerateTableAlias(ITableBase modelTable)
        => GenerateTableAlias(modelTable.Name);

    /// <summary>
    ///     Performs a post-processing pass over aliases in the provided SQL tree, closing any gaps.
    /// </summary>
    /// <param name="expression">The SQL tree to post-process.</param>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    [EntityFrameworkInternal]
    public virtual Expression PostprocessAliases(Expression expression)
    {
        // To post-process (finalize) table aliases in the tree, we visit it to see which aliases are actually in use.
        // We then remap those alias, e.g. closing any gaps caused by tables getting pruned, etc.
        // Finally, we revisit the tree in order to apply the remappings.
        var tableAliases = TableAliasCollector.Collect(expression);

        var aliasRewritingMap = RemapTableAliases(tableAliases);

        return aliasRewritingMap is null
            ? expression
            : TableAliasRewriter.Rewrite(expression, aliasRewritingMap);
    }

    /// <summary>
    ///     Given the list of table aliases currently in use in the SQL tree, produces a remapping for aliases within that list.
    ///     Can be used to e.g. close gaps for tables which have been pruned, etc.
    /// </summary>
    public virtual Dictionary<string, string>? RemapTableAliases(IReadOnlySet<string> usedAliases)
    {
        // Aliases consist of a single character, followed by a counter for uniquification.
        // We construct process the collected aliases above into a bitmap that represents, for each alias char, which numbers have been
        // seen. Note that since a0 is the 2nd uniquified alias (a is the first), the bits are off-by-one, with position 0 representing
        // a, position 1 representing a0, and so on.
        Dictionary<char, BitArray> aliasBitmaps = new();

        foreach (var alias in usedAliases)
        {
            var aliasBase = alias[0];
            var aliasNum = alias.Length == 1 ? 0 : int.Parse(alias[1..]) + 1;

            if (aliasBitmaps.TryGetValue(aliasBase, out var bitmap))
            {
                if (bitmap.Length < aliasNum + 1)
                {
                    bitmap.Length = aliasNum + 1;
                }
            }
            else
            {
                bitmap = aliasBitmaps[aliasBase] = new(aliasNum + 1);
            }

            bitmap[aliasNum] = true;
        }

        Dictionary<string, string>? aliasRewritingMap = null;
        foreach (var (aliasBase, bitmap) in aliasBitmaps)
        {
            if (bitmap.HasAllSet())
            {
                // There are no gaps, no need to do any rewriting of the aliases for this alias base
                continue;
            }

            var numHoles = 0;
            for (var i = 0; i < bitmap.Length; i++)
            {
                if (!bitmap[i])
                {
                    numHoles++;
                }
                else if (numHoles > 0)
                {
                    var oldAlias = aliasBase + (i == 0 ? "" : (i - 1).ToString());
                    var j = i - numHoles;
                    var newAlias = aliasBase + (j == 0 ? "" : (j - 1).ToString());

                    aliasRewritingMap ??= new();
                    aliasRewritingMap[oldAlias] = newAlias;
                }
            }
        }

        return aliasRewritingMap;
    }

    private sealed class TableAliasCollector : ExpressionVisitor
    {
        private readonly HashSet<string> _tableAliases = new();

        internal static HashSet<string> Collect(Expression expression)
        {
            var collector = new TableAliasCollector();
            collector.Visit(expression);
            return collector._tableAliases;
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case ShapedQueryExpression shapedQuery:
                    return shapedQuery.UpdateQueryExpression(Visit(shapedQuery.QueryExpression));

                case ColumnExpression { TableAlias: var alias }:
                    _tableAliases.Add(alias);
                    return base.VisitExtension(node);

                case TableExpressionBase { Alias: string alias }:
                    _tableAliases.Add(alias);
                    return base.VisitExtension(node);

                default:
                    return base.VisitExtension(node);
            }
        }
    }

    private sealed class TableAliasRewriter(IReadOnlyDictionary<string, string> aliasRewritingMap) : ExpressionVisitor
    {
        internal static Expression Rewrite(Expression expression, IReadOnlyDictionary<string, string> aliasRewritingMap)
            => new TableAliasRewriter(aliasRewritingMap).Visit(expression);

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case ShapedQueryExpression shapedQuery:
                    return shapedQuery.UpdateQueryExpression(Visit(shapedQuery.QueryExpression));

                // Note that this skips joins (which wrap the table that has the actual alias), as well as the top-level select
                case TableExpressionBase { Alias: string alias } table:
                    if (aliasRewritingMap.TryGetValue(alias, out var newAlias))
                    {
                        table = table.WithAlias(newAlias);
                    }

                    return base.VisitExtension(table);

                case ColumnExpression column when aliasRewritingMap.TryGetValue(column.TableAlias, out var newTableAlias):
                    return new ColumnExpression(column.Name, newTableAlias, column.Type, column.TypeMapping, column.IsNullable);

                default:
                    return base.VisitExtension(node);
            }
        }
    }

    private sealed class MutableInt
    {
        internal int Value;
    }
}
