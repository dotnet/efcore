// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class SqlServerDesignTimeProviderServicesTest : DesignTimeProviderServicesTest
    {
        protected override Assembly GetRuntimeAssembly()
            => typeof(SqlServerConnection).Assembly;

        protected override Type GetDesignTimeServicesType()
            => typeof(SqlServerDesignTimeServices);
    }
}
