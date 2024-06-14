// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[DebuggerDisplay("{PrintShortSql(), nq}")]
public class SelectExpression : Expression, IPrintableExpression
{
    private const string RootAlias = "c";

    private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();
    private readonly List<SourceExpression> _sources = [];
    private readonly List<ProjectionExpression> _projection = [];
    private readonly List<OrderingExpression> _orderings = [];

    private readonly List<(Expression ValueExpression, IProperty Property)> _partitionKeyValues = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(IEntityType entityType, ReadItemInfo readItemInfo)
        : this(entityType)
    {
        ReadItemInfo = readItemInfo;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(IEntityType entityType)
    {
        // TODO: Redo aliasing
        _sources = [new SourceExpression(new ObjectReferenceExpression(entityType, "root"), RootAlias)];
        _projectionMapping[new ProjectionMember()]
            = new EntityProjectionExpression(entityType, new ObjectReferenceExpression(entityType, RootAlias));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(IEntityType entityType, string sql, Expression argument)
    {
        var fromSql = new FromSqlExpression(entityType.ClrType, sql, argument);
        _sources = [new SourceExpression(fromSql, RootAlias)];
        _projectionMapping[new ProjectionMember()] = new EntityProjectionExpression(
            entityType, new ObjectReferenceExpression(entityType, RootAlias));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(
        List<ProjectionExpression> projections,
        List<SourceExpression> sources,
        List<OrderingExpression> orderings)
    {
        _projection = projections;
        _sources = sources;
        _orderings = orderings;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ReadItemInfo? ReadItemInfo { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(SqlExpression projection)
        => _projectionMapping[new ProjectionMember()] = projection;

    private SelectExpression()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SelectExpression CreateForPrimitiveCollection(
        SourceExpression source,
        Type elementClrType,
        CoreTypeMapping elementTypeMapping)
        => new()
        {
            _sources = { source },
            _projectionMapping =
            {
                [new ProjectionMember()] = new ScalarReferenceExpression(source.Alias, elementClrType, elementTypeMapping)
            },
            UsesSingleValueProjection = true
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<ProjectionExpression> Projection
        => _projection;

    /// <summary>
    ///     If set, indicates that the <see cref="SelectExpression" /> has a Cosmos VALUE projection, which does not get wrapped in a
    ///     JSON object. If <see langword="true" />, <see cref="Projection" /> must contain a single item.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual bool UsesSingleValueProjection { get; init; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<SourceExpression> Sources
        => _sources;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<OrderingExpression> Orderings
        => _orderings;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Predicate { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Limit { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Offset { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsDistinct { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression GetMappedProjection(ProjectionMember projectionMember)
        => _projectionMapping[projectionMember];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ClearProjection()
        => _projectionMapping.Clear();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddPartitionKey(IProperty partitionKeyProperty, Expression expression)
        => _partitionKeyValues.Add((expression, partitionKeyProperty));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PartitionKey GetPartitionKeyValue(IReadOnlyDictionary<string, object> parameterValues)
    {
        if (!_partitionKeyValues.Any())
        {
            return PartitionKey.None;
        }

        var builder = new PartitionKeyBuilder();
        foreach (var tuple in _partitionKeyValues)
        {
            var rawKeyValue = tuple.ValueExpression switch
            {
                ConstantExpression constantExpression
                    => constantExpression.Value,
                ParameterExpression parameterExpression when parameterValues.TryGetValue(parameterExpression.Name!, out var value)
                    => value,
                _ => null
            };
            builder.Add(rawKeyValue, tuple.Property);
        }

        return builder.Build();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyProjection()
    {
        if (Projection.Any())
        {
            return;
        }

        var result = new Dictionary<ProjectionMember, Expression>();
        foreach (var (projectionMember, expression) in _projectionMapping)
        {
            result[projectionMember] = Constant(
                AddToProjection(
                    expression,
                    projectionMember.Last?.Name));
        }

        _projectionMapping = result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ReplaceProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMapping)
    {
        _projectionMapping.Clear();
        foreach (var (projectionMember, expression) in projectionMapping)
        {
            _projectionMapping[projectionMember] = expression;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int AddToProjection(SqlExpression sqlExpression)
        => AddToProjection(sqlExpression, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int AddToProjection(EntityProjectionExpression entityProjection)
        => AddToProjection(entityProjection, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int AddToProjection(ObjectArrayProjectionExpression objectArrayProjection)
        => AddToProjection(objectArrayProjection, null);

    private int AddToProjection(Expression expression, string? alias)
    {
        var existingIndex = _projection.FindIndex(pe => pe.Expression.Equals(expression));
        if (existingIndex != -1)
        {
            return existingIndex;
        }

        var baseAlias = alias
            ?? (expression as IAccessExpression)?.Name
            ?? "c";

        var currentAlias = baseAlias;
        var counter = 0;
        while (_projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
        {
            currentAlias = $"{baseAlias}{counter++}";
        }

        _projection.Add(new ProjectionExpression(expression, currentAlias));

        return _projection.Count - 1;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyDistinct()
        => IsDistinct = true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ClearOrdering()
        => _orderings.Clear();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyPredicate(SqlExpression expression)
    {
        if (expression is SqlConstantExpression { Value: true })
        {
            return;
        }

        Predicate = Predicate == null
            ? expression
            : new SqlBinaryExpression(
                ExpressionType.AndAlso,
                Predicate,
                expression,
                typeof(bool),
                expression.TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyLimit(SqlExpression sqlExpression)
    {
        if (Limit != null)
        {
            throw new InvalidOperationException(
                CoreStrings.TranslationFailedWithDetails(
                    sqlExpression.Print(),
                    CosmosStrings.NoSubqueryPushdown));
        }

        Limit = sqlExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyOffset(SqlExpression sqlExpression)
    {
        if (Limit != null
            || Offset != null)
        {
            throw new InvalidOperationException(
                CoreStrings.TranslationFailedWithDetails(
                    sqlExpression.Print(),
                    CosmosStrings.NoSubqueryPushdown));
        }

        Offset = sqlExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyOrdering(OrderingExpression orderingExpression)
    {
        if (IsDistinct
            || Limit != null
            || Offset != null)
        {
            throw new InvalidOperationException(
                CoreStrings.TranslationFailedWithDetails(
                    orderingExpression.Print(),
                    CosmosStrings.NoSubqueryPushdown));
        }

        _orderings.Clear();
        _orderings.Add(orderingExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AppendOrdering(OrderingExpression orderingExpression)
    {
        if (_orderings.FirstOrDefault(o => o.Expression.Equals(orderingExpression.Expression)) == null)
        {
            _orderings.Add(orderingExpression);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ReverseOrderings()
    {
        if (Limit != null
            || Offset != null)
        {
            throw new InvalidOperationException(CosmosStrings.ReverseAfterSkipTakeNotSupported);
        }

        var existingOrderings = _orderings.ToArray();

        _orderings.Clear();

        foreach (var existingOrdering in existingOrderings)
        {
            _orderings.Add(
                new OrderingExpression(
                    existingOrdering.Expression,
                    !existingOrdering.IsAscending));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Type Type
        => typeof(object);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;

        var projections = new List<ProjectionExpression>();
        IDictionary<ProjectionMember, Expression> projectionMapping;
        if (Projection.Any())
        {
            projectionMapping = _projectionMapping;
            foreach (var item in Projection)
            {
                var projection = (ProjectionExpression)visitor.Visit(item);
                projections.Add(projection);

                changed |= projection != item;
            }
        }
        else
        {
            projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var (projectionMember, expression) in _projectionMapping)
            {
                var newProjection = visitor.Visit(expression);
                changed |= newProjection != expression;

                projectionMapping[projectionMember] = newProjection;
            }
        }

        var sources = new List<SourceExpression>();
        foreach (var source in _sources)
        {
            var visitedSource = (SourceExpression)visitor.Visit(source);
            changed |= visitedSource != source;
            sources.Add(visitedSource);
        }

        var predicate = (SqlExpression?)visitor.Visit(Predicate);
        changed |= predicate != Predicate;

        var orderings = new List<OrderingExpression>();
        foreach (var ordering in _orderings)
        {
            var orderingExpression = (SqlExpression)visitor.Visit(ordering.Expression);
            changed |= orderingExpression != ordering.Expression;
            orderings.Add(ordering.Update(orderingExpression));
        }

        var offset = (SqlExpression?)visitor.Visit(Offset);
        changed |= offset != Offset;

        var limit = (SqlExpression?)visitor.Visit(Limit);
        changed |= limit != Limit;

        if (changed)
        {
            var newSelectExpression = new SelectExpression(projections, sources, orderings)
            {
                _projectionMapping = projectionMapping,
                Predicate = predicate,
                Offset = offset,
                Limit = limit,
                IsDistinct = IsDistinct
            };

            return newSelectExpression;
        }

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SelectExpression Update(
        List<ProjectionExpression> projections,
        List<SourceExpression> sources,
        SqlExpression? predicate,
        List<OrderingExpression> orderings,
        SqlExpression? limit,
        SqlExpression? offset)
    {
        var projectionMapping = new Dictionary<ProjectionMember, Expression>();
        foreach (var (projectionMember, expression) in _projectionMapping)
        {
            projectionMapping[projectionMember] = expression;
        }

        return new SelectExpression(projections, sources, orderings)
        {
            _projectionMapping = projectionMapping,
            Predicate = predicate,
            Offset = offset,
            Limit = limit,
            IsDistinct = IsDistinct
        };
    }

    #region Print

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Print(ExpressionPrinter expressionPrinter)
    {
        PrintProjections(expressionPrinter);
        expressionPrinter.AppendLine();
        PrintSql(expressionPrinter);
    }

    private void PrintProjections(ExpressionPrinter expressionPrinter)
    {
        if (_projectionMapping.Count > 0)
        {
            expressionPrinter.AppendLine("Projection Mapping:");
            using (expressionPrinter.Indent())
            {
                foreach (var (projectionMember, expression) in _projectionMapping)
                {
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append(projectionMember.ToString()).Append(" -> ");
                    expressionPrinter.Visit(expression);
                }
            }
        }
    }

    private void PrintSql(ExpressionPrinter expressionPrinter, bool withTags = true)
    {
        if (withTags)
        {
            // foreach (var tag in Tags)
            // {
            //     expressionPrinter.Append($"-- {tag}");
            // }
        }

        expressionPrinter.Append("SELECT ");

        if (IsDistinct)
        {
            expressionPrinter.Append("DISTINCT ");
        }

        if (Projection.Any())
        {
            if (UsesSingleValueProjection)
            {
                expressionPrinter.Append("VALUE ");
            }

            expressionPrinter.VisitCollection(Projection);
        }
        else
        {
            expressionPrinter.Append("1");
        }

        if (Sources.Any())
        {
            expressionPrinter.AppendLine().Append("FROM ");

            expressionPrinter.VisitCollection(Sources, p => p.AppendLine());
        }

        if (Predicate != null)
        {
            expressionPrinter.AppendLine().Append("WHERE ");
            expressionPrinter.Visit(Predicate);
        }

        if (Orderings.Any())
        {
            expressionPrinter.AppendLine().Append("ORDER BY ");
            expressionPrinter.VisitCollection(Orderings);
        }

        if (Offset != null)
        {
            expressionPrinter.AppendLine().Append("OFFSET ");
            expressionPrinter.Visit(Offset);
            expressionPrinter.Append(" ROWS");

            if (Limit != null)
            {
                expressionPrinter.Append(" FETCH NEXT ");
                expressionPrinter.Visit(Limit);
                expressionPrinter.Append(" ROWS ONLY");
            }
        }
    }

    private string PrintShortSql()
    {
        var expressionPrinter = new ExpressionPrinter();
        PrintSql(expressionPrinter, withTags: false);
        return expressionPrinter.ToString();
    }

    /// <summary>
    ///     <para>
    ///         Expand this property in the debugger for a human-readable representation of this <see cref="SelectExpression" />.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the debug strings.
    ///         They are designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    [EntityFrameworkInternal]
    public virtual string DebugView
        => this.Print();

    #endregion Print
}
