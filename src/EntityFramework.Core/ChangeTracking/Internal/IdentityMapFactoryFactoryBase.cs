// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class IdentityMapFactoryFactoryBase
    {
        protected virtual Type GetKeyType([NotNull] IKey key)
        {
            var keyType = key.Properties.First().ClrType;
            return key.Properties.Count > 1
                   || typeof(IStructuralEquatable).GetTypeInfo().IsAssignableFrom(keyType.GetTypeInfo())
                ? typeof(IKeyValue)
                : keyType;
        }
    }
}
