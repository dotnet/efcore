// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class F1ULongSqlServerFixture : F1SqlServerFixtureBase<ulong>
    {
        protected override string StoreName
            => "F1TestULong";

        protected override void BuildModelExternal(ModelBuilder modelBuilder)
        {
            base.BuildModelExternal(modelBuilder);

            modelBuilder
                .Entity<OptimisticOptionalChild>()
                .Property(x => x.Version)
                .IsRowVersion()
                .HasConversion<byte[]>();

            modelBuilder
                .Entity<OptimisticParent>()
                .HasData(
                    new OptimisticParent()
                    {
                        Id = new("AF8451C3-61CB-4EDA-8282-92250D85EF03"),
                    }
                );
        }

        public class OptimisticOptionalChild
        {
            public Guid Id { get; set; }
            public ICollection<OptimisticParent> Parents { get; set; }
            public ulong Version { get; set; }
        }

        public class OptimisticParent
        {
            public Guid Id { get; set; }
            public OptimisticOptionalChild OptionalChild { get; set; }
        }
    }

    public class F1SqlServerFixture : F1SqlServerFixtureBase<byte[]>
    {
    }

    public abstract class F1SqlServerFixtureBase<TRowVersion> : F1RelationalFixture<TRowVersion>
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override TestHelpers TestHelpers
            => SqlServerTestHelpers.Instance;

        protected override void BuildModelExternal(ModelBuilder modelBuilder)
        {
            base.BuildModelExternal(modelBuilder);

            modelBuilder.Entity<TitleSponsor>()
                .OwnsOne(
                    s => s.Details, eb =>
                    {
                        eb.Property(d => d.Space).HasColumnType("decimal(18,2)");
                    });
        }
    }
}
