// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;

namespace Microsoft.Data.Entity.SqlServer.Metadata.ModelConventions
{
    public class SqlServerValueGenerationStrategyConvention : IModelConvention
    {
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            modelBuilder.Annotation(
                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration,
                SqlServerValueGenerationStrategy.Identity.ToString(),
                ConfigurationSource.Convention);
            return modelBuilder;
        }
    }
}
