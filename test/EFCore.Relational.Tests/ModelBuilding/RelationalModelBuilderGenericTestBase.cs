// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class RelationalModelBuilderTest : ModelBuilderTest
    {
        public abstract class TestTableBuilder<TEntity>
            where TEntity : class
        {
            public abstract TestTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true);
        }

        public class GenericTestTableBuilder<TEntity> : TestTableBuilder<TEntity>, IInfrastructure<TableBuilder<TEntity>>
            where TEntity : class
        {
            public GenericTestTableBuilder(TableBuilder<TEntity> tableBuilder)
            {
                TableBuilder = tableBuilder;
            }

            protected TableBuilder<TEntity> TableBuilder { get; }

            public TableBuilder<TEntity> Instance => TableBuilder;

            protected virtual TestTableBuilder<TEntity> Wrap(TableBuilder<TEntity> tableBuilder)
                => new GenericTestTableBuilder<TEntity>(tableBuilder);

            public override TestTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true)
                => Wrap(TableBuilder.ExcludeFromMigrations(excluded));
        }

        public class NonGenericTestTableBuilder<TEntity> : TestTableBuilder<TEntity>, IInfrastructure<TableBuilder>
            where TEntity : class
        {
            public NonGenericTestTableBuilder(TableBuilder tableBuilder)
            {
                TableBuilder = tableBuilder;
            }

            protected TableBuilder TableBuilder { get; }

            public TableBuilder Instance => TableBuilder;

            protected virtual TestTableBuilder<TEntity> Wrap(TableBuilder tableBuilder)
                => new NonGenericTestTableBuilder<TEntity>(tableBuilder);

            public override TestTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true)
                => Wrap(TableBuilder.ExcludeFromMigrations(excluded));
        }
    }
}
