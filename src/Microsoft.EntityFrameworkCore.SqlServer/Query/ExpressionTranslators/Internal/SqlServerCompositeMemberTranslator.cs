// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerCompositeMemberTranslator()
        {
            var sqlServerTranslators = new List<IMemberTranslator>
            {
                new SqlServerStringLengthTranslator(),
                new SqlServerDateTimeNowTranslator(),
                new SqlServerDateTimeDateComponentTranslator(),
                new SqlServerDateTimeDatePartComponentTranslator()
            };

            AddTranslators(sqlServerTranslators);
        }
    }
}
