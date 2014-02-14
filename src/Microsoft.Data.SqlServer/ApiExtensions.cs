// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.Data.Entity;

namespace Microsoft.Data.SqlServer
{
    public static class ApiExtensions
    {
        public static EntityContext CreateContext(
            [NotNull] this EntityConfiguration entityConfiguration, [NotNull] string nameOrConnectionString)
        {
            entityConfiguration.DataStore = new SqlServerDataStore(nameOrConnectionString);



            return new EntityContext(entityConfiguration);
        }
    }
}
