// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Update;

public class EntityDataReaderTest
{
    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public CustomerStatus Status { get; set; }
    }

    private enum CustomerStatus
    {
        Active,
        Suspended
    }

    private static IModel BuildModel()
    {
        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers", "sales");
            b.Property(c => c.Id).UseIdentityColumn();
            b.Property(c => c.Name).HasMaxLength(100).IsRequired();
            b.Property(c => c.Email).HasMaxLength(200);
            b.Property(c => c.Status).HasConversion<string>();
        });

        return modelBuilder.FinalizeModel();
    }

    [Fact]
    public void Identity_column_excluded_when_keepIdentity_is_false()
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(Customer))!;
        var storeObject = StoreObjectIdentifier.Table("Customers", "sales");

        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "Alice", Email = "alice@example.com", Status = CustomerStatus.Active }
        };

        using var reader = new EntityDataReader<Customer>(entityType, customers, storeObject, keepIdentity: false);

        Assert.Equal(3, reader.FieldCount);
        Assert.DoesNotContain("Id", reader.ColumnNames);
        Assert.Contains("Name", reader.ColumnNames);
        Assert.Contains("Email", reader.ColumnNames);
        Assert.Contains("Status", reader.ColumnNames);

        Assert.True(reader.Read());
        Assert.Equal("Alice", reader["Name"]);
        Assert.Equal("alice@example.com", reader["Email"]);
        Assert.Equal("Active", reader["Status"]);
        Assert.False(reader.Read());
    }

    [Fact]
    public void Identity_column_included_when_keepIdentity_is_true()
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(Customer))!;
        var storeObject = StoreObjectIdentifier.Table("Customers", "sales");

        var customers = new List<Customer>
        {
            new() { Id = 42, Name = "Bob", Email = null, Status = CustomerStatus.Suspended }
        };

        using var reader = new EntityDataReader<Customer>(entityType, customers, storeObject, keepIdentity: true);

        Assert.Equal(4, reader.FieldCount);
        Assert.Contains("Id", reader.ColumnNames);

        Assert.True(reader.Read());
        Assert.Equal(42, reader["Id"]);
        Assert.Equal("Bob", reader["Name"]);
        Assert.Equal(DBNull.Value, reader["Email"]);
        Assert.True(reader.IsDBNull(reader.GetOrdinal("Email")));
        Assert.Equal("Suspended", reader["Status"]);
    }

    [Fact]
    public async Task Async_streaming_works_correctly()
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(Customer))!;
        var storeObject = StoreObjectIdentifier.Table("Customers", "sales");

        async IAsyncEnumerable<Customer> GetCustomersAsync()
        {
            yield return new Customer { Id = 1, Name = "Stream1", Status = CustomerStatus.Active };
            await Task.Yield();
            yield return new Customer { Id = 2, Name = "Stream2", Status = CustomerStatus.Suspended };
        }

        await using var reader = new EntityDataReader<Customer>(entityType, GetCustomersAsync(), storeObject, keepIdentity: false);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("Stream1", reader["Name"]);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("Stream2", reader["Name"]);

        Assert.False(await reader.ReadAsync());
    }
}
