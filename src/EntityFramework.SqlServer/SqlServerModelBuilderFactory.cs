// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.SqlServer.Metadata.ModelConventions;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerModelBuilderFactory : ModelBuilderFactory
    {
        protected override ConventionSet CreateConventionSet()
        {
            var conventions = base.CreateConventionSet();

            conventions.ModelConventions.Add(new SqlServerValueGenerationStrategyConvention());

            return conventions;
        }
    }
}
