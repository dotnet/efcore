// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteCompositeMemberTranslator([NotNull] RelationalCompositeMemberTranslatorDependencies dependencies)
            : base(dependencies)
        {
            var sqliteTranslators = new List<IMemberTranslator>
            {
                new SqliteDateTimeMemberTranslator(),
                new SqliteStringLengthTranslator()
            };

            AddTranslators(sqliteTranslators);
        }
    }
}
