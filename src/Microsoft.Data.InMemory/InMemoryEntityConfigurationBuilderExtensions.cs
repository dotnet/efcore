// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public static class InMemoryEntityConfigurationBuilderExtensions
    {
        public static EntityConfigurationBuilder UseInMemoryStore(
            [NotNull] this EntityConfigurationBuilder builder, bool persist = true)
        {
            Check.NotNull(builder, "builder");

            builder.AddBuildAction(c => c.AddExtension(new InMemoryConfigurationExtension { Persist = persist }));

            return builder;
        }
    }
}
