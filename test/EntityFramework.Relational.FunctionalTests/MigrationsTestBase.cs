// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class MigrationsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : MigrationsFixtureBase, new()
    {
        private readonly TFixture _fixture;

        public MigrationsTestBase(TFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Can_apply_all_migrations()
        {
            using (var db = _fixture.CreateContext())
            {
                db.Database.EnsureDeleted();

                db.Database.ApplyMigrations();

                var history = db.GetService().GetRequiredService<IHistoryRepository>();
                Assert.Collection(
                    history.GetAppliedMigrations(),
                    x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
                    x => Assert.Equal("00000000000002_Migration2", x.MigrationId));
            }
        }

        [Fact]
        public void Can_apply_one_migration()
        {
            using (var db = _fixture.CreateContext())
            {
                db.Database.EnsureDeleted();

                var migrator = db.GetService().GetRequiredService<IMigrator>();
                migrator.ApplyMigrations("Migration1");

                var history = db.GetService().GetRequiredService<IHistoryRepository>();
                Assert.Collection(
                    history.GetAppliedMigrations(),
                    x => Assert.Equal("00000000000001_Migration1", x.MigrationId));
            }
        }

        [Fact]
        public void Can_revert_all_migrations()
        {
            using (var db = _fixture.CreateContext())
            {
                db.Database.EnsureDeleted();
                db.Database.ApplyMigrations();

                var migrator = db.GetService().GetRequiredService<IMigrator>();
                migrator.ApplyMigrations(Migrator.InitialDatabase);

                var history = db.GetService().GetRequiredService<IHistoryRepository>();
                Assert.Empty(history.GetAppliedMigrations());
            }
        }

        [Fact]
        public void Can_revert_one_migrations()
        {
            using (var db = _fixture.CreateContext())
            {
                db.Database.EnsureDeleted();
                db.Database.ApplyMigrations();

                var migrator = db.GetService().GetRequiredService<IMigrator>();
                migrator.ApplyMigrations("Migration1");

                var history = db.GetService().GetRequiredService<IHistoryRepository>();
                Assert.Collection(
                    history.GetAppliedMigrations(),
                    x => Assert.Equal("00000000000001_Migration1", x.MigrationId));
            }
        }
    }
}
