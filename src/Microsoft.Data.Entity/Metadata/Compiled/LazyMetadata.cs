// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public static class LazyMetadata
    {
        public static T Init<T>([CanBeNull] ref T target) where T : class, new()
        {
            return LazyInitializer.EnsureInitialized(ref target, () => new T());
        }
    }
}
