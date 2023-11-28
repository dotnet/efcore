// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqlServerModelBuilderNonGenericTest : SqlServerModelBuilderTestBase
{
    public class SqlServerNonGenericNonRelationship : SqlServerNonRelationship
    {
        public SqlServerNonGenericNonRelationship(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericComplexType : SqlServerComplexType
    {
        public SqlServerNonGenericComplexType(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericInheritance : SqlServerInheritance
    {
        public SqlServerNonGenericInheritance(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericOneToMany : SqlServerOneToMany
    {
        public SqlServerNonGenericOneToMany(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericManyToOne : SqlServerManyToOne
    {
        public SqlServerNonGenericManyToOne(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericOneToOne : SqlServerOneToOne
    {
        public SqlServerNonGenericOneToOne(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericManyToMany : SqlServerManyToMany
    {
        public SqlServerNonGenericManyToMany(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class SqlServerNonGenericOwnedTypes : SqlServerOwnedTypes
    {
        public SqlServerNonGenericOwnedTypes(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }
}
