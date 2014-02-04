// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.SqlServer
{
    using Microsoft.Data.Entity;
    using Microsoft.Data.Entity.Utilities;

    public static class ApiExtensions
    {
        public static EntityContext CreateContext(
            this EntityConfiguration entityConfiguration, string nameOrConnectionString)
        {
            Check.NotNull(entityConfiguration, "entityConfiguration");
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            entityConfiguration.DataStore = new SqlServerDataStore(nameOrConnectionString);

            return new EntityContext(entityConfiguration);
        }
    }
}
