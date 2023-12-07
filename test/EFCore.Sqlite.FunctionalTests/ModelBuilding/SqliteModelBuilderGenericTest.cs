// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqliteModelBuilderGenericTest : SqliteModelBuilderTestBase
{
    public class SqlServerGenericNonRelationship : SqliteNonRelationship
    {
        public SqlServerGenericNonRelationship(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericComplexType : SqliteComplexType
    {
        public SqlServerGenericComplexType(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericInheritance : SqliteInheritance
    {
        public SqlServerGenericInheritance(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericOneToMany : SqliteOneToMany
    {
        public SqlServerGenericOneToMany(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericManyToOne : SqliteManyToOne
    {
        public SqlServerGenericManyToOne(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericOneToOne : SqliteOneToOne
    {
        public SqlServerGenericOneToOne(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericManyToMany : SqliteManyToMany
    {
        public SqlServerGenericManyToMany(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericOwnedTypes : SqliteOwnedTypes
    {
        public SqlServerGenericOwnedTypes(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }
}
