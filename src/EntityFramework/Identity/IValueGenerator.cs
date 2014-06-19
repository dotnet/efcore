// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Identity
{
    public interface IValueGenerator
    {
        object Next([NotNull] DbContextConfiguration contextConfiguration, [NotNull] IProperty property);

        Task<object> NextAsync(
            [NotNull] DbContextConfiguration contextConfiguration,
            [NotNull] IProperty property,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
