// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a table value function as a table source in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class TableValuedFunctionExpression : TableExpressionBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="TableValuedFunctionExpression" /> class.
    /// </summary>
    /// <param name="storeFunction">The <see cref="IStoreFunction" /> associated this function.</param>
    /// <param name="arguments">The arguments of the function.</param>
    public TableValuedFunctionExpression(IStoreFunction storeFunction, IReadOnlyList<SqlExpression> arguments)
        : this(
            storeFunction.Name[..1].ToLowerInvariant(),
            storeFunction,
            arguments,
            annotations: null)
    {
    }

    private TableValuedFunctionExpression(
        string alias,
        IStoreFunction storeFunction,
        IReadOnlyList<SqlExpression> arguments,
        IEnumerable<IAnnotation>? annotations)
        : base(alias, annotations)
    {
        StoreFunction = storeFunction;
        Arguments = arguments;
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    [NotNull]
    public override string? Alias
    {
        get => base.Alias!;
        internal set => base.Alias = value;
    }

    /// <summary>
    ///     The store function.
    /// </summary>
    public virtual IStoreFunction StoreFunction { get; }

    /// <summary>
    ///     The list of arguments of this function.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Arguments { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;
        var arguments = new SqlExpression[Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = (SqlExpression)visitor.Visit(Arguments[i]);
            changed |= arguments[i] != Arguments[i];
        }

        return changed
            ? new TableValuedFunctionExpression(Alias, StoreFunction, arguments, GetAnnotations())
            : this;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="arguments">The <see cref="Arguments" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual TableValuedFunctionExpression Update(IReadOnlyList<SqlExpression> arguments)
        => !arguments.SequenceEqual(Arguments)
            ? new TableValuedFunctionExpression(Alias, StoreFunction, arguments, GetAnnotations())
            : this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        if (!string.IsNullOrEmpty(StoreFunction.Schema))
        {
            expressionPrinter.Append(StoreFunction.Schema).Append(".");
        }

        expressionPrinter.Append(StoreFunction.Name);
        expressionPrinter.Append("(");
        expressionPrinter.VisitCollection(Arguments);
        expressionPrinter.Append(")");
        PrintAnnotations(expressionPrinter);
        expressionPrinter.Append(" AS ");
        expressionPrinter.Append(Alias);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is TableValuedFunctionExpression tableValuedFunctionExpression
                && Equals(tableValuedFunctionExpression));

    private bool Equals(TableValuedFunctionExpression tableValuedFunctionExpression)
        => base.Equals(tableValuedFunctionExpression)
            && StoreFunction == tableValuedFunctionExpression.StoreFunction
            && Arguments.SequenceEqual(tableValuedFunctionExpression.Arguments);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(StoreFunction);
        for (var i = 0; i < Arguments.Count; i++)
        {
            hash.Add(Arguments[i]);
        }

        return hash.ToHashCode();
    }
}
