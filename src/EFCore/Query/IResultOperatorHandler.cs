// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A handler for <see cref="ResultOperatorBase" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IResultOperatorHandler
    {
        /// <summary>
        ///     Handles a result operator.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The entity query model visitor. </param>
        /// <param name="resultOperator"> The result operator. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     A compiled query expression fragment representing the result operator.
        /// </returns>
        Expression HandleResultOperator(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] ResultOperatorBase resultOperator,
            [NotNull] QueryModel queryModel);
    }
}
