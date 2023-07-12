// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents include operation in <see cref="ShapedQueryExpression.ShaperExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public class IncludeExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="IncludeExpression" /> class. The navigation will be set
    ///     as loaded after completing the Include.
    /// </summary>
    /// <param name="entityExpression">An expression to get entity which is performing include.</param>
    /// <param name="navigationExpression">An expression to get included navigation element.</param>
    /// <param name="navigation">The navigation for this include operation.</param>
    public IncludeExpression(
        Expression entityExpression,
        Expression navigationExpression,
        INavigationBase navigation)
        : this(entityExpression, navigationExpression, navigation, setLoaded: true)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public IncludeExpression(
        Expression entityExpression,
        Expression navigationExpression,
        INavigationBase navigation,
        bool setLoaded)
    {
        EntityExpression = entityExpression;
        NavigationExpression = navigationExpression;
        Navigation = navigation;
        Type = EntityExpression.Type;
        SetLoaded = setLoaded;
    }

    /// <summary>
    ///     The expression representing entity performing this include.
    /// </summary>
    public virtual Expression EntityExpression { get; }

    /// <summary>
    ///     The expression representing included navigation element.
    /// </summary>
    public virtual Expression NavigationExpression { get; }

    /// <summary>
    ///     The navigation associated with this include operation.
    /// </summary>
    public virtual INavigationBase Navigation { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual bool SetLoaded { get; }

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var newEntityExpression = visitor.Visit(EntityExpression);
        var newNavigationExpression = visitor.Visit(NavigationExpression);

        return Update(newEntityExpression, newNavigationExpression);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="entityExpression">The <see cref="EntityExpression" /> property of the result.</param>
    /// <param name="navigationExpression">The <see cref="NavigationExpression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual IncludeExpression Update(Expression entityExpression, Expression navigationExpression)
        => entityExpression != EntityExpression || navigationExpression != NavigationExpression
            ? new IncludeExpression(entityExpression, navigationExpression, Navigation, SetLoaded)
            : this;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine("IncludeExpression(");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.AppendLine("EntityExpression:");
            expressionPrinter.Visit(EntityExpression);
            expressionPrinter.AppendLine(", ");
            expressionPrinter.AppendLine("NavigationExpression:");
            expressionPrinter.Visit(NavigationExpression);
            expressionPrinter.AppendLine($", {Navigation.Name})");
        }
    }
}
