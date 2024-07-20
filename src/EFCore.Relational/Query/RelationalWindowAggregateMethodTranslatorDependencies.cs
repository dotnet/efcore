// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// todo
    /// </summary>
    public record RelationalWindowAggregateMethodTranslatorDependencies
    {
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="sqlExpressionFactory">todo</param>
        public RelationalWindowAggregateMethodTranslatorDependencies(ISqlExpressionFactory sqlExpressionFactory)
        {
            SqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        /// todo
        /// </summary>
        public virtual ISqlExpressionFactory SqlExpressionFactory { get; }
    }
}
