// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     A stateful manager for SQL aliases, capable of generating uniquified source aliases and rewriting them in post-processing.
///     An instance of <see cref="CosmosAliasManager" /> is valid for a single query compilation, and is owned by
///     <see cref="CosmosQueryCompilationContext" />.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class CosmosAliasManager
{
    /// <summary>
    ///     Maps alias prefixes to the highest number postfix currently in use.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    private readonly Dictionary<char, MutableInt> _aliases = new();

    /// <summary>
    ///     Generates an alias based on the given <paramref name="expression" />.
    ///     All aliases produced by a given instance of <see cref="CosmosAliasManager" /> are unique.
    /// </summary>
    /// <param name="expression">
    ///     An expression to use as the starting point for the alias; this method knows a number of well-known expression types and can
    ///     generate appropriate aliases for them. A number postfix will be appended to it as necessary.
    /// </param>
    /// <param name="fallback">
    ///     If <paramref name="expression" /> isn't a well-known expression type, this fallback string will be used.
    /// </param>
    /// <returns>A fully unique alias within the context of this translation process.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual string GenerateSourceAlias(Expression expression, string? fallback = null)
        => GenerateSourceAlias(
            expression switch
            {
                IAccessExpression { PropertyName: string propertyName } => propertyName,
                FromSqlExpression => "sql",
                SqlFunctionExpression { Name: "ARRAY_SLICE", Arguments: [var array, ..] } => GenerateSourceAlias(array),
                ObjectFunctionExpression { Name: "ARRAY_SLICE", Arguments: [var array, ..] } => GenerateSourceAlias(array),
                SqlFunctionExpression { Name: var name } => name,
                ObjectFunctionExpression { Name: var name } => name,
                ArrayConstantExpression => "array",

                _ => fallback ?? "value"
            });

    /// <summary>
    ///     Generates an alias based on the given <paramref name="name" />.
    ///     All aliases produced by a given instance of <see cref="CosmosAliasManager" /> are unique.
    /// </summary>
    /// <param name="name">
    ///     A name (e.g. of a container) to use as the starting point for the alias; a number postfix will be appended to it as necessary.
    /// </param>
    /// <returns>A fully unique alias within the context of this translation process.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual string GenerateSourceAlias(string name)
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
    ///     Performs a post-processing pass over aliases in the provided SQL tree, closing any gaps.
    /// </summary>
    /// <param name="expression">The SQL tree to post-process.</param>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual Expression PostprocessAliases(Expression expression)
    {
        // To post-process (finalize) source aliases in the tree, we visit it to see which aliases are actually in use.
        // We then remap those alias, e.g. closing any gaps caused by tables getting pruned, etc.
        // Finally, we revisit the tree in order to apply the remapped aliases.

        var sourceAliases = SourceAliasCollector.Collect(expression);

        var aliasRewritingMap = RemapSourceAliases(sourceAliases);

        return aliasRewritingMap is null
            ? expression
            : SourceAliasRewriter.Rewrite(expression, aliasRewritingMap);
    }

    /// <summary>
    ///     Given the list of source aliases currently in use in the SQL tree, produces a remapping for aliases within that list.
    ///     Can be used to e.g. close gaps for sources which have been pruned, etc.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    protected virtual Dictionary<string, string>? RemapSourceAliases(IReadOnlySet<string> usedAliases)
    {
        // Aliases consist of a single character, followed by a counter for uniquification.
        // We process the collected aliases above into a bitmap that represents, for each alias char, which numbers have been seen.
        // Note that since a0 is the 2nd uniquified alias (a is the first), the bits are off-by-one, with position 0 representing
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
                bitmap = aliasBitmaps[aliasBase] = new BitArray(aliasNum + 1);
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

                    aliasRewritingMap ??= new Dictionary<string, string>();
                    aliasRewritingMap[oldAlias] = newAlias;
                }
            }
        }

        return aliasRewritingMap;
    }

    private sealed class SourceAliasCollector : ExpressionVisitor
    {
        private readonly HashSet<string> _sourceAliases = new();

        internal static HashSet<string> Collect(Expression expression)
        {
            var collector = new SourceAliasCollector();
            collector.Visit(expression);
            return collector._sourceAliases;
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case ShapedQueryExpression shapedQuery:
                    return shapedQuery.UpdateQueryExpression(Visit(shapedQuery.QueryExpression));

                case SourceExpression { Alias: string alias }:
                    _sourceAliases.Add(alias);
                    return base.VisitExtension(node);

                default:
                    return base.VisitExtension(node);
            }
        }
    }

    private sealed class SourceAliasRewriter(IReadOnlyDictionary<string, string> aliasRewritingMap) : ExpressionVisitor
    {
        internal static Expression Rewrite(Expression expression, IReadOnlyDictionary<string, string> aliasRewritingMap)
            => new SourceAliasRewriter(aliasRewritingMap).Visit(expression);

        protected override Expression VisitExtension(Expression node)
            => node switch
            {
                ShapedQueryExpression shapedQuery => shapedQuery.UpdateQueryExpression(Visit(shapedQuery.QueryExpression)),

                SourceExpression { Alias: string alias } source when aliasRewritingMap.TryGetValue(alias, out var newAlias)
                    => base.VisitExtension(new SourceExpression(source.Expression, newAlias, source.WithIn)),
                ScalarReferenceExpression reference when aliasRewritingMap.TryGetValue(reference.Name, out var newAlias)
                    => new ScalarReferenceExpression(newAlias, reference.Type, reference.TypeMapping),
                ObjectReferenceExpression reference when aliasRewritingMap.TryGetValue(reference.Name, out var newAlias)
                    => new ObjectReferenceExpression(reference.EntityType, newAlias),

                _ => base.VisitExtension(node)
            };
    }

    private sealed class MutableInt
    {
        internal int Value;
    }
}
