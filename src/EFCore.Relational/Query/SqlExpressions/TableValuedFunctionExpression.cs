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
public class TableValuedFunctionExpression : TableExpressionBase, ITableBasedExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="TableValuedFunctionExpression" /> class.
    /// </summary>
    /// <param name="storeFunction">The <see cref="IStoreFunction" /> associated this function.</param>
    /// <param name="arguments">The arguments of the function.</param>
    public TableValuedFunctionExpression(IStoreFunction storeFunction, IReadOnlyList<SqlExpression> arguments)
        : this(
            storeFunction.Name[..1].ToLowerInvariant(),
            storeFunction.Name,
            storeFunction.Schema,
            storeFunction.IsBuiltIn,
            arguments,
            annotations: null)
    {
        StoreFunction = storeFunction;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="TableValuedFunctionExpression" /> class.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="arguments">The arguments of the function.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    public TableValuedFunctionExpression(
        string name,
        IReadOnlyList<SqlExpression> arguments,
        IEnumerable<IAnnotation>? annotations = null)
        : this(name[..1].ToLowerInvariant(), name, schema: null, builtIn: true, arguments, annotations)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="TableValuedFunctionExpression" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="arguments">The arguments of the function.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    public TableValuedFunctionExpression(
        string alias,
        string name,
        IReadOnlyList<SqlExpression> arguments,
        IEnumerable<IAnnotation>? annotations = null)
        : this(alias, name, schema: null, builtIn: true, arguments, annotations)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="TableValuedFunctionExpression" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="schema">The schema of the function.</param>
    /// <param name="builtIn">Whether the function is built-in.</param>
    /// <param name="arguments">The arguments of the function.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    protected TableValuedFunctionExpression(
        string alias,
        string name,
        string? schema,
        bool builtIn,
        IReadOnlyList<SqlExpression> arguments,
        IEnumerable<IAnnotation>? annotations = null)
        : base(alias, annotations)
    {
        Name = name;
        Schema = schema;
        IsBuiltIn = builtIn;
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
    public virtual IStoreFunction? StoreFunction { get; }

    /// <inheritdoc />
    ITableBase? ITableBasedExpression.Table
        => StoreFunction;

    /// <summary>
    ///     The name of the function.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     The schema of the function.
    /// </summary>
    public virtual string? Schema { get; }

    /// <summary>
    ///     Gets the value indicating whether the function is built-in.
    /// </summary>
    public virtual bool IsBuiltIn { get; }

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
            ? new TableValuedFunctionExpression(Alias, Name, Schema, IsBuiltIn, arguments, GetAnnotations())
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
            ? new TableValuedFunctionExpression(Alias, Name, Schema, IsBuiltIn, arguments, GetAnnotations())
            : this;

    /// <inheritdoc />
    protected override TableExpressionBase CreateWithAnnotations(IEnumerable<IAnnotation> annotations)
        => new TableValuedFunctionExpression(Alias, Name, Schema, IsBuiltIn, Arguments, annotations);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        if (!string.IsNullOrEmpty(Schema))
        {
            expressionPrinter.Append(Schema).Append(".");
        }

        expressionPrinter.Append(Name);
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
            && Name == tableValuedFunctionExpression.Name
            && Schema == tableValuedFunctionExpression.Schema
            && IsBuiltIn == tableValuedFunctionExpression.IsBuiltIn
            && StoreFunction == tableValuedFunctionExpression.StoreFunction
            && Arguments.SequenceEqual(tableValuedFunctionExpression.Arguments);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Name);
        hash.Add(Schema);
        hash.Add(IsBuiltIn);
        hash.Add(StoreFunction);
        for (var i = 0; i < Arguments.Count; i++)
        {
            hash.Add(Arguments[i]);
        }

        return hash.ToHashCode();
    }
}
