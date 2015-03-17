// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.NullSemantics;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class NullSemanticsQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract NullSemanticsContext CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NullSemanticsEntity1>().Key(e => e.Id);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.Id).GenerateValueOnAdd(false);

            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.BoolA).Required(true);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.BoolB).Required(true);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.BoolC).Required(true);

            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.NullableBoolA).Required(false);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.NullableBoolB).Required(false);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.NullableBoolC).Required(false);

            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.IntA).Required(true);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.IntB).Required(true);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.IntC).Required(true);

            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.NullableIntA).Required(false);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.NullableIntB).Required(false);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.NullableIntC).Required(false);

            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringA).Required(true);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringB).Required(true);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringC).Required(true);

            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.NullableStringA).Required(false);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.NullableStringB).Required(false);
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.NullableStringC).Required(false);

            modelBuilder.Entity<NullSemanticsEntity2>().Key(e => e.Id);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.Id).GenerateValueOnAdd(false);

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.BoolA).Required(true);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.BoolB).Required(true);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.BoolC).Required(true);

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.NullableBoolA).Required(false);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.NullableBoolB).Required(false);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.NullableBoolC).Required(false);

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.IntA).Required(true);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.IntB).Required(true);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.IntC).Required(true);

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.NullableIntA).Required(false);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.NullableIntB).Required(false);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.NullableIntC).Required(false);

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringA).Required(true);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringB).Required(true);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringC).Required(true);

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.NullableStringA).Required(false);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.NullableStringB).Required(false);
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.NullableStringC).Required(false);
        }
    }
}