// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class RelationalServiceCollectionExtensionsTestBase : EntityFrameworkServiceCollectionExtensionsTestBase
{
    protected RelationalServiceCollectionExtensionsTestBase(TestHelpers testHelpers)
        : base(testHelpers)
    {
    }

    public override void Required_services_are_registered_with_expected_lifetimes()
        => LifetimeTest(EntityFrameworkServicesBuilder.CoreServices, EntityFrameworkRelationalServicesBuilder.RelationalServices);
}
