// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Internal.Tests;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
{
    public class SqlServerModelValidatorTest : RelationalModelValidatorTest
    {
        public override void Detects_duplicate_column_names()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            modelBuilder.Entity<Product>();
            modelBuilder.Entity<Product>().Property(b => b.Name).ForSqlServerHasColumnName("Id");

            VerifyError(RelationalStrings.DuplicateColumnName("Id", typeof(Product).FullName, "Name"), modelBuilder.Model);
        }

        private class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        protected override ModelValidator CreateModelValidator()
            => new SqlServerModelValidator(
                new Logger<SqlServerModelValidator>(
                    new ListLoggerFactory(Log, l => l == typeof(SqlServerModelValidator).FullName)),
                new TestSqlServerAnnotationProvider());
    }

    public class TestSqlServerAnnotationProvider : TestAnnotationProvider
    {
        public override IRelationalPropertyAnnotations For(IProperty property) => new SqlServerPropertyAnnotations(property);
    }
}
