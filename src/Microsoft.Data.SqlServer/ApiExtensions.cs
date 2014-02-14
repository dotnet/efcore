// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public static class ApiExtensions
    {
        public static EntityContext CreateContext(
            [NotNull] this EntityConfiguration entityConfiguration, [NotNull] string nameOrConnectionString)
        {
            Check.NotNull(entityConfiguration, "entityConfiguration");
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            entityConfiguration.DataStore = new SqlServerDataStore(nameOrConnectionString);

            return new EntityContext(entityConfiguration);
        }
    }
}
