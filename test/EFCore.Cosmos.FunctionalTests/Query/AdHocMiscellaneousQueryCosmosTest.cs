// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocMiscellaneousQueryCosmosTest : NonSharedModelTestBase
{
    #region 34911

    [ConditionalFact]
    public virtual async Task Enum_partition_key()
    {
        var contextFactory = await InitializeAsync<Context34911>(
            onModelCreating: b => b.Entity<Context34911.Member>().HasPartitionKey(d => d.MemberType),
            seed: async context =>
            {
                context.Members.Add(new Context34911.Member { MemberType = Context34911.MemberType.Admin, Name = "Some Admin" });
                await context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateContext())
        {
            var admin = await context.Members.Where(p => p.MemberType == Context34911.MemberType.Admin).SingleAsync();
            Assert.Equal("Some Admin", admin.Name);
        }
    }

    protected class Context34911(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Member> Members { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Member>().HasData(new Member { Id = 1, Name = "Product 1" });

        public class Member
        {
            public int Id { get; set; }
            public MemberType MemberType { get; set; }
            public string Name { get; set; }
        }

        public enum MemberType
        {
            User,
            Admin
        }
    }

    #endregion 34911

    protected override string StoreName
        => "AdHocMiscellaneousQueryTests";

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(b => b.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;
}
