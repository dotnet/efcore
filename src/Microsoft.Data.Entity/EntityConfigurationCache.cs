// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    internal class EntityConfigurationCache
    {
        private static readonly EntityConfigurationCache _instance = new EntityConfigurationCache();

        private readonly ThreadSafeDictionaryCache<Type, EntityConfiguration> _configurations
            = new ThreadSafeDictionaryCache<Type, EntityConfiguration>();

        public static EntityConfigurationCache Instance
        {
            get { return _instance; }
        }

        public virtual EntityConfiguration GetOrAddConfiguration([NotNull] EntityContext context)
        {
            Check.NotNull(context, "context");

            return _configurations.GetOrAdd(context.GetType(), k => CreateConfiguration(context));
        }

        private static EntityConfiguration CreateConfiguration(EntityContext context)
        {
            var builder = new EntityConfigurationBuilder();
            context.CallOnConfiguring(builder);
            return builder.BuildConfiguration();
        }
    }
}
