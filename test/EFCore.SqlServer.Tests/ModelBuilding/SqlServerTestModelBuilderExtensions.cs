// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public static class SqlServerTestModelBuilderExtensions
    {
        public static ModelBuilderTest.TestIndexBuilder ForSqlServerIsClustered(
            this ModelBuilderTest.TestIndexBuilder builder, bool clustered = true)
        {
            var indexBuilder = builder.GetInfrastructure();
            indexBuilder.ForSqlServerIsClustered(clustered);
            return builder;
        }
    }
}
