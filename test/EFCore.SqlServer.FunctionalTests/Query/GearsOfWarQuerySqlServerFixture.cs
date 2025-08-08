// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class GearsOfWarQuerySqlServerFixture : GearsOfWarQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<City>().Property(g => g.Location).HasColumnType("varchar(100)");

        modelBuilder.Entity<Mission>(
            b =>
            {
                // Full-text binary search
                b.Property<byte[]>("BriefingDocument");
                b.Property<string>("BriefingDocumentFileExtension").HasColumnType("nvarchar(16)");
            });
    }

    protected override async Task SeedAsync(GearsOfWarContext context)
    {
        await base.SeedAsync(context);

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
