// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
/// test
/// </summary>
public class WindowOverExpression : SqlExpression, IPrintableExpression
{
    /// <summary>
    /// todo
    /// </summary>
    public WindowPartitionExpression? Partition { get; init; }

    /// <summary>
    /// todo
    /// </summary>
    public SqlFunctionExpression Aggregate { get; set; }

    /// <summary>
    /// todo
    /// </summary>
    public IReadOnlyList<OrderingExpression> Ordering { get; init; }

    /// <summary>
    /// todo
    /// </summary>
    public WindowFrameExpression? WindowFrame { get; init; }

    /// <summary>
    /// todo
    /// </summary>
    public SqlExpression? Filter { get; init; }


    /// <summary>
    /// todo
    /// </summary>
    /// <param name="aggregateExpression">todo</param>
    /// <param name="partitionExpression">todo</param>
    /// <param name="orderingExpressions">todo</param>
    /// <param name="windowframeExpression">todo</param>
    public WindowOverExpression(SqlFunctionExpression aggregateExpression, WindowPartitionExpression? partitionExpression,
        IReadOnlyList<OrderingExpression> orderingExpressions, WindowFrameExpression? windowframeExpression)
        : base(aggregateExpression.Type, aggregateExpression.TypeMapping)
    {
        Partition = partitionExpression;
        Aggregate = aggregateExpression;
        Ordering = orderingExpressions;
        WindowFrame = windowframeExpression;
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="aggregateExpression">todo</param>
    /// <param name="partitionExpression">todo</param>
    /// <param name="orderingExpressions">todo</param>
    /// <param name="windowframeExpression">todo</param>
    /// <param name="filterExpression">todo</param>
    /// <param name="type">todo</param>
    /// <param name="relationalTypeMapping">todo</param>
    public WindowOverExpression(SqlFunctionExpression aggregateExpression, WindowPartitionExpression? partitionExpression,
        IReadOnlyList<OrderingExpression> orderingExpressions, WindowFrameExpression? windowframeExpression, SqlExpression? filterExpression,
        Type type, RelationalTypeMapping? relationalTypeMapping)
        : base(type, relationalTypeMapping)
    {
        Partition = partitionExpression;
        Aggregate = aggregateExpression;
        Ordering = orderingExpressions;
        WindowFrame = windowframeExpression;
        Filter = filterExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var aggregate = (SqlFunctionExpression)visitor.Visit(Aggregate);
        var partition = Partition != null ? visitor.Visit(Partition) as WindowPartitionExpression : null;
        var orderBys = new List<OrderingExpression>();
        var frame = visitor.Visit(WindowFrame) as WindowFrameExpression;

        foreach (var orderingExpression in Ordering)
        {
            var newOrder = (OrderingExpression)visitor.Visit(orderingExpression);
            orderBys.Add(newOrder);
        }

        return Update(partition, aggregate, orderBys, frame);
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="partition">todo</param>
    /// <param name="aggregate">todo</param>
    /// <param name="ordering">todo</param>
    /// <param name="frame">todo</param>
    /// <returns>todo</returns>
    public virtual WindowOverExpression Update(
        WindowPartitionExpression? partition,
        SqlFunctionExpression aggregate,
        IReadOnlyList<OrderingExpression> ordering,
        WindowFrameExpression? frame)
        => partition != Partition || aggregate != Aggregate || frame != WindowFrame || !Enumerable.SequenceEqual(ordering, Ordering)
            ? new WindowOverExpression(aggregate, partition, ordering, frame)
            : this;


    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual WindowOverExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new(
            Aggregate.ApplyTypeMapping(typeMapping),
            Partition,
            Ordering,
            WindowFrame,
            Filter,
            Type,
            typeMapping ?? TypeMapping);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="expressionPrinter">todo</param>
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("OVER ");
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <returns>todo</returns>
    public override Expression Quote()
    {
        //what is this supposed to do?
        return this;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is WindowOverExpression windowOverExpression
                && Equals(windowOverExpression));

    private bool Equals(WindowOverExpression windowOverExpression)
        => base.Equals(windowOverExpression)
            && Aggregate.Equals(windowOverExpression.Aggregate)
            && ((Partition == null && windowOverExpression.Partition == null)
                || (Partition != null && Partition.Equals(windowOverExpression.Partition)))
            && ((WindowFrame == null && windowOverExpression.WindowFrame == null)
                || (WindowFrame != null && WindowFrame.Equals(windowOverExpression.WindowFrame)))
            & ((Filter == null && windowOverExpression.Filter == null)
                || (Filter != null && Filter.Equals(windowOverExpression.Filter)))
            && ((Ordering == null && windowOverExpression.Ordering == null)
                || (Ordering != null && windowOverExpression.Ordering != null
                        && Ordering.SequenceEqual(windowOverExpression.Ordering)));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Aggregate);
        hash.Add(WindowFrame);
        hash.Add(WindowFrame);

        if (Ordering != null)
        {
            for (var i = 0; i < Ordering.Count; i++)
            {
                hash.Add(Ordering[i]);
            }
        }

        return hash.ToHashCode();
    }
}
