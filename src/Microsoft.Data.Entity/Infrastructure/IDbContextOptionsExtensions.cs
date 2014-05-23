// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IDbContextOptionsExtensions
    {
        void AddOrUpdateExtension<TExtension>([NotNull] Action<TExtension> updater, [CanBeNull] [CallerMemberName] string memberName = "")
            where TExtension : DbContextOptionsExtension, new();

        IReadOnlyList<DbContextOptionsExtension> Extensions { get; }
    }
}
