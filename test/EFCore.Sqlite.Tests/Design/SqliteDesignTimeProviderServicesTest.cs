// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Design;

public class SqliteDesignTimeProviderServicesTest : DesignTimeProviderServicesTest
{
    protected override Assembly GetRuntimeAssembly()
        => typeof(SqliteRelationalConnection).Assembly;

    protected override Type GetDesignTimeServicesType()
        => typeof(SqliteDesignTimeServices);
}
