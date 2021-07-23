// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#nullable enable

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

        public abstract class TestCheckConstraintBuilder
        {
            public abstract TestCheckConstraintBuilder HasName(string name);
        }

        public class NonGenericTestCheckConstraintBuilder : TestCheckConstraintBuilder, IInfrastructure<CheckConstraintBuilder>
        {
            public NonGenericTestCheckConstraintBuilder(CheckConstraintBuilder checkConstraintBuilder)
            {
                CheckConstraintBuilder = checkConstraintBuilder;
            }

            protected CheckConstraintBuilder CheckConstraintBuilder { get; }

            public CheckConstraintBuilder Instance => CheckConstraintBuilder;

            protected virtual TestCheckConstraintBuilder Wrap(CheckConstraintBuilder checkConstraintBuilder)
                => new NonGenericTestCheckConstraintBuilder(checkConstraintBuilder);

            public override TestCheckConstraintBuilder HasName(string name)
                => Wrap(CheckConstraintBuilder.HasName(name));
        }
    }
}
