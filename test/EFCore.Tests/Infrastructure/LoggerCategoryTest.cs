// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class DbLoggerCategoryTest
    {
        [Fact]
        public void Logger_categories_have_the_correct_names()
        {
            Assert.Equal("Microsoft.EntityFrameworkCore.Database", DbLoggerCategory.Database.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.Command", DbLoggerCategory.Database.Command.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.Connection", DbLoggerCategory.Database.Connection.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.Transaction", DbLoggerCategory.Database.Transaction.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Infrastructure", DbLoggerCategory.Infrastructure.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Migrations", DbLoggerCategory.Migrations.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Scaffolding", DbLoggerCategory.Scaffolding.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Model", DbLoggerCategory.Model.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Model.Validation", DbLoggerCategory.Model.Validation.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Query", DbLoggerCategory.Query.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Update", DbLoggerCategory.Update.Name);
        }

        [Fact]
        public void DbLoggerCategory_instances_generate_the_correct_names()
        {
            Assert.Equal(DbLoggerCategory.Database.Name, new DbLoggerCategory.Database());
            Assert.Equal(DbLoggerCategory.Database.Command.Name, new DbLoggerCategory.Database.Command());
            Assert.Equal(DbLoggerCategory.Database.Connection.Name, new DbLoggerCategory.Database.Connection());
            Assert.Equal(DbLoggerCategory.Database.Transaction.Name, new DbLoggerCategory.Database.Transaction());
            Assert.Equal(DbLoggerCategory.Infrastructure.Name, new DbLoggerCategory.Infrastructure());
            Assert.Equal(DbLoggerCategory.Migrations.Name, new DbLoggerCategory.Migrations());
            Assert.Equal(DbLoggerCategory.Scaffolding.Name, new DbLoggerCategory.Scaffolding());
            Assert.Equal(DbLoggerCategory.Model.Name, new DbLoggerCategory.Model());
            Assert.Equal(DbLoggerCategory.Model.Validation.Name, new DbLoggerCategory.Model.Validation());
            Assert.Equal(DbLoggerCategory.Query.Name, new DbLoggerCategory.Query());
            Assert.Equal(DbLoggerCategory.Update.Name, new DbLoggerCategory.Update());
        }
    }
}
