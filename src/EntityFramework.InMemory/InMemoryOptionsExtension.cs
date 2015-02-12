// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryOptionsExtension : DbContextOptionsExtension
    {
        public virtual bool Persist { get; internal set; }

        protected override void ApplyServices(EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.AddInMemoryStore();
        }
    }
}
