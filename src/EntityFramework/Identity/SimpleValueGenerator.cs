// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public abstract class SimpleValueGenerator : IValueGenerator
    {
        public abstract object Next(StateEntry entry, IProperty property);

        public virtual Task<object> NextAsync(
            StateEntry stateEntry,
            IProperty property,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            return Task.FromResult(Next(stateEntry, property));
        }
    }
}
