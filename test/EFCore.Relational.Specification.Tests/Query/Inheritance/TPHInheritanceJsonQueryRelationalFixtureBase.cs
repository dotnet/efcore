// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public abstract class TPHInheritanceJsonQueryRelationalFixtureBase : TPHInheritanceQueryFixture
{
    protected override string StoreName
        => "TPHInheritanceJsonTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Drink>().ComplexProperty(d => d.ParentComplexType, d => d.ToJson().IsRequired(false));
        modelBuilder.Entity<Coke>().ComplexProperty(c => c.ChildComplexType, c => c.ToJson().IsRequired(false));
        modelBuilder.Entity<Tea>().ComplexProperty(t => t.ChildComplexType, t => t.ToJson().IsRequired(false));

        modelBuilder.Entity<Manufacturer>().OwnsOne(m => m.Headquarters, ob => ob.ComplexProperty(o => o.Address, a => a.ToJson()));
        modelBuilder.Entity<Brewery>().OwnsOne(b => b.Brewhouse, ob => ob.ComplexProperty(o => o.Address, a => a.ToJson()));
    }
}
