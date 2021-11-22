// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class TemporalComplexNavigationsSharedTypeQuerySqlServerFixture : ComplexNavigationsSharedTypeQuerySqlServerFixture
{
    protected override string StoreName { get; } = "TemporalComplexNavigationsOwned";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Level1>().ToTable(tb => tb.IsTemporal());
    }
}
