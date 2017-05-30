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
            Assert.Equal("Microsoft.EntityFrameworkCore.Database", EF.LoggerCategories.Database.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.Command", EF.LoggerCategories.Database.Command.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.Connection", EF.LoggerCategories.Database.Connection.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Database.Transaction", EF.LoggerCategories.Database.Transaction.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Infrastructure", EF.LoggerCategories.Infrastructure.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Migrations", EF.LoggerCategories.Migrations.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Scaffolding", EF.LoggerCategories.Scaffolding.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Model", EF.LoggerCategories.Model.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Model.Validation", EF.LoggerCategories.Model.Validation.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Query", EF.LoggerCategories.Query.Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Update", EF.LoggerCategories.Update.Name);
        }

        [Fact]
        public void DbLoggerCategory_instances_generate_the_correct_names()
        {
            Assert.Equal(EF.LoggerCategories.Database.Name, new EF.LoggerCategories.Database());
            Assert.Equal(EF.LoggerCategories.Database.Command.Name, new EF.LoggerCategories.Database.Command());
            Assert.Equal(EF.LoggerCategories.Database.Connection.Name, new EF.LoggerCategories.Database.Connection());
            Assert.Equal(EF.LoggerCategories.Database.Transaction.Name, new EF.LoggerCategories.Database.Transaction());
            Assert.Equal(EF.LoggerCategories.Infrastructure.Name, new EF.LoggerCategories.Infrastructure());
            Assert.Equal(EF.LoggerCategories.Migrations.Name, new EF.LoggerCategories.Migrations());
            Assert.Equal(EF.LoggerCategories.Scaffolding.Name, new EF.LoggerCategories.Scaffolding());
            Assert.Equal(EF.LoggerCategories.Model.Name, new EF.LoggerCategories.Model());
            Assert.Equal(EF.LoggerCategories.Model.Validation.Name, new EF.LoggerCategories.Model.Validation());
            Assert.Equal(EF.LoggerCategories.Query.Name, new EF.LoggerCategories.Query());
            Assert.Equal(EF.LoggerCategories.Update.Name, new EF.LoggerCategories.Update());
        }
    }
}
