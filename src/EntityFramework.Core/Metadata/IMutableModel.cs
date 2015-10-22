// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IMutableModel : IModel, IMutableAnnotatable
    {
        IMutableEntityType AddEntityType([NotNull] string name);
        IMutableEntityType GetOrAddEntityType([NotNull] string name);
        new IMutableEntityType FindEntityType([NotNull] string name);
        IMutableEntityType RemoveEntityType([NotNull] string name);
        new IReadOnlyList<IMutableEntityType> GetEntityTypes();
    }
}
