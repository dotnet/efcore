// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IEntityKeyFactorySource
    {
        EntityKeyFactory GetKeyFactory([NotNull] IReadOnlyList<IProperty> keyProperties);
    }
}
