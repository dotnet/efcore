// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a UNION operation in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class UnionExpression : SetOperationBase
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="UnionExpression" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="source1">A table source which is first source in the set operation.</param>
    /// <param name="source2">A table source which is second source in the set operation.</param>
    /// <param name="distinct">A bool value indicating whether result will remove duplicate rows.</param>
    public UnionExpression(
        string alias,
        SelectExpression source1,
        SelectExpression source2,
        bool distinct)
        : this(alias, source1, source2, distinct, annotations: null)
    {
    }

    private UnionExpression(
        string alias,
        SelectExpression source1,
        SelectExpression source2,
        bool distinct,
        IReadOnlyDictionary<string, IAnnotation>? annotations)
        : base(alias, source1, source2, distinct, annotations)
    {
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var source1 = (SelectExpression)visitor.Visit(Source1);
        var source2 = (SelectExpression)visitor.Visit(Source2);

        return Update(source1, source2);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="source1">The <see cref="SetOperationBase.Source1" /> property of the result.</param>
    /// <param name="source2">The <see cref="SetOperationBase.Source2" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public override UnionExpression Update(SelectExpression source1, SelectExpression source2)
        => source1 != Source1 || source2 != Source2
            ? new UnionExpression(Alias, source1, source2, IsDistinct, Annotations)
            : this;

    /// <inheritdoc />
    protected override UnionExpression WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => new(Alias, Source1, Source2, IsDistinct, annotations);

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
        => new UnionExpression(
            alias!,
            (SelectExpression)cloningExpressionVisitor.Visit(Source1),
            (SelectExpression)cloningExpressionVisitor.Visit(Source2),
            IsDistinct);

    /// <inheritdoc />
    public override UnionExpression WithAlias(string newAlias)
        => new(newAlias, Source1, Source2, IsDistinct);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(UnionExpression).GetConstructor(
                [typeof(string), typeof(SelectExpression), typeof(SelectExpression), typeof(bool), typeof(IReadOnlyDictionary<string, IAnnotation>)])!,
            Constant(Alias, typeof(string)),
            Source1.Quote(),
            Source2.Quote(),
            Constant(IsDistinct),
            RelationalExpressionQuotingUtilities.QuoteAnnotations(Annotations));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("(");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Visit(Source1);
            expressionPrinter.AppendLine();
            expressionPrinter.Append("UNION");
            if (!IsDistinct)
            {
                expressionPrinter.AppendLine(" ALL");
            }

            expressionPrinter.Visit(Source2);
        }

        expressionPrinter.AppendLine();
        expressionPrinter.Append(")");
        PrintAnnotations(expressionPrinter);
        expressionPrinter.AppendLine($" AS {Alias}");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is UnionExpression unionExpression
                && Equals(unionExpression));

    private bool Equals(UnionExpression unionExpression)
        => base.Equals(unionExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), GetType());
}
