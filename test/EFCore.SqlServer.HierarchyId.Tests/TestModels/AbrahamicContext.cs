// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.TestModels;

internal class AbrahamicContext : DbContext
{
    private readonly TestSqlLoggerFactory _loggerFactory = new();

    public DbSet<Patriarch> Patriarchy { get; set; }
    public DbSet<ConvertedPatriarch> ConvertedPatriarchy { get; set; }

    public string Sql
        => _loggerFactory.Sql;

    public void ClearSql()
        => _loggerFactory.Clear();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlServer(
                SqlServerTestStore.CreateConnectionString("HierarchyIdTests"),
                x => x.UseHierarchyId())
            .UseLoggerFactory(_loggerFactory);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patriarch>()
            .HasData(
                new Patriarch { Id = HierarchyId.GetRoot(), Name = "Abraham" },
                new Patriarch { Id = HierarchyId.Parse("/1/"), Name = "Isaac" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/"), Name = "Jacob" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/1/"), Name = "Reuben" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/2/"), Name = "Simeon" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/3/"), Name = "Levi" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/4/"), Name = "Judah" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/5/"), Name = "Issachar" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/6/"), Name = "Zebulun" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/7/"), Name = "Dan" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/8/"), Name = "Naphtali" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/9/"), Name = "Gad" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/10/"), Name = "Asher" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/11.1/"), Name = "Ephraim" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/11.2/"), Name = "Manasseh" },
                new Patriarch { Id = HierarchyId.Parse("/1/1/12/"), Name = "Benjamin" });

        modelBuilder.Entity<ConvertedPatriarch>(
            b =>
            {
                b.Property(e => e.HierarchyId)
                    .HasConversion(v => HierarchyId.Parse(v), v => v.ToString());

                b.HasData(
                    new ConvertedPatriarch
                    {
                        Id = 1,
                        HierarchyId = HierarchyId.GetRoot().ToString(),
                        Name = "Abraham"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 2,
                        HierarchyId = HierarchyId.Parse("/1/").ToString(),
                        Name = "Isaac"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 3,
                        HierarchyId = HierarchyId.Parse("/1/1/").ToString(),
                        Name = "Jacob"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 4,
                        HierarchyId = HierarchyId.Parse("/1/1/1/").ToString(),
                        Name = "Reuben"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 5,
                        HierarchyId = HierarchyId.Parse("/1/1/2/").ToString(),
                        Name = "Simeon"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 6,
                        HierarchyId = HierarchyId.Parse("/1/1/3/").ToString(),
                        Name = "Levi"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 7,
                        HierarchyId = HierarchyId.Parse("/1/1/4/").ToString(),
                        Name = "Judah"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 8,
                        HierarchyId = HierarchyId.Parse("/1/1/5/").ToString(),
                        Name = "Issachar"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 9,
                        HierarchyId = HierarchyId.Parse("/1/1/6/").ToString(),
                        Name = "Zebulun"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 10,
                        HierarchyId = HierarchyId.Parse("/1/1/7/").ToString(),
                        Name = "Dan"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 11,
                        HierarchyId = HierarchyId.Parse("/1/1/8/").ToString(),
                        Name = "Naphtali"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 12,
                        HierarchyId = HierarchyId.Parse("/1/1/9/").ToString(),
                        Name = "Gad"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 13,
                        HierarchyId = HierarchyId.Parse("/1/1/10/").ToString(),
                        Name = "Asher"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 14,
                        HierarchyId = HierarchyId.Parse("/1/1/11.1/").ToString(),
                        Name = "Ephraim"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 15,
                        HierarchyId = HierarchyId.Parse("/1/1/11.2/").ToString(),
                        Name = "Manasseh"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 16,
                        HierarchyId = HierarchyId.Parse("/1/1/12/").ToString(),
                        Name = "Benjamin"
                    });
            });
    }
}
