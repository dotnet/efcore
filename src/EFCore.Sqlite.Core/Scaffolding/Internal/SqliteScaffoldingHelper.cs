// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class SqliteScaffoldingHelper : IScaffoldingHelper
    {
        public virtual string GetProviderOptionsBuilder(string connectionString)
        {
            return $"{nameof(SqliteDbContextOptionsBuilderExtensions.UseSqlite)}({CSharpUtilities.Instance.GenerateVerbatimStringLiteral(connectionString)});";
        }
    }
}
