// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Relational;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerDataStore : RelationalDataStore
    {
        public SqlServerDataStore([NotNull] string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }
    }
}
