// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public abstract class InheritanceQueryRelationalFixtureBase : InheritanceQueryFixtureBase, ITestSqlLoggerFactory
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // In relational, complex collections are only supported as JSON and must be explicitly configured as such
        modelBuilder.Entity<Drink>().ComplexCollection(n => n.ComplexTypeCollection, n => n.ToJson());
    }
}
