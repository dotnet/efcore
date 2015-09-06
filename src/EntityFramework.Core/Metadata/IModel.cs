// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IModel : IAnnotatable
    {
        IEntityType FindEntityType([NotNull] Type type);

        IEntityType GetEntityType([NotNull] Type type);

        IReadOnlyList<IEntityType> EntityTypes { get; }

        IEntityType FindEntityType([NotNull] string name);

        IEntityType GetEntityType([NotNull] string name);
    }
}
