// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable
#pragma warning disable EF8001 // Owned JSON entities are obsolete

public abstract class PropertyValuesRelationalTestBase<TFixture>(TFixture fixture)
    : PropertyValuesTestBase<TFixture>(fixture)
    where TFixture : PropertyValuesRelationalTestBase<TFixture>.PropertyValuesRelationalFixture, new()
{
    public abstract class PropertyValuesRelationalFixture : PropertyValuesFixtureBase
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<School>(b => b.ComplexCollection(e => e.Departments, b => b.ToJson()));
        }
    }
}
