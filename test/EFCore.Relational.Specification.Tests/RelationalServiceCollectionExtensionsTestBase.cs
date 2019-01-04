// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class RelationalServiceCollectionExtensionsTestBase : EntityFrameworkServiceCollectionExtensionsTestBase
    {
        protected RelationalServiceCollectionExtensionsTestBase(TestHelpers testHelpers)
            : base(testHelpers)
        {
        }

        public override void Required_services_are_registered_with_expected_lifetimes()
        {
            LifetimeTest(EntityFrameworkServicesBuilder.CoreServices, EntityFrameworkRelationalServicesBuilder.RelationalServices);
        }
    }
}
