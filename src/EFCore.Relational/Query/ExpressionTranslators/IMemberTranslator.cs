// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     <para>
    ///         A LINQ expression translator for CLR <see cref="MemberExpression" /> expressions.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
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
