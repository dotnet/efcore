// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression visitor that replaces one expression with another in given expression tree.
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
public class ReplacingExpressionVisitor : ExpressionVisitor
{
    private readonly IReadOnlyList<Expression> _originals;
    private readonly IReadOnlyList<Expression> _replacements;

    /// <summary>
    ///     Replaces one expression with another in given expression tree.
    /// </summary>
    /// <param name="original">The expression to replace.</param>
    /// <param name="replacement">The expression to be used as replacement.</param>
    /// <param name="tree">The expression tree in which replacement is going to be performed.</param>
    /// <returns>An expression tree with replacements made.</returns>
    public static Expression Replace(Expression original, Expression replacement, Expression tree)
        => new ReplacingExpressionVisitor(new[] { original }, new[] { replacement }).Visit(tree);

    /// <summary>
    ///     Creates a new instance of the <see cref="ReplacingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="originals">A list of original expressions to replace.</param>
    /// <param name="replacements">A list of expressions to be used as replacements.</param>
    public ReplacingExpressionVisitor(IReadOnlyList<Expression> originals, IReadOnlyList<Expression> replacements)
    {
        _originals = originals;
        _replacements = replacements;
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull("expression")]
    public override Expression? Visit(Expression? expression)
    {
        if (expression is null or ShapedQueryExpression or StructuralTypeShaperExpression or GroupByShaperExpression)
        {
            return expression;
        }

        // We use two arrays rather than a dictionary because hash calculation here can be prohibitively expensive
        // for deep trees. Locality of reference makes arrays better for the small number of replacements anyway.
        for (var i = 0; i < _originals.Count; i++)
        {
            if (expression.Equals(_originals[i]))
            {
                return _replacements[i];
            }
        }

        return base.Visit(expression);
    }

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var innerExpression = Visit(memberExpression.Expression);

        if (innerExpression is GroupByShaperExpression groupByShaperExpression
            && memberExpression.Member.Name == nameof(IGrouping<int, int>.Key))
        {
            return groupByShaperExpression.KeySelector;
        }

        if (innerExpression is NewExpression newExpression)
        {
            var index = newExpression.Members?.IndexOf(memberExpression.Member);
            if (index >= 0)
            {
                return newExpression.Arguments[index.Value];
            }
        }

        var mayBeMemberInitExpression = innerExpression.UnwrapTypeConversion(out _);
        if (mayBeMemberInitExpression is MemberInitExpression memberInitExpression
            && memberInitExpression.Bindings.SingleOrDefault(
                mb => mb.Member.IsSameAs(memberExpression.Member)) is MemberAssignment memberAssignment)
        {
            return memberAssignment.Expression;
        }

        return memberExpression.Update(innerExpression);
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.TryGetEFPropertyArguments(out var entityExpression, out var propertyName))
        {
            var newEntityExpression = Visit(entityExpression);
            if (newEntityExpression is NewExpression newExpression)
            {
                var index = newExpression.Members?.Select(m => m.Name).IndexOf(propertyName);
                if (index >= 0)
                {
                    return newExpression.Arguments[index.Value];
                }
            }

            var mayBeMemberInitExpression = newEntityExpression.UnwrapTypeConversion(out _);
            if (mayBeMemberInitExpression is MemberInitExpression memberInitExpression
                && memberInitExpression.Bindings.SingleOrDefault(
                    mb => mb.Member.Name == propertyName) is MemberAssignment memberAssignment)
            {
                return memberAssignment.Expression;
            }

            return methodCallExpression.Update(null, new[] { newEntityExpression, methodCallExpression.Arguments[1] });
        }

        return base.VisitMethodCall(methodCallExpression);
    }
}
