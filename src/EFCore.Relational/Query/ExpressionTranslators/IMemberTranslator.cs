// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     A LINQ expression translator for CLR <see cref="MemberExpression" /> expressions.
    /// </summary>
    public interface IMemberTranslator
    {
        /// <summary>
        ///     Translates the given member expression.
        /// </summary>
        /// <param name="memberExpression"> The member expression. </param>
        /// <returns>
        ///     A SQL expression representing the translated MemberExpression.
        /// </returns>
        Expression Translate([NotNull] MemberExpression memberExpression);
    }
}
