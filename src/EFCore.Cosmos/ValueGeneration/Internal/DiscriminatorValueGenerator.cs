// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal
{
    public class DiscriminatorValueGenerator : ValueGenerator
    {
        private readonly object _discriminator;

        public DiscriminatorValueGenerator(object discriminator)
        {
            _discriminator = discriminator;
        }

        protected override object NextValue(EntityEntry entry) => _discriminator;

        public override bool GeneratesTemporaryValues => false;
    }
}
