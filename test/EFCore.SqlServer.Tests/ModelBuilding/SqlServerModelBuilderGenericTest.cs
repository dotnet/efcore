// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqlServerModelBuilderGenericTest : SqlServerModelBuilderTestBase
{
    public class SqlServerGenericNonRelationship : SqlServerNonRelationship, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerGenericNonRelationship(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericComplexType : SqlServerComplexType, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerGenericComplexType(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericInheritance : SqlServerInheritance, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerGenericInheritance(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericOneToMany : SqlServerOneToMany, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerGenericOneToMany(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericManyToOne : SqlServerManyToOne, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerGenericManyToOne(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericOneToOne : SqlServerOneToOne, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerGenericOneToOne(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericManyToMany : SqlServerManyToMany, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerGenericManyToMany(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericOwnedTypes : SqlServerOwnedTypes, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerGenericOwnedTypes(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }
}
