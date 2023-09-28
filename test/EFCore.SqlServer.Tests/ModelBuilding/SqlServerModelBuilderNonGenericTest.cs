// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqlServerModelBuilderNonGenericTest : SqlServerModelBuilderTestBase
{
    public class SqlServerNonGenericNonRelationship : SqlServerNonRelationship
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericInheritance : SqlServerInheritance
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericOneToMany : SqlServerOneToMany
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericManyToOne : SqlServerManyToOne
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericOneToOne : SqlServerOneToOne
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericManyToMany : SqlServerManyToMany
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerNonGenericOwnedTypes : SqlServerOwnedTypes
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderNonGenericTest.NonGenericTestModelBuilder(testHelpers, configure);
    }
}
