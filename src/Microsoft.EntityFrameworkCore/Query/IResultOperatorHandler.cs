// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A handler for <see cref="ResultOperatorBase" /> instances.
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
