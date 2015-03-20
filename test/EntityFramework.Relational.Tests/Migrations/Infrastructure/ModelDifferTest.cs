// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.Infrastructure
{
    public class ModelDifferTest
    {
        [Fact]
        public void Model_differ_breaks_foreign_key_cycles()
        {
            var model = new Entity.Metadata.Model();

            var firstType = model.AddEntityType("First");
            var firstKey = firstType.SetPrimaryKey(firstType.AddProperty("ID", typeof(int), true));
            var firstFk = firstType.AddProperty("FK", typeof(int), true);

            var secondType = model.AddEntityType("Second");
            var secondKey = secondType.SetPrimaryKey(secondType.AddProperty("ID", typeof(int), true));
            var secondFk = secondType.AddProperty("FK", typeof(int), true);

            firstType.AddForeignKey(firstFk, secondKey);
            secondType.AddForeignKey(secondFk, firstKey);

            var modelDiffer = new ModelDiffer(new RelationalTypeMapper());

            var result = modelDiffer.GetDifferences(null, model);

            Assert.Equal(3, result.Count);

            var firstOperation = result[0] as CreateTableOperation;
            var secondOperation = result[1] as CreateTableOperation;
            var thirdOperation = result[2] as AddForeignKeyOperation;

            Assert.NotNull(firstOperation);
            Assert.NotNull(secondOperation);
            Assert.NotNull(thirdOperation);

            Assert.Equal(0, firstOperation.ForeignKeys.Count);
            Assert.Equal(1, secondOperation.ForeignKeys.Count);
            Assert.Equal(firstOperation.Name, thirdOperation.DependentTable);
        }
    }
}
