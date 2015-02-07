// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalTestHelpers : TestHelpers
    {
        protected RelationalTestHelpers()
        {
        }

        public new static RelationalTestHelpers Instance { get; } = new RelationalTestHelpers();

        protected override EntityServicesBuilder AddProviderServices(EntityServicesBuilder entityServicesBuilder)
        {
            return entityServicesBuilder.AddRelational();
        }

        protected override DbContextOptions UseProviderOptions(DbContextOptions options)
        {
            return options;
        }
    }
}
