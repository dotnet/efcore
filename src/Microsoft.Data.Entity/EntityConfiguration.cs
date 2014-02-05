// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.AspNet.DependencyInjection;
    using Microsoft.Data.Entity.Resources;
    using Microsoft.Data.Entity.Utilities;

    public class EntityConfiguration
    {
        private readonly IServiceProvider _serviceProvider;

        private DataStore _dataStore;

        public EntityConfiguration()
            : this(EntityServices.CreateDefaultProvider())
        {
        }

        public EntityConfiguration([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            _serviceProvider = serviceProvider;
        }

        public virtual DataStore DataStore
        {
            get
            {
                return _dataStore
                       ?? _serviceProvider.GetService<DataStore>()
                       ?? ThrowNotConfigured<DataStore>();
            }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _dataStore = value;
            }
        }

        public virtual EntityContext CreateContext()
        {
            return new EntityContext(this);
        }

        private static T ThrowNotConfigured<T>(string propertyName = null)
        {
            throw new InvalidOperationException(
                Strings.MissingConfigurationItem(propertyName ?? typeof(T).Name));
        }
    }
}
