// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.SqlServer.Metadata.ModelConventions
{
    public class SqlServerIdentityStrategyConvention : IModelConvention
    {
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            modelBuilder.Annotation(
                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ItentityStrategy,
                SqlServerIdentityStrategy.IdentityColumn.ToString(),
                ConfigurationSource.Convention);
            return modelBuilder;
        }
    }
}
