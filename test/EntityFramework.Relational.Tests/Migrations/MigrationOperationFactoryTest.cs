// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations
{
    public class MigrationOperationFactoryTest
    {
        [Fact]
        public void CreateTableOperation_columns_are_ordered_by_name_with_pk_columns_first_and_fk_columns_last()
        {
            var modelBuider = new BasicModelBuilder();
            modelBuider.Entity("A",
                b =>
                    {
                        b.Property<int>("Px");
                        b.Property<int>("Py");
                        b.Key("Px", "Py");
                    });
            modelBuider.Entity("B",
                b =>
                    {
                        b.Property<int>("P6");
                        b.Property<int>("P5");
                        b.Property<int>("P4");
                        b.Property<int>("P3");
                        b.Property<int>("P2");
                        b.Property<int>("P1");
                        b.Key("P5", "P2");
                        b.ForeignKey("A", "P6", "P4");
                        b.ForeignKey("A", "P4", "P5");
                    });

            var createTableOperationA = OperationFactory().CreateTableOperation(modelBuider.Entity("A").Metadata);
            var createTableOperationB = OperationFactory().CreateTableOperation(modelBuider.Entity("B").Metadata);

            Assert.Equal(new[] { "Px", "Py" }, createTableOperationA.Columns.Select(c => c.Name));
            Assert.Equal(new[] { "P5", "P2", "P1", "P3", "P6", "P4" }, createTableOperationB.Columns.Select(c => c.Name));
        }

        private static MigrationOperationFactory OperationFactory()
        {
            return new MigrationOperationFactory(RelationalTestHelpers.ExtensionProvider());
        }
    }
}
