// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels
{
    public class SqlServerF1Context : RelationalF1Context
    {
        public SqlServerF1Context(DbContextOptions options)
            : base(options)
        {
        }

        public SqlServerF1Context(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ForSqlServer().UseSequence();
        }

        public static Task<SqlServerTestStore> GetSharedStoreAsync()
        {
            return SqlServerTestStore.GetOrCreateSharedAsync(DatabaseName, async () =>
                {
                    var options = new DbContextOptions()
                        .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName));

                    using (var context = new SqlServerF1Context(options))
                    {
                        await ConcurrencyModelInitializer.SeedAsync(context);
                    }
                });
        }

        public static SqlServerF1Context Create(SqlServerTestStore testStore)
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection
                    .BuildServiceProvider();

            var options
                = new DbContextOptions()
                    .UseSqlServer(testStore.Connection);

            var context = new SqlServerF1Context(serviceProvider, options);
            context.Database.AsRelational().Connection.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
