// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class RelationalServiceCollectionExtensionsTestBase(TestHelpers testHelpers)
    : EntityFrameworkServiceCollectionExtensionsTestBase(testHelpers)
{
    public override void Required_services_are_registered_with_expected_lifetimes()
        => LifetimeTest(EntityFrameworkServicesBuilder.CoreServices, EntityFrameworkRelationalServicesBuilder.RelationalServices);
}
