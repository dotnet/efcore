// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Various helpers for query translation.
/// </summary>
public static class QueryHelpers
{
    /// <summary>
    ///     Returns whether the given expression represents a member access and if so, returns the decomposed base expression and the member
    ///     identity.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <param name="model">The model being used.</param>
    /// <param name="baseExpression">The given expression, with the top-level member access node removed.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="expression"/> represents a member access, <see langword="false" /> otherwise.
    /// </returns>
    public static bool IsMemberAccess(
        Expression expression,
        IModel model,
        [NotNullWhen(true)] out Expression? baseExpression)
        => IsMemberAccess(expression, model, out baseExpression, out _);

    /// <summary>
    ///     Returns whether the given expression represents a member access and if so, returns the decomposed base expression and the member
    ///     identity.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <param name="model">The model being used.</param>
    /// <param name="baseExpression">The given expression, with the top-level member access node removed.</param>
    /// <param name="memberIdentity">A <see cref="MemberIdentity" /> representing the member being accessed.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="expression"/> represents a member access, <see langword="false" /> otherwise.
    /// </returns>
    public static bool IsMemberAccess(
        Expression expression,
        IModel model,
        [NotNullWhen(true)] out Expression? baseExpression,
        out MemberIdentity memberIdentity)
    {
        switch (expression)
        {
            case MemberExpression { Expression: not null } member:
                baseExpression = member.Expression;
                memberIdentity = MemberIdentity.Create(member.Member);
                return true;
            case MethodCallExpression methodCall
                when methodCall.TryGetEFPropertyArguments(out baseExpression, out var propertyName)
                || methodCall.TryGetIndexerArguments(model, out baseExpression, out propertyName):
                memberIdentity = MemberIdentity.Create(propertyName);
                return true;
            default:
                memberIdentity = MemberIdentity.None;
                baseExpression = null;
                return false;
        }
    }
}
