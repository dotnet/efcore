// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.AspNet.DependencyInjection
{
    public class EntityServicesBuilder
    {
        private readonly IServiceCollection _serviceCollection;

        public EntityServicesBuilder([NotNull] IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, "serviceCollection");

            _serviceCollection = serviceCollection;
        }

        public virtual IServiceCollection ServiceCollection
        {
            get { return _serviceCollection; }
        }
    }
}
