// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqlServerModelBuilderGenericTest : SqlServerModelBuilderTestBase
{
    public class SqlServerGenericNonRelationship : SqlServerNonRelationship
    {
        public SqlServerGenericNonRelationship(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericComplexType : SqlServerComplexType
    {
        public SqlServerGenericComplexType(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericInheritance : SqlServerInheritance
    {
        public SqlServerGenericInheritance(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericOneToMany : SqlServerOneToMany
    {
        public SqlServerGenericOneToMany(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericManyToOne : SqlServerManyToOne
    {
        public SqlServerGenericManyToOne(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericOneToOne : SqlServerOneToOne
    {
        public SqlServerGenericOneToOne(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericManyToMany : SqlServerManyToMany
    {
        public SqlServerGenericManyToMany(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerGenericOwnedTypes : SqlServerOwnedTypes
    {
        public SqlServerGenericOwnedTypes(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }
}
