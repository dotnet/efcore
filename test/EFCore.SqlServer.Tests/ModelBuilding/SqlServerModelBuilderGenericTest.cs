// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqlServerModelBuilderGenericTest : SqlServerModelBuilderTestBase
{
    public class SqlServerGenericNonRelationship : SqlServerNonRelationship
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericInheritance : SqlServerInheritance
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericOneToMany : SqlServerOneToMany
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericManyToOne : SqlServerManyToOne
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericOneToOne : SqlServerOneToOne
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericManyToMany : SqlServerManyToMany
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }

    public class SqlServerGenericOwnedTypes : SqlServerOwnedTypes
    {
        protected override TestModelBuilder CreateTestModelBuilder(
            TestHelpers testHelpers,
            Action<ModelConfigurationBuilder>? configure)
            => new ModelBuilderGenericTest.GenericTestModelBuilder(testHelpers, configure);
    }
}
