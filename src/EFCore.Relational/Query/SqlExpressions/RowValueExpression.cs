// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a SQL row.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RowValueExpression : SqlExpression
{
    /// <summary>
    /// The values of this row.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Values { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="RowValueExpression" /> class.
    /// </summary>
    /// <param name="values">The values of this row.</param>
    public RowValueExpression(IReadOnlyList<SqlExpression> values)
        : base(typeof(ValueTuple<object>), RowValueTypeMapping.Instance)
    {
        Check.NotEmpty(values, nameof(values));

        Values = values;
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        SqlExpression[]? newValues = null;

        for (var i = 0; i < Values.Count; i++)
        {
            var value = Values[i];
            var visited = (SqlExpression)visitor.Visit(value);
            if (visited != value && newValues is null)
            {
                newValues = new SqlExpression[Values.Count];
                for (var j = 0; j < i; j++)
                {
                    newValues[j] = Values[j];
                }
            }

            if (newValues is not null)
            {
                newValues[i] = visited;
            }
        }

        return newValues is null ? this : new RowValueExpression(newValues);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual RowValueExpression Update(IReadOnlyList<SqlExpression> values)
        => values.Count == Values.Count && values.Zip(Values, (x, y) => (x, y)).All(tup => tup.x == tup.y)
            ? this
            : new RowValueExpression(values);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("(");

        var count = Values.Count;
        for (var i = 0; i < count; i++)
        {
            expressionPrinter.Visit(Values[i]);

            if (i < count - 1)
            {
                expressionPrinter.Append(", ");
            }
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is RowValueExpression other && Equals(other);

    private bool Equals(RowValueExpression? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null || !base.Equals(other) || other.Values.Count != Values.Count)
        {
            return false;
        }

        for (var i = 0; i < Values.Count; i++)
        {
            if (!other.Values[i].Equals(Values[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (var value in Values)
        {
            hashCode.Add(value);
        }

        return hashCode.ToHashCode();
    }

    private sealed class RowValueTypeMapping : RelationalTypeMapping
    {
        public static RowValueTypeMapping Instance = new(typeof(ValueTuple<object>));

        private RowValueTypeMapping(Type clrType)
            : base("", clrType)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => this;
    }
}
