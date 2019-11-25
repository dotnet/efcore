// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public static class FakeRelationalDbContextOptionsExtension
    {
        public static DbContextOptionsBuilder UseFakeRelational(
            this DbContextOptionsBuilder optionsBuilder,
            Action<FakeRelationalDbContextOptionsBuilder> fakeRelationalOptionsAction = null)
        {
            return optionsBuilder.UseFakeRelational("Database=Fake", fakeRelationalOptionsAction);
        }

        public static DbContextOptionsBuilder UseFakeRelational(
            this DbContextOptionsBuilder optionsBuilder,
            string connectionString,
            Action<FakeRelationalDbContextOptionsBuilder> fakeRelationalOptionsAction = null)
        {
            return optionsBuilder.UseFakeRelational(new FakeDbConnection(connectionString), fakeRelationalOptionsAction);
        }

        public static DbContextOptionsBuilder UseFakeRelational(
            this DbContextOptionsBuilder optionsBuilder,
            DbConnection connection,
            Action<FakeRelationalDbContextOptionsBuilder> fakeRelationalOptionsAction = null)
        {
            var extension = (FakeRelationalOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            fakeRelationalOptionsAction?.Invoke(new FakeRelationalDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        private static FakeRelationalOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.Options.FindExtension<FakeRelationalOptionsExtension>()
                ?? new FakeRelationalOptionsExtension();
    }
}
