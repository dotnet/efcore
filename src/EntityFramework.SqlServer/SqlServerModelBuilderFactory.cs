// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.SqlServer.Metadata.ModelConventions;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerModelBuilderFactory : ModelBuilderFactory, ISqlServerModelBuilderFactory
    {
        protected override ConventionSet CreateConventionSet()
        {
            var conventions = base.CreateConventionSet();

            var sqlServerValueGenerationStrategyConvention = new SqlServerValueGenerationStrategyConvention();
            conventions.KeyAddedConventions.Add(sqlServerValueGenerationStrategyConvention);

            conventions.ForeignKeyAddedConventions.Add(sqlServerValueGenerationStrategyConvention);

            conventions.ForeignKeyRemovedConventions.Add(sqlServerValueGenerationStrategyConvention);

            conventions.ModelConventions.Add(sqlServerValueGenerationStrategyConvention);

            return conventions;
        }
    }
}
