// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public abstract class SimpleValueGenerator : IValueGenerator
    {
        public abstract GeneratedValue Next([NotNull] IProperty property);

        public virtual GeneratedValue Next(IProperty property, ContextService<DataStoreServices> dataStoreServices)
        {
            Check.NotNull(property, "property");

            return Next(property);
        }

        public virtual Task<GeneratedValue> NextAsync(
            IProperty property,
            ContextService<DataStoreServices> dataStoreServices,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(property, "property");

            return Task.FromResult(Next(property));
        }
    }
}
