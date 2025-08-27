// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests.Migrations;

/// <summary>
/// Integration test that simulates the user's original issue scenario more closely
/// </summary>
public class InvalidColumnTypeIntegrationTest
{
    [ConditionalFact]
    public void Creating_migration_with_invalid_column_type_should_not_throw_null_reference()
    {
        var options = new DbContextOptionsBuilder<InvalidColumnTypeContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TestInvalidColumnType;Trusted_Connection=true;MultipleActiveResultSets=true")
            .Options;

        using var context = new InvalidColumnTypeContext(options);
        
        // This simulates the scenario where a user has invalid Column attributes
        // Before the fix, this would throw a NullReferenceException
        // After the fix, this should throw a more helpful InvalidOperationException
        var ex = Assert.ThrowsAny<Exception>(() => 
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });

        // We should NOT get a NullReferenceException
        Assert.IsNotType<NullReferenceException>(ex);
        
        // We should get some form of meaningful error instead
        Assert.True(ex is InvalidOperationException || ex is ArgumentException || ex is NotSupportedException);
    }

    public class InvalidColumnTypeContext : DbContext
    {
        public InvalidColumnTypeContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Contract> Contracts { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        
        [Column(TypeName = "decimal(18,2)")] // Invalid for string - this is what the user had
        public string FirstName { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")] // Invalid for string - this is what the user had
        public string LastName { get; set; } = string.Empty;
        
        public int Age { get; set; }
        public List<Contract> Contracts { get; set; } = new();
    }

    public class Contract
    {
        public int Id { get; set; }
        public string ContractLabel { get; set; } = string.Empty;
        public DateTime DateSign { get; set; } = DateTime.Now;
        public int? PersonId { get; set; }
        public Person Person { get; set; }
    }
}