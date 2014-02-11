// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity;

namespace Microsoft.Data.SqlServer
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override Assembly TargetAssembly
        {
            get { return typeof(SqlServerDataStore).Assembly; }
        }
    }
}
