// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
/// test
/// </summary>
public class WindowPartitionExpression : Expression, IPrintableExpression
{
    /// <summary>
    /// todo
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Partitions { get;  init; }

    /// <summary>
    /// tests
    /// </summary>
    /// <param name="partitions">test</param>
    public WindowPartitionExpression(IReadOnlyList<SqlExpression> partitions)
    {
        Partitions = partitions;
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="expressionPrinter">todo</param>
    public void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("PARTITION BY ");
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var newParts = new List<SqlExpression>();

        bool changed = false;

        foreach(var partition in Partitions)
        {
            var newPart = (SqlExpression)visitor.Visit(partition);

            newParts.Add(newPart);

            changed |= partition != newPart;
        }

        return changed
            ? new WindowPartitionExpression(newParts)
            : this;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is WindowPartitionExpression windowPartitionExpression
                && Equals(windowPartitionExpression));


    private bool Equals(WindowPartitionExpression windowPartitionExpression)
        => base.Equals(windowPartitionExpression)
                && ((Partitions == null && windowPartitionExpression.Partitions == null)
                    || (Partitions != null && windowPartitionExpression.Partitions != null
                    && Partitions.SequenceEqual(windowPartitionExpression.Partitions)));


    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());

        if (Partitions != null)
        {
            for (var i = 0; i < Partitions.Count; i++)
            {
                hash.Add(Partitions[i]);
            }
        }

        return hash.ToHashCode();
    }
}

