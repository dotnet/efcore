// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public abstract class IdentityMapFactoryFactoryBase
    {
        protected virtual Type GetKeyType([NotNull] IKey key)
            => key.Properties.Count > 1 ? typeof(object[]) : key.Properties.First().ClrType;
    }
}
