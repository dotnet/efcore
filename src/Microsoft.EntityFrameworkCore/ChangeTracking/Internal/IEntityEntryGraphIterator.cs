// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public interface IEntityEntryGraphIterator
    {
        void TraverseGraph([NotNull] EntityEntryGraphNode node, [NotNull] Func<EntityEntryGraphNode, bool> handleNode);
    }
}
