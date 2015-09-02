// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Extensions
{
    public static class ModelBuilderContributorDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder UseConvention<TContributor>(this DbContextOptionsBuilder builder)
            where TContributor : class, IModelBuilderConvention
        {
            var extension = builder.Options.FindExtension<ModelBuilderContributorExtension>() ??
                            new ModelBuilderContributorExtension();
            extension.AddConvention<TContributor>();
            ((IDbContextOptionsBuilderInfrastructure) builder).AddOrUpdateExtension(extension);
            return builder;
        }
    }
}
