// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public interface ILiftableConstantProcessor
{
    /// <summary>
    ///     Exposes all constants that have been lifted during the last invocation of <see cref="LiftedConstants" />.
    /// </summary>
    IReadOnlyList<(ParameterExpression Parameter, Expression Expression)> LiftedConstants { get; }

    /// <summary>
    ///     Inlines all liftable constants as simple <see cref="ConstantExpression" /> nodes in the tree, containing the result of
    ///     evaluating the liftable constants' resolvers.
    /// </summary>
    /// <param name="expression">An expression containing <see cref="LiftableConstantExpression" /> nodes.</param>
    /// <returns>
    ///     An expression tree containing <see cref="ConstantExpression" /> nodes instead of <see cref="LiftableConstantExpression" /> nodes.
    /// </returns>
    /// <remarks>
    ///     Liftable constant inlining is performed in the regular, non-precompiled query pipeline flow.
    /// </remarks>
    Expression InlineConstants(Expression expression);

    /// <summary>
    ///     Lifts all <see cref="LiftableConstantExpression" /> nodes, embedding <see cref="ParameterExpression" /> in their place and
    ///     exposing the parameter and resolver via <see cref="LiftedConstants" />.
    /// </summary>
    /// <param name="expression">An expression containing <see cref="LiftableConstantExpression" /> nodes.</param>
    /// <param name="contextParameter">
    ///     The <see cref="ParameterExpression" /> to be embedded in the liftable constant nodes' resolvers, instead of their lambda
    ///     parameter.
    /// </param>
    /// <param name="variableNames">
    ///     A set of variables already in use, for uniquification. Any generates variables will be added to this set.
    /// </param>
    /// <returns>
    ///     An expression tree containing <see cref="ParameterExpression" /> nodes instead of <see cref="LiftableConstantExpression" /> nodes.
    /// </returns>
    /// <remarks>
    ///     Constant lifting is performed in the precompiled query pipeline flow.
    /// </remarks>
    Expression LiftConstants(Expression expression, ParameterExpression contextParameter, HashSet<string> variableNames);
}
