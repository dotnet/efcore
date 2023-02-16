// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

public class NorthwindSqliteContext : NorthwindRelationalContext
{
    public NorthwindSqliteContext(DbContextOptions options)
        : base(options)
    {
    }
}
