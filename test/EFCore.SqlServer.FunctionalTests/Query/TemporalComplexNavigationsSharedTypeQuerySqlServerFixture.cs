// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TemporalComplexNavigationsSharedTypeQuerySqlServerFixture : ComplexNavigationsSharedTypeQuerySqlServerFixture
    {
        protected override string StoreName { get; } = "TemporalComplexNavigationsOwned";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Level1>().ToTable(tb => tb.IsTemporal());
        }
    }
}
