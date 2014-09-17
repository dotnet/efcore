// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Identity
{
    public interface IValueGenerator
    {
        void Next([NotNull] StateEntry stateEntry, [NotNull] IProperty property);

        Task NextAsync(
            [NotNull] StateEntry stateEntry,
            [NotNull] IProperty property,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
