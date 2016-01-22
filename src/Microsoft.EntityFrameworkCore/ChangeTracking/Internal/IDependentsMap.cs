// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public interface IDependentsMap
    {
        void Add([NotNull] InternalEntityEntry entry);
        void Update([NotNull] InternalEntityEntry entry);
        void Remove([NotNull] InternalEntityEntry entry);
        IEnumerable<InternalEntityEntry> GetDependents([NotNull] InternalEntityEntry principalEntry);
        IEnumerable<InternalEntityEntry> GetDependentsUsingRelationshipSnapshot([NotNull] InternalEntityEntry principalEntry);
    }
}
