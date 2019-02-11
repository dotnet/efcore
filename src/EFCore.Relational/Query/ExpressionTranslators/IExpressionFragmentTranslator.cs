// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     <para>
    ///         A LINQ expression translator for arbitrary CLR expression fragments.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
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
