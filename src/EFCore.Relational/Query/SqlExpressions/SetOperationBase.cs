// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a set operation between two table sources.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public abstract class SetOperationBase : TableExpressionBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="SetOperationBase" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="source1">A table source which is first source in the set operation.</param>
    /// <param name="source2">A table source which is second source in the set operation.</param>
    /// <param name="distinct">A bool value indicating whether result will remove duplicate rows.</param>
    protected SetOperationBase(
        string alias,
        SelectExpression source1,
        SelectExpression source2,
        bool distinct)
        : this(alias, source1, source2, distinct, annotations: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="SetOperationBase" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="source1">A table source which is first source in the set operation.</param>
    /// <param name="source2">A table source which is second source in the set operation.</param>
    /// <param name="distinct">A bool value indicating whether result will remove duplicate rows.</param>
    /// <param name="annotations">Collection of annotations associated with this expression.</param>
    protected SetOperationBase(
        string alias,
        SelectExpression source1,
        SelectExpression source2,
        bool distinct,
        IReadOnlyDictionary<string, IAnnotation>? annotations)
        : base(alias, annotations)
    {
        IsDistinct = distinct;
        Source1 = source1;
        Source2 = source2;
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    public override string Alias
        => base.Alias!;

    /// <summary>
    ///     The bool value indicating whether result will remove duplicate rows.
    /// </summary>
    public virtual bool IsDistinct { get; }

    /// <summary>
    ///     The first source of the set operation.
    /// </summary>
    public virtual SelectExpression Source1 { get; }

    /// <summary>
    ///     The second source of the set operation.
    /// </summary>
    public virtual SelectExpression Source2 { get; }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="source1">The <see cref="Source1" /> property of the result.</param>
    /// <param name="source2">The <see cref="Source2" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public abstract SetOperationBase Update(SelectExpression source1, SelectExpression source2);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SetOperationBase setOperationBase
                && Equals(setOperationBase));

    private bool Equals(SetOperationBase setOperationBase)
        => IsDistinct == setOperationBase.IsDistinct
            && Source1.Equals(setOperationBase.Source1)
            && Source2.Equals(setOperationBase.Source2);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), IsDistinct, Source1, Source2);
}
