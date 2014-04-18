// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public static class InMemoryDataStoreEntityConfigurationBuilderExtensions
    {
        public static EntityConfigurationBuilder UseInMemoryStore(
            [NotNull] this EntityConfigurationBuilder builder, bool persist = false)
        {
            Check.NotNull(builder, "builder");

            builder.SetMode(persist);

            return builder;
        }

        public static EntityConfigurationBuilder UseInMemoryStore(
            [NotNull] this EntityConfigurationBuilder builder, [NotNull] string name, bool persist = false)
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(name, "name");

            builder.Annotations[typeof(InMemoryDataStore)][InMemoryDataStore.NameKey] = name;

            builder.SetMode(persist);

            return builder;
        }

        private static void SetMode(this EntityConfigurationBuilder builder, bool persist)
        {
            builder.Annotations[typeof(InMemoryDataStore)][InMemoryDataStore.ModeKey] = persist
                ? InMemoryDataStore.PersistentMode
                : InMemoryDataStore.TransientMode;
        }
    }
}
