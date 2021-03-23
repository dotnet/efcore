// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

[assembly: DesignTimeProviderServices("Microsoft.EntityFrameworkCore.InMemory.Design.Internal.InMemoryDesignTimeServices")]

namespace Microsoft.EntityFrameworkCore.InMemory.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InMemoryDesignTimeServices : IDesignTimeServices
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.AddEntityFrameworkInMemoryDatabase();
            new EntityFrameworkDesignServicesBuilder(serviceCollection)
                .TryAdd<ICSharpSlimAnnotationCodeGenerator, InMemoryCSharpSlimAnnotationCodeGenerator>()
                .TryAddCoreServices();
        }
    }
}
