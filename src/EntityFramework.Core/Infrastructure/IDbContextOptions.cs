// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IDbContextOptions
    {
        DbContextOptions Clone();

        DbContextOptions UseModel([NotNull] IModel model);

        [CanBeNull]
        IModel Model { get; }

        void AddOrUpdateExtension<TExtension>([NotNull] Action<TExtension> updater)
            where TExtension : DbContextOptionsExtension, new();

        void AddExtension([NotNull] DbContextOptionsExtension extension);

        IReadOnlyList<DbContextOptionsExtension> Extensions { get; }

        IReadOnlyDictionary<string, string> RawOptions { get; [param: NotNull] set; }
    }
}
