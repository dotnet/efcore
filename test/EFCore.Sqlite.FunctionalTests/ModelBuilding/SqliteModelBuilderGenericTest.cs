// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqliteModelBuilderGenericTest : SqliteModelBuilderTestBase
{
    public class SqlServerGenericNonRelationship(SqliteModelBuilderFixture fixture) : SqliteNonRelationship(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericComplexType(SqliteModelBuilderFixture fixture) : SqliteComplexType(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericInheritance(SqliteModelBuilderFixture fixture) : SqliteInheritance(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericOneToMany(SqliteModelBuilderFixture fixture) : SqliteOneToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericManyToOne(SqliteModelBuilderFixture fixture) : SqliteManyToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericOneToOne(SqliteModelBuilderFixture fixture) : SqliteOneToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericManyToMany(SqliteModelBuilderFixture fixture) : SqliteManyToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericOwnedTypes(SqliteModelBuilderFixture fixture) : SqliteOwnedTypes(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }
}
