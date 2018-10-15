// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerDbContextOptionsExtensionsTest
    {
        [Fact]
        public void Can_add_extension_with_max_batch_size()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie", b => b.MaxBatchSize(123));

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal(123, extension.MaxBatchSize);
        }

        [Fact]
        public void Can_add_extension_with_command_timeout()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie", b => b.CommandTimeout(30));

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal(30, extension.CommandTimeout);
        }

        [Fact]
        public void Can_add_extension_with_connection_string()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection_string_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlServer("Database=Whisper");

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            var connection = new SqlConnection();

            optionsBuilder.UseSqlServer(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void Can_add_extension_with_connection_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            var connection = new SqlConnection();

            optionsBuilder.UseSqlServer(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void Can_add_extension_with_legacy_paging()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

            optionsBuilder.UseSqlServer("Database=Kilimanjaro", b => b.UseRowNumberForPaging());

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.True(extension.RowNumberPaging.HasValue);
            Assert.True(extension.RowNumberPaging.Value);
        }

        [Fact]
        public void Can_add_extension_with_connection_stringbuilder_action_generic()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SampleDbContext>();
            optionsBuilder.UseSqlServer(csb =>
            {
                csb.DataSource = "Kilimanjaro";
            });

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();
            Assert.Equal("Data Source=Kilimanjaro;Initial Catalog=SampleDbContext;ConnectRetryCount=0", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection_stringbuilder_action()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(csb =>
            {
                csb.DataSource = "Kilimanjaro";
            });

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();
            Assert.Equal("Data Source=Kilimanjaro;ConnectRetryCount=0", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        private class SampleDbContext : DbContext { }
    }
}
