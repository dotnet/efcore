// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A node containing an expression expressing how to obtain a constant value, which may get lifted out of an expression tree.
/// </summary>
/// <remarks>
///     <para>
///         When the expression tree is compiled, the constant value can simply be evaluated beforehand, and a
///         <see cref="ConstantExpression" /> expression can directly reference the result.
///     </para>
///     <para>
///         When the expression tree is translated to source code instead (in query pre-compilation), the expression can be rendered out
///         separately, to be assigned to a variable, and this node is replaced by a reference to that variable.
///     </para>
/// </remarks>
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
[Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
public class LiftableConstantExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public LiftableConstantExpression(
        ConstantExpression originalExpression,
        LambdaExpression resolverExpression,
        string variableName,
        Type type)
    {
        OriginalExpression = originalExpression;
        ResolverExpression = resolverExpression;
        VariableName = char.ToLower(variableName[0]) + variableName[1..];
        Type = type;
    }

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConstantExpression OriginalExpression { get; }

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual LambdaExpression ResolverExpression { get; }

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string VariableName { get; }

    /// <inheritdoc />
    public override Type Type { get; }

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    // TODO: Complete other expression stuff (equality, etc.)

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var resolverExpression = (LambdaExpression)visitor.Visit(ResolverExpression);

        return Update(resolverExpression);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="resolverExpression">The <see cref="ResolverExpression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual LiftableConstantExpression Update(LambdaExpression resolverExpression)
        => resolverExpression != ResolverExpression
            ? new LiftableConstantExpression(OriginalExpression, resolverExpression, VariableName, Type)
            : this;

    /// <inheritdoc />
    public void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("[LIFTABLE Constant: ");
        expressionPrinter.Visit(OriginalExpression);
        expressionPrinter.Append(" | Resolver: ");
        expressionPrinter.Visit(ResolverExpression);
        expressionPrinter.Append("]");
    }
}
