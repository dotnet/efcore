// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqlServerFixture : GearsOfWarQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<City>().Property(g => g.Location).HasColumnType("varchar(100)");

            // Full-text binary search
            modelBuilder.Entity<Mission>()
                .Property<byte[]>("BriefingDocument");

            modelBuilder.Entity<Mission>()
                .Property<string>("BriefingDocumentFileExtension")
                .HasColumnType("nvarchar(16)");
        }

        protected override void Seed(GearsOfWarContext context)
        {
            base.Seed(context);

            // Set up full-text search and add some full-text binary data
            context.Database.ExecuteSqlRaw(
                @"
UPDATE [Missions]
SET
    [BriefingDocumentFileExtension] = '.html',
    [BriefingDocument] = CONVERT(varbinary(max), '<h1>Deploy the Lightmass Bomb to destroy the Locust Horde</h1>')
WHERE [Id] = 1;

UPDATE [Missions]
SET
    [BriefingDocumentFileExtension] = '.html',
    [BriefingDocument] = CONVERT(varbinary(max), '<h1>Two-day long military counterattack to kill the remaining Locust</h1>')
WHERE [Id] = 2;

IF (FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = 'GearsOfWar_FTC')
    BEGIN
        DROP FULLTEXT CATALOG GearsOfWar_FTC;
    END

    CREATE FULLTEXT CATALOG GearsOfWar_FTC AS DEFAULT;
    CREATE FULLTEXT INDEX ON Missions (BriefingDocument TYPE COLUMN BriefingDocumentFileExtension) KEY INDEX PK_Missions;

    WAITFOR DELAY '00:00:03';
END");
        }
    }
}
