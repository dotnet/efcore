// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerDataStore : RelationalDataStore
    {
        public SqlServerDataStore([NotNull] string nameOrConnectionString)
            : base(Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString"))
        {
        }
    }
}
