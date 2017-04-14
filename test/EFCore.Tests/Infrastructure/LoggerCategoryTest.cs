// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Infrastructure
{
    public class LoggerCategoryTest
    {
        [Fact]
        public void Logger_categories_have_the_correct_names()
        {
            Assert.Equal("Microsoft.EntityFrameworkCore.Database", LoggerCategory.Database.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.Sql", LoggerCategory.Database.Sql.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.Connection", LoggerCategory.Database.Connection.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.DataReader", LoggerCategory.Database.DataReader.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.Transaction", LoggerCategory.Database.Transaction.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Infrastructure", LoggerCategory.Infrastructure.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Migrations", LoggerCategory.Migrations.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Scaffolding", LoggerCategory.Scaffolding.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Model", LoggerCategory.Model.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Model.Validation", LoggerCategory.Model.Validation.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Query", LoggerCategory.Query.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Update", LoggerCategory.Update.Name);
        }

        [Fact]
        public void LoggerCategory_instances_generate_the_correct_names()
        {
            Assert.Equal(LoggerCategory.Database.Name, new LoggerCategory.Database());
            Assert.Equal(LoggerCategory.Database.Sql.Name, new LoggerCategory.Database.Sql());
            Assert.Equal(LoggerCategory.Database.Connection.Name, new LoggerCategory.Database.Connection());
            Assert.Equal(LoggerCategory.Database.DataReader.Name, new LoggerCategory.Database.DataReader());
            Assert.Equal(LoggerCategory.Database.Transaction.Name, new LoggerCategory.Database.Transaction());
            Assert.Equal(LoggerCategory.Infrastructure.Name, new LoggerCategory.Infrastructure());
            Assert.Equal(LoggerCategory.Migrations.Name, new LoggerCategory.Migrations());
            Assert.Equal(LoggerCategory.Scaffolding.Name, new LoggerCategory.Scaffolding());
            Assert.Equal(LoggerCategory.Model.Name, new LoggerCategory.Model());
            Assert.Equal(LoggerCategory.Model.Validation.Name, new LoggerCategory.Model.Validation());
            Assert.Equal(LoggerCategory.Query.Name, new LoggerCategory.Query());
            Assert.Equal(LoggerCategory.Update.Name, new LoggerCategory.Update());
        }
    }
}
