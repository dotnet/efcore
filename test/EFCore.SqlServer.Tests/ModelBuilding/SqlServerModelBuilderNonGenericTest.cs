// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqlServerModelBuilderNonGenericTest : SqlServerModelBuilderTestBase
{
    public class SqlServerNonGenericNonRelationship : SqlServerNonRelationship, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerNonGenericNonRelationship(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericComplexType : SqlServerComplexType, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerNonGenericComplexType(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericInheritance : SqlServerInheritance, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerNonGenericInheritance(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericOneToMany : SqlServerOneToMany, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerNonGenericOneToMany(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericManyToOne : SqlServerManyToOne, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerNonGenericManyToOne(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericOneToOne : SqlServerOneToOne, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerNonGenericOneToOne(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericManyToMany : SqlServerManyToMany, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerNonGenericManyToMany(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericOwnedTypes : SqlServerOwnedTypes, IClassFixture<SqlServerModelBuilderFixture>
    {
        public SqlServerNonGenericOwnedTypes(SqlServerModelBuilderFixture fixture)
            : base(fixture)
        {
        }

        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }
}
