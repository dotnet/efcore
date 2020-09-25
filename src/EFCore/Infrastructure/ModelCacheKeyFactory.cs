// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <inheritdoc />
    public class ModelCacheKeyFactory : IModelCacheKeyFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelCacheKeyFactory" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ModelCacheKeyFactory([NotNull] ModelCacheKeyFactoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        /// <inheritdoc />
        public virtual object Create(DbContext context)
            => new ModelCacheKey(context);
    }
}
