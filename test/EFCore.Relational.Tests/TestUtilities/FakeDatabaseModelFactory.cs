// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class FakeDatabaseModelFactory : IDatabaseModelFactory
{
    public virtual DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
        => throw new NotImplementedException();

    public virtual DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
        => throw new NotImplementedException();
}
