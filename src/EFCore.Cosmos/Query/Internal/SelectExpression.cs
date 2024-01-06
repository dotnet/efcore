// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable warnings

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SelectExpression : Expression
{
    private const string RootAlias = "c";

    private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();
    private readonly List<ProjectionExpression> _projection = [];
    private readonly List<OrderingExpression> _orderings = [];

    private ValueConverter _partitionKeyValueConverter;
    private Expression _partitionKeyValue;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(IEntityType entityType)
    {
        Container = entityType.GetContainer();
        FromExpression = new RootReferenceExpression(entityType, RootAlias);
        _projectionMapping[new ProjectionMember()] = new EntityProjectionExpression(entityType, FromExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(IEntityType entityType, string sql, Expression argument)
    {
        Container = entityType.GetContainer();
        FromExpression = new FromSqlExpression(entityType, RootAlias, sql, argument);
        _projectionMapping[new ProjectionMember()] = new EntityProjectionExpression(
            entityType, new RootReferenceExpression(entityType, RootAlias));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(
        List<ProjectionExpression> projections,
        RootReferenceExpression fromExpression,
        List<OrderingExpression> orderings)
    {
        _projection = projections;
        FromExpression = fromExpression;
        _orderings = orderings;
    }

    private SelectExpression(
        List<ProjectionExpression> projections,
        RootReferenceExpression fromExpression,
        List<OrderingExpression> orderings,
        string container)
        : this(projections, fromExpression, orderings)
    {
        Container = container;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Container { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<ProjectionExpression> Projection
        => _projection;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RootReferenceExpression FromExpression { get; }

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
    public virtual SqlExpression Predicate { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Limit { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Offset { get; private set; }

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
    public virtual void SetPartitionKey(IProperty partitionKeyProperty, Expression expression)
    {
        _partitionKeyValueConverter = partitionKeyProperty.GetTypeMapping().Converter;
        _partitionKeyValue = expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GetPartitionKey(IReadOnlyDictionary<string, object> parameterValues)
    {
        return _partitionKeyValue switch
        {
            ConstantExpression constantExpression
                => GetString(_partitionKeyValueConverter, constantExpression.Value),
            ParameterExpression parameterExpression when parameterValues.TryGetValue(parameterExpression.Name, out var value)
                => GetString(_partitionKeyValueConverter, value),
            _ => null
        };

        static string GetString(ValueConverter converter, object value)
            => converter is null
                ? (string)value
                : (string)converter.ConvertToProvider(value);
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

    private int AddToProjection(Expression expression, string alias)
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

        var fromExpression = (RootReferenceExpression)visitor.Visit(FromExpression);
        changed |= fromExpression != FromExpression;

        var predicate = (SqlExpression)visitor.Visit(Predicate);
        changed |= predicate != Predicate;

        var orderings = new List<OrderingExpression>();
        foreach (var ordering in _orderings)
        {
            var orderingExpression = (SqlExpression)visitor.Visit(ordering.Expression);
            changed |= orderingExpression != ordering.Expression;
            orderings.Add(ordering.Update(orderingExpression));
        }

        var offset = (SqlExpression)visitor.Visit(Offset);
        changed |= offset != Offset;

        var limit = (SqlExpression)visitor.Visit(Limit);
        changed |= limit != Limit;

        if (changed)
        {
            var newSelectExpression = new SelectExpression(projections, fromExpression, orderings)
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
        RootReferenceExpression fromExpression,
        SqlExpression? predicate,
        List<OrderingExpression>? orderings,
        SqlExpression? limit,
        SqlExpression? offset)
    {
        var projectionMapping = new Dictionary<ProjectionMember, Expression>();
        foreach (var (projectionMember, expression) in _projectionMapping)
        {
            projectionMapping[projectionMember] = expression;
        }

        return new SelectExpression(projections, fromExpression, orderings, Container)
        {
            _projectionMapping = projectionMapping,
            Predicate = predicate,
            Offset = offset,
            Limit = limit,
            IsDistinct = IsDistinct
        };
    }
}
