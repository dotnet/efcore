// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     A LINQ expression translator for CLR <see cref="MethodCallExpression" /> expressions.
    /// </summary>
    public interface ICompositeMethodCallTranslator
    {
        /// <summary>
        ///     Translates the given method call expression.
        /// </summary>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="model"> The current model. </param>
        /// <returns>
        ///     A SQL expression representing the translated MethodCallExpression.
        /// </returns>
        Expression Translate([NotNull] MethodCallExpression methodCallExpression, [NotNull] IModel model);
    }
}
