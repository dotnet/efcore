// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Tests
{
    public static class TestHelperExtensions
    {
        public static EntityServicesBuilder AddProviderServices(this EntityServicesBuilder entityServicesBuilder)
        {
            return entityServicesBuilder.AddSqlServer();
        }

        public static DbContextOptions UseProviderOptions(this DbContextOptions options)
        {
            return options.UseSqlServer(new SqlConnection("Database=DummyDatabase"));
        }
    }
}
