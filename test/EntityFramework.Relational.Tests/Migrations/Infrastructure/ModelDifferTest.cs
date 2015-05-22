// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public class ModelDifferTest : ModelDifferTestBase
    {
        [Fact]
        public void Model_differ_breaks_foreign_key_cycles()
        {
            Execute(
                _ => { },
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "First",
                        x =>
                        {
                            x.Property<int>("ID");
                            x.Key("ID");
                            x.Property<int>("FK");
                        });

                    modelBuilder.Entity(
                        "Second",
                        x =>
                        {
                            x.Property<int>("ID");
                            x.Key("ID");
                            x.Property<int>("FK");
                        });

                    modelBuilder.Entity("First").Reference("Second").InverseCollection().ForeignKey("FK").PrincipalKey("ID");
                    modelBuilder.Entity("Second").Reference("First").InverseCollection().ForeignKey("FK").PrincipalKey("ID");
                },
                result =>
                {
                    Assert.Equal(3, result.Count);

                    var firstOperation = result[0] as CreateTableOperation;
                    var secondOperation = result[1] as CreateTableOperation;
                    var thirdOperation = result[2] as AddForeignKeyOperation;

                    Assert.NotNull(firstOperation);
                    Assert.NotNull(secondOperation);
                    Assert.NotNull(thirdOperation);

                    Assert.Equal(0, firstOperation.ForeignKeys.Count);
                    Assert.Equal(1, secondOperation.ForeignKeys.Count);
                    Assert.Equal(firstOperation.Name, thirdOperation.Table);
                });
        }
    }
}
