// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.InMemory.Utilities;

namespace Microsoft.Data.Entity
{
    public static class InMemoryEntityConfigurationBuilderExtensions
    {
        public static DbContextOptions UseInMemoryStore(
            [NotNull] this DbContextOptions builder, bool persist = true)
        {
            Check.NotNull(builder, "builder");

            builder.AddBuildAction(c => c.AddOrUpdateExtension<InMemoryConfigurationExtension>(x => x.Persist = persist));

            return builder;
        }
    }
}
