// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.TestUtilities.FakeProvider
{
    public class FakeProviderTestHelpers
    {
        public const string ConnectionString = "Fake Connection String";

        public static FakeRelationalConnection CreateConnection(IDbContextOptions options = null)
            => new FakeRelationalConnection(options ?? CreateOptions());

        public static IDbContextOptions CreateOptions(FakeRelationalOptionsExtension optionsExtension = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(optionsExtension ?? CreateOptionsExtension());

            return optionsBuilder.Options;
        }

        public static FakeRelationalOptionsExtension CreateOptionsExtension()
            => new FakeRelationalOptionsExtension { ConnectionString = ConnectionString };

        public static FakeRelationalOptionsExtension CreateOptionsExtension(FakeDbConnection connection)
            => new FakeRelationalOptionsExtension { Connection = connection };

        public static FakeDbConnection CreateDbConnection(FakeCommandExecutor executor)
            => new FakeDbConnection(ConnectionString, executor);
    }
}
