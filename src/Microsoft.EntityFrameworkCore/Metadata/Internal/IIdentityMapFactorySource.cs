// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Query.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public interface IIdentityMapFactorySource
    {
        Func<IIdentityMap> IdentityMapFactory { get; }
        Func<IWeakReferenceIdentityMap> WeakReferenceIdentityMapFactory { get; }
    }
}
