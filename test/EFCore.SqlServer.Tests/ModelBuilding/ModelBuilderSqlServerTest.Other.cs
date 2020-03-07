// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderSqlServerTest : ModelBuilderOtherTest
    {
        protected override DbContextOptions Configure()
            => new DbContextOptionsBuilder()
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .AddSingleton<IModelCacheKeyFactory, TestModelCacheKeyFactory>()
                        .BuildServiceProvider())
                .UseSqlServer("Database = None")
                .Options;

        protected override void RunThrowDifferPipeline(DbContext context)
            => context.GetService<IMigrationsModelDiffer>().GetDifferences(null, context.Model);
    }
}
