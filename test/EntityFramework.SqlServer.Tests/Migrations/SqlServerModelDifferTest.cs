// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Migrations
{
    public class SqlServerModelDifferTest : ModelDifferTestBase
    {
        [Fact]
        public void Can_use_provider_overrides()
        {
            Execute(
                _ => { },
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "Person",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.ForSqlServer().Table("People");
                        });
                },
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var addTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                    Assert.Equal("People", addTableOperation.Name);
                });
        }

        protected override ModelBuilder CreateModelBuilder() => SqlServerTestHelpers.Instance.CreateConventionBuilder();
        protected override ModelDiffer CreateModelDiffer()
            => new SqlServerModelDiffer(new SqlServerTypeMapper(), new SqlServerMetadataExtensionProvider());
    }
}
