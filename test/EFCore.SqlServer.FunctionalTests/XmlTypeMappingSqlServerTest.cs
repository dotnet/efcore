// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class XmlTypeMappingSqlServerTest : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .AddEntityFrameworkSqlServer()
        .BuildServiceProvider(validateScopes: true);

    // The grinning-face emoji is outside the BMP and is lost when an xml value is sent to the server as a
    // non-Unicode string, which is what makes it a good probe for the SqlXml/SqlDbType.Xml parameter path.
    private const string Emoji = "\U0001F600";

    [Theory]
    [InlineData("<root>" + Emoji + "</root>", "<root>" + Emoji + "</root>")]
    // An explicit non-UTF-16 prolog is accepted because the value is sent as 'xml', not 'nvarchar(max)'.
    [InlineData("<?xml version=\"1.0\" encoding=\"utf-8\"?><root>" + Emoji + "</root>", "<root>" + Emoji + "</root>")]
    [InlineData("<?xml version=\"1.0\" encoding=\"utf-16\"?><root>a</root>", "<root>a</root>")]
    // Content forms that the 'xml' store type accepts beyond a single well-formed document.
    [InlineData("", "")]
    [InlineData("text fragment", "text fragment")]
    [InlineData("<a/><b/>", "<a /><b />")]
    public async Task Xml_value_round_trips(string value, string expected)
    {
        int id;
        await using (var context = new XmlContext(_serviceProvider, TestStore.Name))
        {
            await context.Database.EnsureCreatedResilientlyAsync();
            var document = new XmlDocument { Content = value };
            context.Documents.Add(document);
            await context.SaveChangesAsync();
            id = document.Id;
        }

        await using (var context = new XmlContext(_serviceProvider, TestStore.Name))
        {
            // xml columns cannot be used in a WHERE comparison, so the row is fetched by its key.
            var roundTripped = (await context.Documents.SingleAsync(d => d.Id == id)).Content;
            Assert.Equal(expected, roundTripped);
        }
    }

    private class XmlContext(IServiceProvider serviceProvider, string databaseName) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly string _databaseName = databaseName;

        public DbSet<XmlDocument> Documents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<XmlDocument>().Property(e => e.Content).HasColumnType("xml");
    }

    private class XmlDocument
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }

    protected SqlServerTestStore TestStore { get; private set; }

    public async ValueTask InitializeAsync()
        => TestStore = await SqlServerTestStore.CreateInitializedAsync(nameof(XmlTypeMappingSqlServerTest));

    public async ValueTask DisposeAsync()
        => await TestStore.DisposeAsync();
}
