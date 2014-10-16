// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity
{
    public class EntityServicesBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

        public EntityServicesBuilder(
            [NotNull] IServiceCollection serviceCollection, 
            [CanBeNull] IConfiguration configuration = null)
        {
            Check.NotNull(serviceCollection, "serviceCollection");

            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        public virtual IServiceCollection ServiceCollection
        {
            get { return _serviceCollection; }
        }

        public virtual IConfiguration Configuration
        {
            get { return _configuration; }
        }
    }
}
