// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     A LINQ expression translator for arbitrary CLR expression fragments.
    /// </summary>
    public interface IExpressionFragmentTranslator
    {
        /// <summary>
        ///     Translates the given expression.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <returns>
        ///     A SQL expression representing the translated expression.
        /// </returns>
        Expression Translate([NotNull] Expression expression);
    }
}
