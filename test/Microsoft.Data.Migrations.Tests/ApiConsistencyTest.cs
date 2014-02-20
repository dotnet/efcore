// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity;
using Microsoft.Data.Migrations.Model;

namespace Microsoft.Data.Migrations
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override Assembly TargetAssembly
        {
            get { return typeof(MigrationOperation).Assembly; }
        }
    }
}
