// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqlServerModelBuilderNonGenericTest : SqlServerModelBuilderTestBase
{
    public class SqlServerNonGenericNonRelationship(SqlServerModelBuilderFixture fixture) : SqlServerNonRelationship(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericComplexType(SqlServerModelBuilderFixture fixture) : SqlServerComplexType(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericInheritance(SqlServerModelBuilderFixture fixture) : SqlServerInheritance(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericOneToMany(SqlServerModelBuilderFixture fixture) : SqlServerOneToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericManyToOne(SqlServerModelBuilderFixture fixture) : SqlServerManyToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericOneToOne(SqlServerModelBuilderFixture fixture) : SqlServerOneToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericManyToMany(SqlServerModelBuilderFixture fixture) : SqlServerManyToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericOwnedTypes(SqlServerModelBuilderFixture fixture) : SqlServerOwnedTypes(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }
}
