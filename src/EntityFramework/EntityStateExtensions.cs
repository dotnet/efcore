// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity
{
    public static class EntityStateExtensions
    {
        public static bool IsDirty(this EntityState state)
        {
            return state == EntityState.Added || state == EntityState.Modified || state == EntityState.Deleted;
        }
    }
}
