// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class SqlServerScaffoldingCodeGenerator : IScaffoldingProviderCodeGenerator
    {
        public virtual string GenerateUseProvider(string connectionString)
        {
            return $".{nameof(SqlServerDbContextOptionsExtensions.UseSqlServer)}({GenerateVerbatimStringLiteral(connectionString)})";
        }

        private static string GenerateVerbatimStringLiteral(string value) => "@\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
