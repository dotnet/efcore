// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IChangeDetector : IPropertyListener
    {
        void DetectChanges([NotNull] IStateManager stateManager);
        void DetectChanges([NotNull] InternalEntityEntry entry);
    }
}
