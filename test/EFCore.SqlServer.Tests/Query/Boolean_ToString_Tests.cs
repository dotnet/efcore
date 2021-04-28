// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SqlServerObjectToStringTranslator_Tests
    {
        public class SimpleEntity
        {
            public long Id { get; set; }
            
            public bool IsEnabled { get; set; }
        }

        public class TestDbContext : DbContext
        {
            public DbSet<SimpleEntity> SimpleEntities { get; set; }

            public TestDbContext(DbContextOptions dbContextOptions) :
                base(dbContextOptions)
            {
            }
        }

        [ConditionalFact]
        public void Test_boolean_ToString_translation()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            using var dbContext = new TestDbContext(optionsBuilder.Options);
            
            var actual = dbContext.SimpleEntities.Select(e => e.IsEnabled.ToString()).ToQueryString();
            var expected =

@"SELECT CASE
    WHEN [s].[IsEnabled] = CAST(0 AS bit) THEN N'False'
    ELSE N'True'
END
FROM [SimpleEntities] AS [s]";

            Assert.Equal(expected, actual);
        }
    }
}
