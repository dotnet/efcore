// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class SqlServerValueGenerationStrategyConvention : IModelConvention
    {
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            modelBuilder.SqlServer(ConfigurationSource.Convention).ValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);

            return modelBuilder;
        }
    }
}
